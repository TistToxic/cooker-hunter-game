using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;
    public Transform player;

    [SerializeField]
    public float health, hSpeed, vSpeed; // Sample values = 100, 100, 200

    private Vector3 newVelocity = new Vector3(0f, 0f, 0f);
    private float jumpVelocity;
    private bool isTouchingFloor = false;

    // Check if player is touching the floor
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor")){
            isTouchingFloor = true;
        }
    }

    // Check if player has stopped touching the floor
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            isTouchingFloor = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Get horizontal direction of player
        newVelocity.x = Convert.ToSingle(Input.GetKey(KeyCode.D)) - Convert.ToSingle(Input.GetKey(KeyCode.A));
        newVelocity.z = Convert.ToSingle(Input.GetKey(KeyCode.W)) - Convert.ToSingle(Input.GetKey(KeyCode.S));
        Debug.Log(newVelocity.y);

        // Normalize velocity 
        if (newVelocity.magnitude > 0f)
        {
            newVelocity.Normalize();
        }
        newVelocity *= hSpeed;
        newVelocity = player.rotation * newVelocity; // Rotate the new velocity 
        newVelocity.y = Convert.ToSingle(Input.GetKey(KeyCode.Space)) * Convert.ToSingle(isTouchingFloor) * vSpeed; // Get jump velocity

        // Apply velocity
        if (isTouchingFloor)
        {
            rb.linearVelocity = newVelocity * Time.fixedDeltaTime;
        }

    }
}
