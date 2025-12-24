using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] public float health = 25, hSpeed = 50, vSpeed = 100;

    private Vector3 newVelocity = new Vector3(0f, 0f, 0f);
    private float iFrameTime = 1f, iFrameTimer = 0f, dodgeMultiplier = 1.5f;
    private bool isTouchingFloor = false, isInvincible = false;

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
    }

    // Update is called once per frame
    void Update()
    {
        // I-frame timer
        if (isInvincible)
        {
            iFrameTimer += Time.deltaTime;
            if (iFrameTimer >= iFrameTime)
            {
                iFrameTimer = 0f;
                isInvincible = false;
            }
        }


        // Get horizontal direction of player
        if (!isInvincible)
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
        if (isTouchingFloor && !isInvincible)
        {
            // Dodge
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                // Dodge back (without direction)
                if (newVelocity.magnitude == 0f)
                {
                    newVelocity = transform.rotation * Vector3.back * hSpeed;
                }
                newVelocity *= dodgeMultiplier;
                isInvincible = true;
            }
            rb.linearVelocity = newVelocity;
        }

    }
}
