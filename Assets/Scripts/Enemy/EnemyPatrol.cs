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

    private NavMeshAgent agent;
    private EnemyFieldOfView fov;

    private Vector3 lastSeenPosition;
    private bool isSearching;

    private float searchTimer;
    private float waitTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        fov = GetComponent<EnemyFieldOfView>();
    }

    void Update()
    {
        if (!enableSearch || agent == null || fov == null)
            return;

        // Player reacquired 
        if (fov.PlayerInFOV || fov.CurrentTarget != null)
        {
            isSearching = false;
            return;
        }

        // Player just lost 
        if (!isSearching && fov.TimeSinceLastSeen() < 0.2f)
        {
            StartSearch();
        }

        if (!isSearching)
            return;

        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0f)
        {
            StopSearch();
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= arrivalDistance)
        {
            waitTimer = waitAtPoint;
            PickNewSearchPoint();
        }
    }

    void StartSearch()
    {
        isSearching = true;
        searchTimer = searchDuration;

        lastSeenPosition = transform.position;
        if (fov.CurrentTarget == null)
        {
            lastSeenPosition = agent.destination; // fallback
        }

        agent.SetDestination(lastSeenPosition);
    }

    void PickNewSearchPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * searchRadius;
        randomDir.y = 0f;
        Vector3 candidate = lastSeenPosition + randomDir;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(candidate, out hit, searchRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void StopSearch()
    {
        isSearching = false;
        agent.ResetPath();
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos || !isSearching) return;

        Gizmos.color = new Color(1f, 0.6f, 0f, 0.25f);
        Gizmos.DrawSphere(lastSeenPosition, searchRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(lastSeenPosition, searchRadius);
    }
}
