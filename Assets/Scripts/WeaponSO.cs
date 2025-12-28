using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")]
public class WeaponSO : ScriptableObject
{
    [Header("Properties")]
    public float cooldown;
    public itemType item_type;
    public Sprite item_sprite;
}

public enum itemType { Hammer, Machete, Mace, Club };
