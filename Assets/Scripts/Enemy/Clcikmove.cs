using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PointClickMove : MonoBehaviour
{
    [Header("Click Settings")]
    [SerializeField] private string groundTag = "Floor";
    [SerializeField] private LayerMask clickLayers = ~0;

    [Header("Debug")]
    [SerializeField] private bool showClickPoint = false;

    private NavMeshAgent agent;
    private Camera mainCam;

//SCRIPT ONLY FOR TESTING AI NAV
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        mainCam = Camera.main;
    }

    void Update()
    {
        if (!agent.isOnNavMesh)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    void HandleClick()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, clickLayers))
        {

            if (!hit.collider.CompareTag(groundTag))
                return;

           
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 1.0f, NavMesh.AllAreas))
            {
                agent.SetDestination(navHit.position);

                if (showClickPoint)
                    Debug.DrawLine(transform.position, navHit.position, Color.green, 1f);
            }
        }
    }
}
