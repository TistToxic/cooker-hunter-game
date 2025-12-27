using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    public Transform cameraTransform;
    [SerializeField] public float hSpeed = 50, vSpeed = 100;

    private Vector3 newVelocity = new Vector3(0f, 0f, 0f);
    private float dodgeMultiplier = 1.5f;
    private bool isTouchingFloor = false;
    public bool canDodge = true;
    private PlayerHealth healthScript;

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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        healthScript = GetComponent<PlayerHealth>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get horizontal direction of player
        if (canDodge)
        {
            newVelocity.x = Convert.ToSingle(Input.GetKey(KeyCode.D)) - Convert.ToSingle(Input.GetKey(KeyCode.A));
            newVelocity.z = Convert.ToSingle(Input.GetKey(KeyCode.W)) - Convert.ToSingle(Input.GetKey(KeyCode.S));
        }

        // Normalize velocity 
        if (newVelocity.magnitude > 0f)
        {
            newVelocity.Normalize();
        }
        newVelocity *= hSpeed;
        newVelocity = transform.rotation * newVelocity; // Rotate the new velocity 
        
        // Apply horizontal velocity
        if (isTouchingFloor && canDodge)
        {
            // Dodge
            if (Input.GetKey(KeyCode.Space)) 
            {
                // Dodge back (without direction)
                if (newVelocity.magnitude == 0f)
                {
                    newVelocity = (new Vector3(cameraTransform.position.x, 0, cameraTransform.position.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized * hSpeed;
                }
                newVelocity *= dodgeMultiplier;
                healthScript.isInvincible = true;
                canDodge = false;
            }
            rb.linearVelocity = newVelocity;
        }

    }
}
