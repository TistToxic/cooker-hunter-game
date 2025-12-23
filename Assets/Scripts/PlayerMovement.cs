using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;
    public Transform player;

    [SerializeField]
    public float health = 25, hSpeed = 50, vSpeed = 100;

    private Vector3 newVelocity = new Vector3(0f, 0f, 0f);
    private float jumpVelocity, iFrameTime = 1f, iFrameTimer = 0f;
    private int stamina = 100;
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
        newVelocity.x = Convert.ToSingle(Input.GetKey(KeyCode.D)) - Convert.ToSingle(Input.GetKey(KeyCode.A));
        newVelocity.z = Convert.ToSingle(Input.GetKey(KeyCode.W)) - Convert.ToSingle(Input.GetKey(KeyCode.S));
        Debug.Log(isInvincible);

        // Normalize velocity 
        if (newVelocity.magnitude > 0f)
        {
            newVelocity.Normalize();
        }
        newVelocity *= hSpeed;
        newVelocity = player.rotation * newVelocity; // Rotate the new velocity 
        
        // Apply horizontal velocity
        if (isTouchingFloor)
        {
            if (Input.GetKey(KeyCode.Space) && stamina >= 25) // Dodge
            {
                if (newVelocity.magnitude == 0f)
                {
                    newVelocity = player.rotation * Vector3.back * hSpeed;
                }
                newVelocity *= 3f;
                isInvincible = true;
                stamina -= 25;
            }
            rb.linearVelocity = newVelocity;
        }

    }
}
