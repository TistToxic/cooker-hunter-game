using UnityEngine;

public class Weapons
{
    public float coolDownTime;
    private float maxDamage;

    public Weapons(float coolDownTime, float maxDamage)
    {
        this.coolDownTime = coolDownTime;
        this.maxDamage = maxDamage;
    }
}

