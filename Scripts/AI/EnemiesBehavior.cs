using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyFieldOfView : MonoBehaviour
{
    [Header("Field of View Settings")]
    [Range(0f, 360f)]
    public float viewAngle = 90f;
    public float viewRadius = 10f;
    public LayerMask targetMask; 
    public LayerMask obstructionMask; 
    
    [Header("Detection Settings")]
    public string playerTag = "Player";
    public float detectionDelay = 0.1f;
    public float raycastHeightOffset = 0.5f; 
    
    [Header("Behavior")]
    public bool chaseWhenDetected = true;
    public float loseTargetTime = 3f;
    
    [Header("Commander Settings")]
    public bool isCommander = false;
    public EnemyFieldOfView[] commandedEnemies;
    
    [Header("Debug")]
    public bool showDebugLogs = false;
    public bool showDebugGizmos = true;
    

    public Vector3 LastSeenPosition => lastSeenPosition;


    public bool PlayerInFOV { get; private set; }
    public Transform CurrentTarget { get; private set; }
    
    private NavMeshAgent agent;
    private float lastSeenTime;
    private bool wasInFOV;
    private bool hasIssuedCommand = false;
    private Transform commandedTarget; 
    private float commandedTargetTime;
    private Vector3 lastSeenPosition;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        PlayerInFOV = false;
        lastSeenTime = -loseTargetTime; 
        StartCoroutine(FOVRoutine());
    }
    
void Update()
{
   
    if (isCommander && PlayerInFOV && CurrentTarget != null)
    {
        CommandNearbyEnemies(); 
    }
    

    if (isCommander && CurrentTarget == null)
    {
        hasIssuedCommand = false;
    }
    
   
    Transform targetToChase = commandedTarget != null ? commandedTarget : CurrentTarget;
    bool hasValidTarget = (PlayerInFOV || commandedTarget != null) && targetToChase != null;
    
    if (hasValidTarget && chaseWhenDetected && agent != null)
    {
        agent.SetDestination(targetToChase.position);
    }
    
    else if (!PlayerInFOV && CurrentTarget != null && commandedTarget == null)
    {
        if (Time.time - lastSeenTime > loseTargetTime)
        {
            if (showDebugLogs) Debug.Log($"{gameObject.name}: Lost target after {loseTargetTime}s");
            CurrentTarget = null;
            
            if (agent != null && chaseWhenDetected)
            {
                agent.ResetPath();
            }
        }
        else if (chaseWhenDetected && agent != null)
        {
            agent.SetDestination(CurrentTarget.position);
        }
    }
    
   
    if (commandedTarget != null)
    {
        bool commanderStillSeesPlayer = false;
        
        EnemyFieldOfView[] allEnemies = Object.FindObjectsOfType<EnemyFieldOfView>();
        foreach (EnemyFieldOfView enemy in allEnemies)
        {
            if (enemy.isCommander && enemy.commandedEnemies != null)
            {
                foreach (EnemyFieldOfView commanded in enemy.commandedEnemies)
                {
                    if (commanded == this && enemy.PlayerInFOV)
                    {
                        commanderStillSeesPlayer = true;
                        commandedTargetTime = Time.time; 
                        break;
                    }
                }
            }
            if (commanderStillSeesPlayer) break;
        }
        
        if (Time.time - commandedTargetTime > loseTargetTime * 2f)
        {
            commandedTarget = null;
            if (showDebugLogs) Debug.Log($"{gameObject.name}: Cleared commanded target");
        }
    }
}
    
void CommandNearbyEnemies()
{
    if (commandedEnemies == null || commandedEnemies.Length == 0)
    {
        if (showDebugLogs && !hasIssuedCommand)
        {
            Debug.Log($"Commander {gameObject.name}: No enemies assigned to command");
        }
        return;
    }
    
    int commandedCount = 0;
    foreach (EnemyFieldOfView enemyFOV in commandedEnemies)
    {
        if (enemyFOV != null && enemyFOV != this && CurrentTarget != null)
        {
            enemyFOV.ReceiveCommand(CurrentTarget);
            commandedCount++;
        }
    }
    
    if (showDebugLogs && !hasIssuedCommand)
    {
        Debug.Log($"Commander {gameObject.name}: Commanding {commandedCount} enemies to follow player");
    }
}
    
  
    public void ReceiveCommand(Transform target)
    {
        commandedTarget = target;
        commandedTargetTime = Time.time;
        
        if (showDebugLogs)
        {
            Debug.Log($"{gameObject.name}: Received command to follow {target.name}");
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
            
            
            float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
            
            if (angleToTarget < viewAngle / 2f)
            {
                float distToTarget = Vector3.Distance(transform.position, target.position);
                
                
                Vector3 rayOrigin = transform.position + Vector3.up * raycastHeightOffset;
                Vector3 targetPoint = target.position + Vector3.up * raycastHeightOffset;
                Vector3 rayDirection = (targetPoint - rayOrigin).normalized;
                float rayDistance = Vector3.Distance(rayOrigin, targetPoint);
                
                
                if (showDebugGizmos)
                {
                    Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.red, detectionDelay);
                }
                
               
if (!Physics.Raycast(rayOrigin, rayDirection, rayDistance, obstructionMask))
{
    PlayerInFOV = true;
    CurrentTarget = target;
    lastSeenTime = Time.time;

    lastSeenPosition = target.position;

    if (!wasInFOV && showDebugLogs)
    {
        Debug.Log($"{gameObject.name}: Player spotted at distance {distToTarget:0.00}m, angle {angleToTarget:0.0}°");
    }

    wasInFOV = true;
    return;
}

            }
        }
        
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
    
   //for debug purpose
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        

        Vector3 viewAngleA = GetDirectionFromAngle(-viewAngle / 2f);
        Vector3 viewAngleB = GetDirectionFromAngle(viewAngle / 2f);
        
        Gizmos.color = PlayerInFOV ? Color.red : Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);
        
   
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * viewRadius);
        
       
        if (CurrentTarget != null)
        {
            Gizmos.color = PlayerInFOV ? Color.red : Color.yellow;
            Gizmos.DrawLine(transform.position + Vector3.up * raycastHeightOffset, 
                           CurrentTarget.position + Vector3.up * raycastHeightOffset);
        }
        
        if (commandedTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position + Vector3.up * raycastHeightOffset, 
                           commandedTarget.position + Vector3.up * raycastHeightOffset);
        }
        
    }
    
    Vector3 GetDirectionFromAngle(float angleInDegrees)
    {
    
        return Quaternion.Euler(0, angleInDegrees, 0) * transform.forward;
    }
}