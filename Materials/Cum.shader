Shader "Custom/DynamicSplatterShader_URP"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        
        _Smoothness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        
        [Header(Splatter Settings)]
        _SplatterCount("Splatter Count", Int) = 0
        _NoiseScale("Noise Scale", Float) = 15.0
        _NoiseStrength("Noise Strength", Range(0, 1)) = 0.3
        _EdgeSoftness("Edge Softness", Range(0, 1)) = 0.2
        _SplatterBlend("Splatter Blend Mode", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 3.5

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                float fogCoord : TEXCOORD4;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 5);
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Smoothness;
                half _Metallic;
                
                int _SplatterCount;
                float _NoiseScale;
                float _NoiseStrength;
                float _EdgeSoftness;
                float _SplatterBlend;
            CBUFFER_END

            // Splatter data arrays (max 20)
            float4 _SplatterPositions[20];
            float4 _SplatterNormals[20];
            float _SplatterRadii[20];
            float4 _SplatterColors[20];
            float _SplatterIntensities[20];
            float _SplatterRoughness[20];
            float _SplatterRotations[20];

            // Procedural noise functions
            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.13);
                p3 += dot(p3, p3.yzx + 3.333);
                return frac((p3.x + p3.y) * p3.z);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                
                for(int i = 0; i < 4; i++)
                {
                    value += amplitude * noise(p * frequency);
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }
                
                return value;
            }

            float2 rotate2D(float2 p, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                return float2(p.x * c - p.y * s, p.x * s + p.y * c);
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.positionWS = vertexInput.positionWS;
                output.positionCS = vertexInput.positionCS;
                output.normalWS = normalInput.normalWS;
                output.shadowCoord = GetShadowCoord(vertexInput);
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);

                OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
                OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Sample base texture
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                
                // Initialize surface properties
                float3 finalAlbedo = baseColor.rgb;
                float finalSmoothness = _Smoothness;
                float finalMetallic = _Metallic;

                // Process splatters
                for(int i = 0; i < _SplatterCount && i < 20; i++)
                {
                    // Transform splatter to world space
                    float3 splatterWorldPos = TransformObjectToWorld(_SplatterPositions[i].xyz);
                    float3 splatterWorldNormal = normalize(TransformObjectToWorldDir(_SplatterNormals[i].xyz));
                    
                    // Calculate distance
                    float3 toSplatter = input.positionWS - splatterWorldPos;
                    float distToCenter = length(toSplatter);
                    
                    // Check normal alignment
                    float normalAlignment = dot(normalize(input.normalWS), splatterWorldNormal);
                    
                    if(normalAlignment > 0.5 && distToCenter < _SplatterRadii[i])
                    {
                        // Project onto splatter plane
                        float3 projected = toSplatter - splatterWorldNormal * dot(toSplatter, splatterWorldNormal);
                        float2 localPos = float2(length(projected.xz), projected.y) * 5.0;
                        
                        // Apply rotation
                        float angleRad = _SplatterRotations[i] * 0.0174533;
                        localPos = rotate2D(localPos, angleRad);
                        
                        // Radial distance
                        float radialDist = distToCenter / _SplatterRadii[i];
                        
                        // Generate noise
                        float noiseValue = fbm(localPos * _NoiseScale);
                        
                        // Create splatter mask
                        float splatterMask = 1.0 - radialDist;
                        splatterMask += (noiseValue - 0.5) * _NoiseStrength;
                        splatterMask = smoothstep(_EdgeSoftness, 1.0, splatterMask);
                        splatterMask *= _SplatterIntensities[i];
                        
                        // Blend color
                        finalAlbedo = lerp(finalAlbedo, _SplatterColors[i].rgb, splatterMask * _SplatterBlend);
                        
                        // Adjust surface properties
                        finalSmoothness = lerp(finalSmoothness, 1.0 - _SplatterRoughness[i], splatterMask);
                        finalMetallic = lerp(finalMetallic, 0.0, splatterMask);
                    }
                }

                // Setup surface data for lighting
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalize(input.normalWS);
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = input.shadowCoord;
                inputData.fogCoord = input.fogCoord;
                inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = finalAlbedo;
                surfaceData.metallic = finalMetallic;
                surfaceData.specular = half3(0.0, 0.0, 0.0);
                surfaceData.smoothness = finalSmoothness;
                surfaceData.normalTS = half3(0, 0, 1);
                surfaceData.emission = 0;
                surfaceData.occlusion = 1.0;
                surfaceData.alpha = 1.0;

                // Calculate lighting
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                
                return color;
            }
            ENDHLSL
        }

        // Shadow caster pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            float3 _LightDirection;

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }

        // DepthOnly pass
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}