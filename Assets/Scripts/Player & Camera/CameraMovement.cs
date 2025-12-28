using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform player, playerHead;
    public Renderer playerRenderer;

    [Header("Distance Settings")]
    [SerializeField] private float maxDistance = 5f; // How far you want it normally
    [SerializeField] private float minDistance = 0.5f;
    [SerializeField] private float smoothSpeed = 10f;

    [Header("Collision")]
    [SerializeField] private float mouseSensitivity = 300f;
    [SerializeField] private LayerMask collisionLayers; // MAKE SURE PLAYER LAYER IS UNCHECKED HERE
    [SerializeField] private float collisionPadding = 0.2f; // Prevents camera clipping into walls

    private float rotationX, rotationY;
    private float currentDistance;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currentDistance = maxDistance;
    }

    void LateUpdate()
    {
        // 1. Input
        rotationX -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        rotationY += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        rotationX = Mathf.Clamp(rotationX, -45f, 90f);

        // 2. Rotate Player & Head
        player.rotation = Quaternion.Euler(0, rotationY, 0);
        playerHead.rotation = Quaternion.Euler(rotationX, rotationY, 0);

        // 3. Collision Check
        Vector3 desiredCameraPos = playerHead.position - (playerHead.forward * maxDistance);
        RaycastHit hit;

        // Shoot a ray from head to the desired camera position
        if (Physics.Raycast(playerHead.position, -playerHead.forward, out hit, maxDistance, collisionLayers))
        {
            // If we hit a wall, the target distance is the hit distance minus padding
            float hitDistance = hit.distance - collisionPadding;
            currentDistance = Mathf.Clamp(hitDistance, minDistance, maxDistance);
        }
        else
        {
            // If nothing is hitting, LERP back out to the max distance
            currentDistance = Mathf.Lerp(currentDistance, maxDistance, Time.deltaTime * smoothSpeed);
        }

        // 4. Apply Position
        transform.position = playerHead.position - (playerHead.forward * currentDistance);
        transform.LookAt(playerHead.position + (playerHead.forward * 10f)); // Keep camera looking forward

        UpdatePlayerOpacity();
    }

    void UpdatePlayerOpacity()
    {
        if (playerRenderer == null) return;

        // Transparency kicks in when camera gets very close
        float alpha = (currentDistance < 1.2f) ? 0.3f : 1.0f;
        Color color = playerRenderer.material.color;

        // Only update if it changed to save performance
        if (color.a != alpha)
        {
            color.a = alpha;
            playerRenderer.material.color = color;
        }
    }
}