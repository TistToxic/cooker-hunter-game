using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBehavior : MonoBehaviour
{
    [Header("Enemy Database")]
    public EnemyDatabaseSO enemyDatabase;
    public int enemyID;
    
    [Header("Detection Settings")]
    public LayerMask targetMask;
    public LayerMask obstructionMask;
    public string playerTag = "Player";
    public float detectionDelay = 0.1f;
    public float raycastHeightOffset = 0.5f;
    
    [Header("Debug")]
    public bool showDebugLogs = false;
    public bool showDebugGizmos = true;
    
    //database
    private EnemyData enemyData;
    private float viewAngle;
    private float viewRadius;
    private float loseTargetTime;
    private float attackRange;
    private float attackDamage;
    private float attackCooldown;
    
    // FOV 
    public bool PlayerInFOV { get; private set; }
    public Transform CurrentTarget { get; private set; }
    
    private NavMeshAgent agent;
    private float lastSeenTime;
    private bool wasInFOV;
    private float lastAttackTime;

    // Health System
    private float currentHealth;
    public float CurrentHealth { get { return currentHealth; } }
    public float MaxHealth { get { return enemyData != null ? enemyData.Health : 100f; } }
    public bool IsDead { get; private set; }
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        LoadEnemyData();
        PlayerInFOV = false;
        lastSeenTime = -loseTargetTime;
        lastAttackTime = -attackCooldown;
        StartCoroutine(FOVRoutine());
    }
    
    void LoadEnemyData()
    {
        if (enemyDatabase == null)
        {
            Debug.LogError($"{gameObject.name}: Enemy Database is not assigned!");
            return;
        }
        
        enemyData = enemyDatabase.GetEnemyDataByID(enemyID);
        
        if (enemyData == null)
        {
            Debug.LogError($"{gameObject.name}: Enemy with ID {enemyID} not found in database!");
            return;
        }
        
        // Load FOV settings
        viewAngle = enemyData.ViewAngle;
        viewRadius = enemyData.ViewRadius;
        loseTargetTime = enemyData.LoseTargetTime;
        
        // Load attack settings
        attackRange = enemyData.AttackRange;
        attackDamage = enemyData.AttackDamage;
        attackCooldown = enemyData.AttackCooldown;

        currentHealth = enemyData.Health;
        IsDead = false;
        
        if (showDebugLogs)
        {
            Debug.Log($"{gameObject.name}: Loaded data for {enemyData.Name} (ID: {enemyID})");
        }
    }
    
    void Update()
    {
        if (IsDead) return;
        
        if (PlayerInFOV && CurrentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, CurrentTarget.position);
            
            if (distanceToTarget <= attackRange)
            {
                if (agent != null && agent.enabled)
                {
                    agent.ResetPath();
                }
                
                AttackTarget();
            }
            else
            {
                if (agent != null && agent.enabled && agent.isOnNavMesh)
                {
                    agent.SetDestination(CurrentTarget.position);
                }
            }
        }
        else if (!PlayerInFOV && CurrentTarget != null)
        {
            if (Time.time - lastSeenTime > loseTargetTime)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"{gameObject.name}: Lost target after {loseTargetTime}s");
                }
                CurrentTarget = null;
                
                if (agent != null && agent.enabled)
                {
                    agent.ResetPath();
                }
            }
            else
            {
                if (agent != null && agent.enabled && agent.isOnNavMesh)
                {
                    agent.SetDestination(CurrentTarget.position);
                }
            }
        }
    }
    
    void AttackTarget()
    {
        if (IsDead) return;
        
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            
            if (showDebugLogs)
            {
                Debug.Log($"{gameObject.name}: Attacking {CurrentTarget.name} for {attackDamage} damage");
            }
            
        }
    }
    
    System.Collections.IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(detectionDelay);
        
        while (true)
        {
            yield return wait;
            
            if (IsDead)
            {
                yield break; 
            }
            
            CheckFieldOfView();
        }
    }
    
    void CheckFieldOfView()
    {
        if (IsDead) return;
        
        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        
        PlayerInFOV = false;
        
        foreach (Collider col in targetsInRadius)
        {
            if (!col.CompareTag(playerTag))
                continue;
            
            Transform target = col.transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            
            float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
            
            if (angleToTarget < viewAngle / 2f)
            {
                float distToTarget = Vector3.Distance(transform.position, target.position);
                
                Vector3 rayOrigin = transform.position + Vector3.up * raycastHeightOffset;
                Vector3 targetPoint = target.position + Vector3.up * raycastHeightOffset;
                Vector3 rayDirection = (targetPoint - rayOrigin).normalized;
                float rayDistance = Vector3.Distance(rayOrigin, targetPoint);
                
                if (showDebugGizmos)
                {
                    Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.red, detectionDelay);
                }
                
                if (!Physics.Raycast(rayOrigin, rayDirection, rayDistance, obstructionMask))
                {
                    PlayerInFOV = true;
                    CurrentTarget = target;
                    lastSeenTime = Time.time;

                    if (!wasInFOV && showDebugLogs)
                    {
                        Debug.Log($"{gameObject.name}: Player spotted at distance {distToTarget:0.00}m, angle {angleToTarget:0.0}°");
                    }

                    wasInFOV = true;
                    return;
                }
            }
        }
        
        if (wasInFOV && showDebugLogs)
        {
            Debug.Log($"{gameObject.name}: Player left FOV");
        }
        wasInFOV = false;
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        float angle = Application.isPlaying ? viewAngle : 90f;
        float radius = Application.isPlaying ? viewRadius : 10f;
        float range = Application.isPlaying ? attackRange : 2f;
        
        // Draw FOV cone
        Vector3 viewAngleA = GetDirectionFromAngle(-angle / 2f);
        Vector3 viewAngleB = GetDirectionFromAngle(angle / 2f);
        
        Gizmos.color = PlayerInFOV ? Color.red : Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * radius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * radius);
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * radius);
        
        // Draw attack range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
        
        if (CurrentTarget != null)
        {
            Gizmos.color = PlayerInFOV ? Color.red : Color.yellow;
            Gizmos.DrawLine(transform.position + Vector3.up * raycastHeightOffset, 
                           CurrentTarget.position + Vector3.up * raycastHeightOffset);
        }

        if (Application.isPlaying && !IsDead)
        {
            Vector3 healthBarPos = transform.position + Vector3.up * 2.5f;
            float healthPercent = currentHealth / MaxHealth;
            
            // Background
            Gizmos.color = Color.red;
            Gizmos.DrawLine(healthBarPos - Vector3.right * 0.5f, healthBarPos + Vector3.right * 0.5f);
            
            // Health
            Gizmos.color = Color.green;
            Vector3 healthBarEnd = healthBarPos - Vector3.right * 0.5f + Vector3.right * healthPercent;
            Gizmos.DrawLine(healthBarPos - Vector3.right * 0.5f, healthBarEnd);
        }
    }
    
    Vector3 GetDirectionFromAngle(float angleInDegrees)
    {
        return Quaternion.Euler(0, angleInDegrees, 0) * transform.forward;
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;
        
        currentHealth -= damage;
        
        if (showDebugLogs)
        {
            Debug.Log($"{gameObject.name}: Took {damage} damage. Health: {currentHealth}/{MaxHealth}");
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (IsDead) return;
        
        IsDead = true;
        
        if (showDebugLogs)
        {
            Debug.Log($"{gameObject.name}: Died!");
        }
        
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;  // Stop the agent first
            agent.ResetPath();       // Clear any path
            agent.enabled = false;   // Then disable it
        }
        
        PlayerInFOV = false;
        CurrentTarget = null;
        
        
        Destroy(gameObject, 2f);
    }
}