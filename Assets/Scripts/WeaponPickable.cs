using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickable : MonoBehaviour, IPickable
{
    public WeaponSO weaponScriprableObject;
    
    public void PickItem()
    {
        Destroy(gameObject);
    }
}
