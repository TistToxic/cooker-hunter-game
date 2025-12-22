using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform player;
    private float mouseY;
    private float rotationY = 0f;

    [SerializeField]
    private float mouseSensitivity;

    void Start()
    {
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Get mouse movement
        mouseY = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        rotationY += mouseY;

        // Rotate the player capsule
        player.rotation = Quaternion.Euler(0, rotationY, 0);
    }
}
