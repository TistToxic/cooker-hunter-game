using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshPathDebug : MonoBehaviour
{
    [Header("Path Debug")]
    public bool showPath = true;
    public Color pathColor = Color.green;
    public float cornerSize = 0.15f;
    

    
    private NavMeshAgent agent;
    
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    
    void OnDrawGizmos()
    {
        // Only show while playing
        if (!Application.isPlaying)
            return;
        
        // Draw NavMesh path
        if (showPath && agent != null && agent.hasPath)
        {
            Gizmos.color = pathColor;
            Vector3 prev = transform.position;
            foreach (Vector3 corner in agent.path.corners)
            {
                Gizmos.DrawLine(prev, corner);
                Gizmos.DrawSphere(corner, cornerSize);
                prev = corner;
            }
        }
        
    }
    
}