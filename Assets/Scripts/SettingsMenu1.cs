using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Audio;

public class SettingsMenu1 : MonoBehaviour
{
    public AudioMixer mainMixer;

    public void SetVolume(float volume)
    {
        mainMixer.SetFloat("volume", volume);
    }

    public void SetFullScreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }
}
