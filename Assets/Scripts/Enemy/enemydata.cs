using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Database/Enemy Database")]
public class EnemyDatabaseSO : ScriptableObject
{
    public List<EnemyData> enemiesData;
    
    public EnemyData GetEnemyDataByID(int id)
    {
        return enemiesData.Find(data => data.ID == id);
    }
}

[Serializable]
public class EnemyData
{
    [field: SerializeField]
    public string Name { get; private set; }
    
    [field: SerializeField]
    public int ID { get; private set; }
    
    [field: SerializeField]
    public GameObject Prefab { get; private set; }
    
    [field: SerializeField]
    public float Health { get; private set; }
    
    [field: SerializeField]
    public float AttackDamage { get; private set; }
    
    [field: SerializeField]
    public float AttackRange { get; private set; }
    
    [field: SerializeField]
    public float AttackCooldown { get; private set; }
    
    [Header("Field of View")]
    [field: SerializeField]
    [field: Range(0f, 360f)]
    public float ViewAngle { get; private set; } = 90f;
    
    [field: SerializeField]
    public float ViewRadius { get; private set; } = 10f;
    
    [field: SerializeField]
    public float LoseTargetTime { get; private set; } = 3f;
    
    [Header("Patrol")]
    [field: SerializeField]
    public float PatrolRadius { get; private set; } = 5f;
    
    [field: SerializeField]
    public float PatrolWaitTime { get; private set; } = 2f;
    
    [Header("Commander")]
    [field: SerializeField]
    public bool IsCommander { get; private set; }
    
    [field: SerializeField]
    public List<int> CommandedEnemyIDs { get; private set; } = new List<int>();
    
    [field: SerializeField]
    public bool IsRangedCommander { get; private set; }
    
    [field: SerializeField]
    public RangedCommanderData RangedData { get; private set; }
}

[Serializable]
public class RangedCommanderData
{
    [field: SerializeField]
    public GameObject ProjectilePrefab { get; private set; }
    
    [field: SerializeField]
    public float ProjectileRange { get; private set; }
    
    [field: SerializeField]
    public float ProjectileDamage { get; private set; }
    
    [field: SerializeField]
    [field: Range(0f, 1f)]
    public float Accuracy { get; private set; } = 1f;
    
    [field: SerializeField]
    public float ProjectileCooldown { get; private set; }
}