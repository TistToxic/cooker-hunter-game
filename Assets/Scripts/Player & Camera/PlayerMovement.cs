using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    public Transform cameraTransform;
    [SerializeField] public float health = 25, hSpeed = 50, sprintSpeed = 100, speed;

    public Vector3 newVelocity = new Vector3(0f, 0f, 0f);
    private float iFrameTime = 1f, iFrameTimer = 0f, dodgeMultiplier = 1.5f;
    private bool isTouchingFloor = false, isInvincible = false, canDodge = true;
    private Stamina staminaScript;
    public int sprintStaminaConsumeRate = 5;
    private float staminaConsumeTimer = 0f;

    // Check if player is touching the floor
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
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
        staminaScript = GetComponent<Stamina>();
        speed = hSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(health);
        // Death
        if (health <= 0f)
        {
            Cursor.lockState = CursorLockMode.None;
            SceneManager.LoadScene("Death");
        }

        // I-frame timer
        if (isInvincible)
        {
            iFrameTimer += Time.deltaTime;
            if (iFrameTimer >= iFrameTime)
            {
                iFrameTimer = 0f;
                isInvincible = false;
                canDodge = true;
            }
        }

        // Get horizontal direction of player
        if (canDodge)
        {
            newVelocity.x = Convert.ToSingle(Input.GetKey(KeyCode.D)) - Convert.ToSingle(Input.GetKey(KeyCode.A));
            newVelocity.z = Convert.ToSingle(Input.GetKey(KeyCode.W)) - Convert.ToSingle(Input.GetKey(KeyCode.S));
        }

        // Sprinting
        if (staminaScript.stamina > 0 && Input.GetKey(KeyCode.LeftShift) && newVelocity.magnitude > 0)
        {
            speed = sprintSpeed;
            staminaConsumeTimer += Time.deltaTime;
            if (staminaConsumeTimer > 1f)
            {
                staminaScript.stamina -= sprintStaminaConsumeRate;
                staminaConsumeTimer -= 1f;
            }
        }
        else
        {
            speed = hSpeed;
        }

        // Normalize velocity 
        if (newVelocity.magnitude > 0f)
        {
            newVelocity.Normalize();
        }
        newVelocity *= speed;
        newVelocity = transform.rotation * newVelocity; // Rotate the new velocity 

        // Apply horizontal velocity
        if (isTouchingFloor && !isInvincible)
        {
            // Dodge
            if (Input.GetKey(KeyCode.Space) && staminaScript.stamina >= 25)
            {
                // Dodge back (without direction)
                if (newVelocity.magnitude == 0f)
                {
                    newVelocity = (new Vector3(cameraTransform.position.x, 0, cameraTransform.position.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized * hSpeed;
                }
                newVelocity *= dodgeMultiplier;
                isInvincible = true;
                canDodge = false;
                staminaScript.consumeStamina(25);
            }
            rb.linearVelocity = newVelocity;
        }

    }
}