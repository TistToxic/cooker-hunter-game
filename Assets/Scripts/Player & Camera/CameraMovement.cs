using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform player, playerHead;
    private float mouseX, mouseY, rotationX, rotationY = 0f;
    private Vector3 collisionPoint;

    [SerializeField] private float mouseSensitivity = 300f;

    void Start()
    {
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    void Update()
    {
        // Get mouse movement
        mouseX = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

        // Add to rotation
        rotationX -= mouseX;
        rotationY += mouseY;
        rotationX = Mathf.Clamp(rotationX, -45f, 90f);

        // Rotate the player and player head
        player.rotation = Quaternion.Euler(0, rotationY, 0);
        playerHead.rotation = Quaternion.Euler(rotationX, rotationY, 0);
    }
}
