using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;
    public Transform player;

    [SerializeField]
    private float forceMagnitude;
    [SerializeField]
    private float JumpMagnitude;

    private Vector3 netForce;
    private float jumpForce;
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
        netForce.x = Convert.ToSingle(Input.GetKey(KeyCode.D)) - Convert.ToSingle(Input.GetKey(KeyCode.A));
        netForce.z = Convert.ToSingle(Input.GetKey(KeyCode.W)) - Convert.ToSingle(Input.GetKey(KeyCode.S));
        Debug.Log(netForce.y);

        // Normalize net force
        if (netForce.magnitude > 0f)
        {
            netForce /= netForce.magnitude; // <--- This might not be the magnitude we want, so we normalize the vector then scale it by the magnitude we want
        }
        netForce *= forceMagnitude * Time.fixedDeltaTime;
        netForce = player.rotation * netForce;

        // Apply horizontal force to player
        rb.AddForce(netForce);

        // Apply vertical force to player
        jumpForce = Convert.ToSingle(Input.GetKey(KeyCode.Space)) * Convert.ToSingle(isTouchingFloor);
        rb.AddForce(0, jumpForce * JumpMagnitude * Time.fixedDeltaTime, 0);
    }
}
