using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    public Transform cameraTransform;
    public float health = 25, hSpeed = 50, sprintSpeed = 100, speed;

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

    void Update()
    {
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

        // 1. Get Input (Cleaner way than Convert.ToSingle)
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D
        float moveZ = Input.GetAxisRaw("Vertical");   // W/S

        if (canDodge)
        {
            // Calculate direction relative to the player's current facing direction
            Vector3 moveDir = new Vector3(moveX, 0, moveZ).normalized;
            newVelocity = transform.TransformDirection(moveDir) * speed;
        }

        // 2. Handle Sprinting
        if (staminaScript.stamina > 0 && Input.GetKey(KeyCode.LeftShift) && newVelocity.sqrMagnitude > 0)
        {
            speed = sprintSpeed;
            staminaConsumeTimer += Time.deltaTime;
            if (staminaConsumeTimer > 1f)
            {
                staminaScript.stamina -= sprintStaminaConsumeRate;
                staminaConsumeTimer = 0f;
            }
        }
        else
        {
            speed = hSpeed;
        }

        // 3. Apply Movement
        if (isTouchingFloor && !isInvincible)
        {
            // Dodge Logic
            if (Input.GetKeyDown(KeyCode.Space) && staminaScript.stamina >= 25)
            {
                if (newVelocity.sqrMagnitude == 0f)
                {
                    // Dash backward relative to where camera is facing
                    newVelocity = -transform.forward * hSpeed;
                }
                newVelocity *= dodgeMultiplier;
                isInvincible = true;
                canDodge = false;
                staminaScript.consumeStamina(25);
            }

            // Apply to Rigidbody (Keep Y velocity so gravity works!)
            rb.linearVelocity = new Vector3(newVelocity.x, rb.linearVelocity.y, newVelocity.z);
        }
    }
}