using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;

    void Start()
    {
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.position = new Vector3(player.position.x, player.position.y + 3, player.position.z - 10);
        //transform.rotation = Quaternion.Euler(45, 0, 0);
    }
}
