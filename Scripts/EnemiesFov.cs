using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyFieldOfView : MonoBehaviour
{
    [Header("Field of View Settings")]
    [Range(0f, 360f)]
    public float viewAngle = 90f;
    public float viewRadius = 10f;
    public LayerMask targetMask; // Set to player layer
    public LayerMask obstructionMask; // Set to walls/obstacles
    
    [Header("Detection Settings")]
    public string playerTag = "Player";
    public float detectionDelay = 0.1f;
    public float raycastHeightOffset = 0.5f; // Height from ground to cast ray
    
    [Header("Behavior")]
    public bool chaseWhenDetected = true;
    public float loseTargetTime = 3f;
    
    [Header("Debug")]
    public bool showDebugLogs = false;
    public bool showDebugGizmos = true;
    
    // Public properties
    public bool PlayerInFOV { get; private set; }
    public Transform CurrentTarget { get; private set; }
    
    private NavMeshAgent agent;
    private float lastSeenTime;
    private bool wasInFOV;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        PlayerInFOV = false;
        lastSeenTime = -loseTargetTime; // Initialize to allow immediate detection
        StartCoroutine(FOVRoutine());
    }
    
    void Update()
    {
        // Only chase if player is CURRENTLY in FOV (can see them now)
        if (PlayerInFOV && chaseWhenDetected && CurrentTarget != null && agent != null)
        {
            agent.SetDestination(CurrentTarget.position);
        }
        // Handle losing target after time
        else if (!PlayerInFOV && CurrentTarget != null)
        {
            if (Time.time - lastSeenTime > loseTargetTime)
            {
                if (showDebugLogs) Debug.Log($"{gameObject.name}: Lost target after {loseTargetTime}s");
                CurrentTarget = null;
                
                // Stop chasing
                if (agent != null && chaseWhenDetected)
                {
                    agent.ResetPath();
                }
            }
            else if (chaseWhenDetected && agent != null)
            {
                // Continue moving to last known position
                agent.SetDestination(CurrentTarget.position);
            }
        }
    }
    
    System.Collections.IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(detectionDelay);
        
        while (true)
        {
            yield return wait;
            CheckFieldOfView();
        }
    }
    
    void CheckFieldOfView()
    {
        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        
        PlayerInFOV = false;
        
        foreach (Collider col in targetsInRadius)
        {
            if (!col.CompareTag(playerTag))
                continue;
            
            Transform target = col.transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            
            // Use the enemy's forward direction (based on rotation)
            // This makes the FOV cone point in the direction the enemy is facing
            float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
            
            if (angleToTarget < viewAngle / 2f)
            {
                float distToTarget = Vector3.Distance(transform.position, target.position);
                
                // Cast ray from enemy's eye level to player's center
                Vector3 rayOrigin = transform.position + Vector3.up * raycastHeightOffset;
                Vector3 targetPoint = target.position + Vector3.up * raycastHeightOffset;
                Vector3 rayDirection = (targetPoint - rayOrigin).normalized;
                float rayDistance = Vector3.Distance(rayOrigin, targetPoint);
                
                // Debug ray
                if (showDebugGizmos)
                {
                    Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.red, detectionDelay);
                }
                
                // Check for obstructions - if raycast hits something on obstruction mask, player is blocked
                if (!Physics.Raycast(rayOrigin, rayDirection, rayDistance, obstructionMask))
                {
                    // Player is in FOV and not obstructed!
                    PlayerInFOV = true;
                    CurrentTarget = target;
                    lastSeenTime = Time.time;
                    
                    if (!wasInFOV && showDebugLogs)
                    {
                        Debug.Log($"{gameObject.name}: Player spotted at distance {distToTarget:0.00}m, angle {angleToTarget:0.0}°");
                    }
                    
                    wasInFOV = true;
                    return; // Found player, no need to check others
                }
                else if (showDebugLogs && wasInFOV)
                {
                    Debug.Log($"{gameObject.name}: Player blocked by obstacle");
                }
            }
        }
        
        // Player not in FOV this frame
        if (wasInFOV && showDebugLogs)
        {
            Debug.Log($"{gameObject.name}: Player left FOV");
        }
        wasInFOV = false;
    }
    
    public bool CanSeePlayer()
    {
        return PlayerInFOV;
    }
    
    public float TimeSinceLastSeen()
    {
        return Time.time - lastSeenTime;
    }
    
    // Visualize FOV in Scene view
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        
        // Draw FOV cone
        Vector3 viewAngleA = GetDirectionFromAngle(-viewAngle / 2f);
        Vector3 viewAngleB = GetDirectionFromAngle(viewAngle / 2f);
        
        Gizmos.color = PlayerInFOV ? Color.red : Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);
        
        // Draw forward direction
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * viewRadius);
        
        // Draw line to target if detected
        if (CurrentTarget != null)
        {
            Gizmos.color = PlayerInFOV ? Color.red : Color.yellow;
            Gizmos.DrawLine(transform.position + Vector3.up * raycastHeightOffset, 
                           CurrentTarget.position + Vector3.up * raycastHeightOffset);
        }
    }
    
    Vector3 GetDirectionFromAngle(float angleInDegrees)
    {
        // Apply the angle relative to the enemy's forward direction
        return Quaternion.Euler(0, angleInDegrees, 0) * transform.forward;
    }
}