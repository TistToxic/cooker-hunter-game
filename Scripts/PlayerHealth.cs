using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealthProximity : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float damagePerHit = 10f;
    public float currentHealth;
    
    [Header("Enemy Damage")]
    public float damageRadius = 2.5f;
    public float timeBeforeDamage = 1.5f;
    public string enemyTag = "Enemy";
    public float gracePeriod = 0.2f; // Time to wait before considering enemy truly gone
    
    [Header("UI")]
    public Text healthText;
    
    [Header("Debug")]
    public Color safeColor = Color.green;
    public Color dangerColor = Color.red;
    public bool showDebugLogs = true;
    
    private bool enemyInRange;
    private float damageTimer;
    private float graceTimer;
    private int enemyCount;
    private bool wasDetected;
    
    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        damageTimer = 0f;
        graceTimer = 0f;
    }
    
    void Update()
    {
        bool detectedThisFrame = IsEnemyInRange();
        
        // Handle grace period for NavMesh transitions
        if (detectedThisFrame)
        {
            wasDetected = true;
            graceTimer = gracePeriod; // Reset grace timer
        }
        else if (wasDetected)
        {
            // Enemy not detected, but might be transitioning
            graceTimer -= Time.deltaTime;
            if (graceTimer > 0)
            {
                detectedThisFrame = true; // Treat as still detected during grace period
                if (showDebugLogs) Debug.Log($"Grace period active: {graceTimer:0.00}s remaining");
            }
            else
            {
                wasDetected = false;
            }
        }
        
        // Main damage logic
        if (detectedThisFrame)
        {
            if (!enemyInRange)
            {
                enemyInRange = true;
                damageTimer = 0f;
                if (showDebugLogs) Debug.Log($"<color=yellow>Enemy entered range! Count: {enemyCount}</color>");
            }
            
            damageTimer += Time.deltaTime;
            
            if (damageTimer >= timeBeforeDamage)
            {
                TakeDamage(damagePerHit);
                damageTimer = 0f; // Reset for continuous damage
            }
        }
        else
        {
            if (enemyInRange)
            {
                enemyInRange = false;
                damageTimer = 0f;
                graceTimer = 0f;
                if (showDebugLogs) Debug.Log("<color=cyan>Enemy left range!</color>");
            }
        }
    }
    
    bool IsEnemyInRange()
    {
        enemyCount = 0;
        
        Collider[] hits = new Collider[10];
        int numHits = Physics.OverlapSphereNonAlloc(transform.position, damageRadius, hits);
        
        for (int i = 0; i < numHits; i++)
        {
            if (hits[i] != null && hits[i].gameObject != null && hits[i].CompareTag(enemyTag))
            {
                // Extra check: make sure the enemy GameObject is active
                if (hits[i].gameObject.activeInHierarchy)
                {
                    enemyCount++;
                    if (showDebugLogs)
                    {
                        float dist = Vector3.Distance(transform.position, hits[i].transform.position);
                        Debug.Log($"Enemy: {hits[i].gameObject.name} | Distance: {dist:0.00}m | Active: {hits[i].enabled}");
                    }
                }
            }
        }
        
        return enemyCount > 0;
    }
    
    void TakeDamage(float amount)
    {
        if (currentHealth <= 0)
            return;
            
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
        
        Debug.Log($"<color=red>DAMAGE! -{amount} HP | Health: {currentHealth}/{maxHealth} | Enemies in range: {enemyCount}</color>");
        
        if (currentHealth <= 0)
        {
            Debug.Log("<color=red>═══ PLAYER DEAD ═══</color>");
            enemyInRange = false;
            this.enabled = false;
        }
    }
    
    void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = $"Health: {currentHealth} / {maxHealth}";
    }
    
    void OnDrawGizmos()
    {
        // Main detection sphere
        Gizmos.color = enemyInRange ? dangerColor : safeColor;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
        
        // Show grace period with inner sphere
        if (Application.isPlaying && graceTimer > 0)
        {
            Gizmos.color = Color.yellow;
            float graceRadius = damageRadius * (graceTimer / gracePeriod);
            Gizmos.DrawWireSphere(transform.position, graceRadius);
        }
        
        // Draw lines to detected enemies
        if (Application.isPlaying && enemyInRange)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
            foreach (GameObject enemy in enemies)
            {
                if (enemy != null)
                {
                    float dist = Vector3.Distance(transform.position, enemy.transform.position);
                    if (dist <= damageRadius)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(transform.position, enemy.transform.position);
                    }
                }
            }
        }
    }
}