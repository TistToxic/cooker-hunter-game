using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    public float maxHealth = 100;
    public float health = 100;
    public bool isInvincible = false;
    public bool isDead = false;
    public float iFrameTimeSec = 0.5f;
    private float iFrameTimer = 0f;

    // Damage
    public void applyDamage(float damage)
    {
        if (!isInvincible && health >= damage)
        {
            health -= damage;
        }
    }

    // Heal
    public void heal(float healAmount)
    {
        if (health + healAmount <= maxHealth)
        {
            health += healAmount;
        }
    }

    void Update()
    {
        //Debug.Log(health);

        // Invincibility frames
        if (isInvincible)
        {
            iFrameTimer += Time.deltaTime;
            if (iFrameTimer >= iFrameTimeSec)
            {
                iFrameTimer = 0f;
                isInvincible = false;
            }
        }

        // Death 
        if (health <= 0)
        {
            isDead = true;
        }
    }
}
