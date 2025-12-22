using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyFieldOfView))]
public class AI_Follow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    
    private NavMeshAgent agent;
    private EnemyFieldOfView fov;
    
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        fov = GetComponent<EnemyFieldOfView>();
    }
    
    void Update()
    {
        if (player == null || !agent.isOnNavMesh)
            return;
        
        // Only follow if player is in FOV
        if (fov != null && fov.PlayerInFOV && fov.CurrentTarget != null)
        {
            agent.SetDestination(fov.CurrentTarget.position);
        }
        else if (fov != null && fov.CurrentTarget != null)
        {
            // Continue to last known position if target was recently lost
            agent.SetDestination(fov.CurrentTarget.position);
        }
        else
        {
            // No target, stop moving
            agent.ResetPath();
        }
    }
}