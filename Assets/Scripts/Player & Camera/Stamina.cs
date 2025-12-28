using UnityEngine;

public class Stamina : MonoBehaviour
{
    public PlayerMovement movementScript;
    public int maxStamina = 100;
    public int stamina = 100;
    public int staminaRechargeRate = 5;
    private float rechargeTimerSec = 0f;

    public void consumeStamina(int staminaConsumed)
    {
        stamina -= staminaConsumed;
    }

    void Update()
    {
        Debug.Log(stamina);
        // Stamina recharge
        if (stamina < maxStamina && (!Input.GetKey(KeyCode.LeftShift) || movementScript.newVelocity.magnitude == 0f))
        {
            if (rechargeTimerSec < 1f)
            {
                rechargeTimerSec += Time.deltaTime;
            }
            else
            {
                rechargeTimerSec -= 1f;
                stamina += staminaRechargeRate;
            }
        }
    }
}
