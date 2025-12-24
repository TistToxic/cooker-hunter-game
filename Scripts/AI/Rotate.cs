using UnityEngine;
using UnityEngine.AI;

public class EnemyLookAround : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Maximum angle to rotate left or right from starting forward direction")]
    [Range(0f, 180f)]
    public float rotationAngle = 90f;

    [Tooltip("Time one complete rotation takes (seconds)")]
    [Range(0.1f, 5f)]
    public float rotationDuration = 1f;

    [Tooltip("Cooldown time after rotation before next chance (seconds)")]
    [Range(0f, 10f)]
    public float cooldownTime = 2f;

    [Tooltip("Chance to rotate when cooldown ends (0-100%)")]
    [Range(0f, 100f)]
    public float rotationChance = 50f;

    [Header("Behavior")]
    [Tooltip("Only look around when not chasing a target")]
    public bool stopWhenChasing = true;

    [Header("Debug")]
    public bool showDebugGizmos = false;

    [Header("Human Look Tuning")]
[Range(0f, 1f)]
public float extraLookChance = 0.35f;

[Range(5f, 40f)]
public float glanceAngleMin = 15f;

[Range(15f, 60f)]
public float glanceAngleMax = 35f;

[Range(10f, 40f)]
public float extraLookAngle = 20f;


    // Rotation state
    private float startingYRotation;
    private float currentYRotation;
    private float startAngle;
    private float targetAngle;

    private bool isRotating;
    private bool onCooldown = true;

    private float rotationProgress;
    private float cooldownTimer;

    private EnemyFieldOfView fovScript;
    private NavMeshAgent agent;

    private enum LookPhase
{
    None,
    Glance,
    ExtraLook,
    Return
}

private LookPhase lookPhase = LookPhase.None;

    void Start()
    {
        startingYRotation = transform.eulerAngles.y;
        currentYRotation = startingYRotation;

        fovScript = GetComponent<EnemyFieldOfView>();
        agent = GetComponent<NavMeshAgent>();

        // IMPORTANT: prevent NavMeshAgent from rotating the object
        if (agent != null)
        {
            agent.updateRotation = false;
        }

        cooldownTimer = cooldownTime;
    }

    void Update()
    {
        // Stop rotation when chasing
        if (stopWhenChasing && fovScript != null)
        {
            bool isChasing = fovScript.PlayerInFOV || fovScript.CurrentTarget != null;
            if (isChasing)
            {
                isRotating = false;
                onCooldown = true;
                cooldownTimer = cooldownTime;
                return;
            }
        }

        // Cooldown logic
        if (onCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                float roll = Random.Range(0f, 100f);
                if (roll <= rotationChance)
                {
                    StartRotation();
                }
                else
                {
                    cooldownTimer = cooldownTime;
                }
            }
            return;
        }

        // Rotation logic
if (isRotating)
{
    float angleDiff = Mathf.Abs(Mathf.DeltaAngle(startAngle, targetAngle));
    float durationMultiplier = Mathf.Lerp(0.6f, 1.4f, angleDiff / rotationAngle);
    float effectiveDuration = rotationDuration * durationMultiplier;

    rotationProgress += Time.deltaTime / effectiveDuration;
    float t = Mathf.Clamp01(rotationProgress);

    float smoothT = Mathf.SmoothStep(0f, 1f, t);
    float newY = Mathf.LerpAngle(startAngle, targetAngle, smoothT);

    transform.localRotation = Quaternion.Euler(0f, newY, 0f);
    currentYRotation = newY;

    if (t >= 1f)
    {
        AdvanceLookPhase();
    }
}


    }

void StartRotation()
{
    isRotating = true;
    onCooldown = false;
    rotationProgress = 0f;

    lookPhase = LookPhase.Glance;

    startAngle = startingYRotation;

    float direction = Random.value < 0.5f ? -1f : 1f;
    float glanceAmount = Random.Range(glanceAngleMin, glanceAngleMax);

    targetAngle = startingYRotation + glanceAmount * direction;

    float leftLimit = startingYRotation - rotationAngle;
    float rightLimit = startingYRotation + rotationAngle;
    targetAngle = Mathf.Clamp(targetAngle, leftLimit, rightLimit);
}



    public void ResetRotation()
    {
        currentYRotation = startingYRotation;
        transform.localRotation = Quaternion.Euler(0f, startingYRotation, 0f);

        isRotating = false;
        onCooldown = true;
        cooldownTimer = cooldownTime;
        rotationProgress = 0f;
    }

void OnDrawGizmos()
{
    if (!showDebugGizmos) return;

    Vector3 origin = transform.position + Vector3.up * 0.1f;
    int segments = 30;
    float halfAngle = rotationAngle;

    Gizmos.color = new Color(1f, 0f, 0f, 0.25f);

    Vector3 prevPoint = origin;
    for (int i = 0; i <= segments; i++)
    {
        float angle = -halfAngle + (i / (float)segments) * halfAngle * 2f;
        Vector3 dir = Quaternion.Euler(0, startingYRotation + angle, 0) * Vector3.forward;
        Vector3 point = origin + dir * 4f;

        if (i > 0)
            Gizmos.DrawLine(prevPoint, point);

        Gizmos.DrawLine(origin, point);
        prevPoint = point;
    }

    // Current facing direction
    Gizmos.color = Color.red;
    Gizmos.DrawRay(origin, transform.forward * 4.5f);
}


    void AdvanceLookPhase()
{
    rotationProgress = 0f;
    startAngle = currentYRotation;

    float leftLimit = startingYRotation - rotationAngle;
    float rightLimit = startingYRotation + rotationAngle;

    switch (lookPhase)
    {
        case LookPhase.Glance:
            if (Random.value < extraLookChance)
            {
                lookPhase = LookPhase.ExtraLook;

                float dir = Mathf.Sign(Mathf.DeltaAngle(startingYRotation, currentYRotation));
                targetAngle = currentYRotation + dir * extraLookAngle;
                targetAngle = Mathf.Clamp(targetAngle, leftLimit, rightLimit);
            }
            else
            {
                lookPhase = LookPhase.Return;
                targetAngle = startingYRotation;
            }
            break;

        case LookPhase.ExtraLook:
            lookPhase = LookPhase.Return;
            targetAngle = startingYRotation;
            break;

        case LookPhase.Return:
            lookPhase = LookPhase.None;
            isRotating = false;
            onCooldown = true;
            cooldownTimer = cooldownTime;
            break;
    }
}

}
