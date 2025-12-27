using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyFieldOfView))]
public class EnemySearchPatrol : MonoBehaviour
{
    [Header("Search Settings")]
    [Tooltip("Enable search behavior after losing player")]
    public bool enableSearch = true;
    [Tooltip("Radius around last seen position to search")]
    public float searchRadius = 6f;
    [Tooltip("How long to search before giving up")]
    public float searchDuration = 8f;
    [Tooltip("Time to wait at each search point")]
    public float waitAtPoint = 1.5f;
    [Tooltip("Distance to consider point reached")]
    public float arrivalDistance = 0.5f;
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    public bool showDebugLogs = false;
    
    private NavMeshAgent agent;
    private EnemyFieldOfView fov;
    private Vector3 lastSeenPosition;
    private bool isSearching;
    private float searchTimer;
    private float waitTimer;
    private bool hasReachedLastSeenPos;
    private Vector3 currentSearchPoint;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        fov = GetComponent<EnemyFieldOfView>();
    }
    
    void Update()
    {
        if (!enableSearch || agent == null || fov == null)
            return;
        
        // Player reacquired - stop searching
        if (fov.PlayerInFOV || fov.CurrentTarget != null)
        {
            if (isSearching && showDebugLogs)
            {
                Debug.Log($"{gameObject.name}: Player reacquired, stopping search");
            }
            isSearching = false;
            hasReachedLastSeenPos = false;
            return;
        }
        
        // Player just lost - start searching
        if (!isSearching && fov.TimeSinceLastSeen() < 0.2f && fov.TimeSinceLastSeen() >= 0f)
        {
            StartSearch();
        }
        
        if (!isSearching)
            return;
        
        // Count down search timer
        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0f)
        {
            if (showDebugLogs)
            {
                Debug.Log($"{gameObject.name}: Search timer expired, giving up");
            }
            StopSearch();
            return;
        }
        
        // Handle waiting at search points
        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f && showDebugLogs)
            {
                Debug.Log($"{gameObject.name}: Wait complete, picking new search point");
            }
            return;
        }
        
        // Check if reached current destination
        if (!agent.pathPending && agent.remainingDistance <= arrivalDistance)
        {
            if (!hasReachedLastSeenPos)
            {
                // Just reached the last seen position
                hasReachedLastSeenPos = true;
                if (showDebugLogs)
                {
                    Debug.Log($"{gameObject.name}: Reached last seen position, starting area search");
                }
                waitTimer = waitAtPoint;
            }
            else
            {
                // Reached a search point
                waitTimer = waitAtPoint;
                PickNewSearchPoint();
            }
        }
    }
    
    void StartSearch()
    {
        isSearching = true;
        searchTimer = searchDuration;
        hasReachedLastSeenPos = false;
        
        // Get last seen position from FOV script
        lastSeenPosition = fov.LastSeenPosition;
        
        if (showDebugLogs)
        {
            Debug.Log($"{gameObject.name}: Starting search at last seen position {lastSeenPosition}");
        }
        
        // Move to last seen position first
        agent.SetDestination(lastSeenPosition);
        currentSearchPoint = lastSeenPosition;
    }
    
    void PickNewSearchPoint()
    {
        // Generate random point around last seen position
        Vector3 randomDir = Random.insideUnitSphere * searchRadius;
        randomDir.y = 0f; // Keep search on same vertical level
        Vector3 candidate = lastSeenPosition + randomDir;
        
        // Find valid NavMesh position
        NavMeshHit hit;
        if (NavMesh.SamplePosition(candidate, out hit, searchRadius, NavMesh.AllAreas))
        {
            currentSearchPoint = hit.position;
            agent.SetDestination(currentSearchPoint);
            
            if (showDebugLogs)
            {
                float dist = Vector3.Distance(lastSeenPosition, currentSearchPoint);
                Debug.Log($"{gameObject.name}: New search point at distance {dist:0.2f}m from last seen pos");
            }
        }
        else if (showDebugLogs)
        {
            Debug.Log($"{gameObject.name}: Failed to find valid NavMesh position for search point");
        }
    }
    
    void StopSearch()
    {
        isSearching = false;
        hasReachedLastSeenPos = false;
        agent.ResetPath();
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos || !isSearching) 
            return;
        
        // Draw search area sphere
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.25f);
        Gizmos.DrawSphere(lastSeenPosition, searchRadius);
        
        // Draw search area wireframe
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(lastSeenPosition, searchRadius);
        
        // Draw last seen position marker
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(lastSeenPosition, 0.3f);
        
        // Draw line from enemy to last seen position
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, lastSeenPosition);
        
        // Draw current search point
        if (hasReachedLastSeenPos)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentSearchPoint, 0.25f);
            Gizmos.DrawLine(transform.position, currentSearchPoint);
        }
        
        // Draw remaining path
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.white;
            Vector3[] corners = agent.path.corners;
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Additional info when selected
        if (!showDebugGizmos) 
            return;
            
        if (isSearching)
        {
            // Draw search timer as text would require UnityEditor
            // So we show it via sphere size animation
            float timerPercent = searchTimer / searchDuration;
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f * timerPercent);
        }
    }
}