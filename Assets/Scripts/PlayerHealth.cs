using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] public float health = 100;
    public bool isInvincible = false;
    private float iFrameTime = 0.5f, iFrameTimer = 0f;
    private PlayerMovement movementScript;

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Hazard") && !isInvincible)
        {
            health -= 25;
            isInvincible = true;
        }
    }

    void Start()
    {
       movementScript = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        Debug.Log(health);

        // I-frame timer
        if (isInvincible)
        {
            iFrameTimer += Time.deltaTime;
            if (iFrameTimer >= iFrameTime)
            {
                iFrameTimer = 0f;
                isInvincible = false;
                movementScript.canDodge = true;
            }
        }

        // Death 
        if (health <= 0)
        {
            Cursor.lockState = CursorLockMode.None;
            SceneManager.LoadScene("Death");
        }
    }
}
