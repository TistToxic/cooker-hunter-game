using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyFieldOfView))]
public class EnemyRangedCombat : MonoBehaviour
{
    [Header("Projectile Settings")]
    [Tooltip("Projectile prefab to instantiate")]
    public GameObject projectilePrefab;
    
    [Tooltip("Name/type of projectile for identification")]
    public string projectileName = "Arrow";
    
    [Tooltip("Point where projectiles spawn")]
    public Transform firePoint;
    
    [Header("Combat Stats")]
    [Range(0f, 1f)]
    [Tooltip("1 = perfect aim, 0 = completely random")]
    public float accuracy = 0.85f;
    
    [Tooltip("Time between shots in seconds")]
    public float reloadSpeed = 2f;
    
    [Tooltip("Maximum range to engage target")]
    public float maxRange = 25f;
    
    [Tooltip("Minimum range to engage target")]
    public float minRange = 3f;
    
[Header("Trajectory Settings")]
[Tooltip("Projectile launch speed")]
public float projectileSpeed = 20f;

[Tooltip("Launch angle for straight shots (in degrees, 0 = horizontal)")]
[Range(-45f, 45f)]
public float straightShotAngle = 0f;

[Tooltip("Launch angle for arc shots (in degrees, 30-60 recommended)")]
[Range(0f, 89f)]
public float arcShotAngle = 45f;

[Tooltip("Arc height multiplier (0 = straight, higher = more arc)")]
public float arcHeight = 1.5f;

[Tooltip("Switch to arc shot if direct path is blocked")]
public bool useArcWhenObstructed = true;
    
    [Header("Commander Mode")]
    [Tooltip("Get target info from commanded enemies' FOV")]
    public bool isCommander = false;
    
    [Header("Debug")]
    public bool showDebugLogs = false;
    public bool showTrajectoryGizmos = true;
    
    private EnemyFieldOfView fov;
    private float nextFireTime;
    private Transform currentTarget;
    private Vector3 lastKnownTargetPosition;
    private bool canSeeTarget;
    
    void Start()
    {
        fov = GetComponent<EnemyFieldOfView>();
        nextFireTime = Time.time;
        
        // Create default fire point if none assigned
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = Vector3.up * 1.5f; // Head height
            firePoint = fp.transform;
            
            if (showDebugLogs)
                Debug.Log($"{gameObject.name}: Created default fire point");
        }
    }
    
    void Update()
    {
        UpdateTarget();
        
        if (currentTarget != null && Time.time >= nextFireTime)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            
            // Check if in valid range
            if (distanceToTarget >= minRange && distanceToTarget <= maxRange)
            {
                FireAtTarget();
                nextFireTime = Time.time + reloadSpeed;
            }
        }
    }
    
    void UpdateTarget()
    {
        if (isCommander)
        {
            // Commander mode: Get target from commanded enemies
            UpdateTargetFromCommanded();
        }
        else
        {
            // Normal mode: Use own FOV
            UpdateTargetFromOwnFOV();
        }
    }
    
    void UpdateTargetFromOwnFOV()
    {
        if (fov.PlayerInFOV && fov.CurrentTarget != null)
        {
            currentTarget = fov.CurrentTarget;
            lastKnownTargetPosition = currentTarget.position;
            canSeeTarget = true;
        }
        else
        {
            canSeeTarget = false;
            // Keep shooting at last known position for a bit
            if (fov.TimeSinceLastSeen() > 3f)
            {
                currentTarget = null;
            }
        }
    }
    
    void UpdateTargetFromCommanded()
    {
        // Find any commanded enemy that sees the player
        if (fov.isCommander && fov.commandedEnemies != null)
        {
            foreach (EnemyFieldOfView commandedEnemy in fov.commandedEnemies)
            {
                if (commandedEnemy != null && commandedEnemy.PlayerInFOV && commandedEnemy.CurrentTarget != null)
                {
                    currentTarget = commandedEnemy.CurrentTarget;
                    lastKnownTargetPosition = currentTarget.position;
                    canSeeTarget = false; // Commander doesn't need direct LOS
                    return;
                }
            }
        }
        
        // Fallback to own FOV if no commanded enemies see target
        UpdateTargetFromOwnFOV();
    }
    
    void FireAtTarget()
    {
        if (projectilePrefab == null)
        {
            if (showDebugLogs)
                Debug.LogWarning($"{gameObject.name}: No projectile prefab assigned!");
            return;
        }
        
        if (currentTarget == null)
            return;
        
        Vector3 targetPosition = currentTarget.position + Vector3.up * 1f; // Aim at chest height
        
        // Check if direct path is obstructed
        bool pathObstructed = IsPathObstructed(targetPosition);
        bool useArc = pathObstructed && useArcWhenObstructed;
        
        // Apply accuracy scatter
        Vector3 aimPosition = ApplyAccuracyScatter(targetPosition);
        
        // Calculate trajectory
        Vector3 launchVelocity;
        if (useArc)
        {
            launchVelocity = CalculateArcVelocity(firePoint.position, aimPosition);
        }
        else
        {
            launchVelocity = CalculateStraightVelocity(firePoint.position, aimPosition);
        }
        
        // Spawn and launch projectile
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        // Orient projectile to face trajectory
        projectile.transform.rotation = Quaternion.LookRotation(launchVelocity.normalized);
        
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = launchVelocity;
        }
        else
        {
            if (showDebugLogs)
                Debug.LogWarning($"{gameObject.name}: Projectile has no Rigidbody!");
        }
        
        if (showDebugLogs)
        {
            string shotType = useArc ? "Arc" : "Straight";
            float dist = Vector3.Distance(transform.position, currentTarget.position);
            Debug.Log($"{gameObject.name}: Fired {projectileName} ({shotType}) at {currentTarget.name} - Distance: {dist:F1}m");
        }
    }
    
    bool IsPathObstructed(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - firePoint.position;
        float distance = direction.magnitude;
        
        // Raycast to check for obstructions
        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, direction.normalized, out hit, distance, fov.obstructionMask))
        {
            // Something is blocking the path
            return true;
        }
        
        return false;
    }
    
    Vector3 ApplyAccuracyScatter(Vector3 targetPosition)
    {
        if (accuracy >= 1f)
            return targetPosition;
        
        float maxScatter = (1f - accuracy) * 3f; 
        
        Vector3 scatter = new Vector3(
            Random.Range(-maxScatter, maxScatter),
            Random.Range(-maxScatter * 0.5f, maxScatter * 0.5f), 
            Random.Range(-maxScatter, maxScatter)
        );
        
        return targetPosition + scatter;
    }
    
Vector3 CalculateStraightVelocity(Vector3 origin, Vector3 target)
{
    Vector3 toTarget = target - origin;
    Vector3 horizontalDirection = new Vector3(toTarget.x, 0, toTarget.z).normalized;
    
    float angleRad = straightShotAngle * Mathf.Deg2Rad;

    Vector3 velocity = horizontalDirection * projectileSpeed * Mathf.Cos(angleRad);
    velocity.y = projectileSpeed * Mathf.Sin(angleRad);
    
    return velocity;
}
    
Vector3 CalculateArcVelocity(Vector3 origin, Vector3 target)
{
    Vector3 toTarget = target - origin;
    Vector3 toTargetXZ = new Vector3(toTarget.x, 0, toTarget.z);
    
    float distance = toTargetXZ.magnitude;
    
    float angle = arcShotAngle * Mathf.Deg2Rad;
    
    Vector3 direction = toTargetXZ.normalized;
    float horizontalSpeed = projectileSpeed * Mathf.Cos(angle);
    float verticalSpeed = projectileSpeed * Mathf.Sin(angle);
    
    Vector3 velocity = direction * horizontalSpeed;
    velocity.y = verticalSpeed;
    
    return velocity;
}
    
    void OnDrawGizmos()
    {
        if (!showTrajectoryGizmos || firePoint == null)
            return;
        
        // Draw fire point
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(firePoint.position, 0.1f);
        
        // Draw range indicators
        Gizmos.color = new Color(1f, 0f, 0f, 0.1f);
        Gizmos.DrawWireSphere(transform.position, maxRange);
        
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, minRange);
        
        // Draw line to current target
        if (currentTarget != null)
        {
            Vector3 targetPos = currentTarget.position + Vector3.up * 1f;
            
            bool obstructed = IsPathObstructed(targetPos);
            Gizmos.color = obstructed ? Color.yellow : Color.green;
            Gizmos.DrawLine(firePoint.position, targetPos);
            
            // Draw target indicator
            Gizmos.DrawWireSphere(targetPos, 0.5f);
            
            // Draw predicted trajectory arc
            if (obstructed && useArcWhenObstructed)
            {
                DrawTrajectoryArc(firePoint.position, targetPos, Color.cyan);
            }
        }
    }
    
    void DrawTrajectoryArc(Vector3 start, Vector3 end, Color color)
    {
        Vector3 velocity = CalculateArcVelocity(start, end);
        Vector3 pos = start;
        Vector3 prevPos = start;
        
        float timeStep = 0.1f;
        float maxTime = 3f;
        
        Gizmos.color = color;
        
        for (float t = 0; t < maxTime; t += timeStep)
        {
            prevPos = pos;
            pos = start + velocity * t + 0.5f * Physics.gravity * t * t;
            
            Gizmos.DrawLine(prevPos, pos);
            
            // Stop if we've gone past the target
            if (pos.y < end.y - 5f)
                break;
        }
    }
    
    // Public method to check if ready to fire
    public bool IsReadyToFire()
    {
        return Time.time >= nextFireTime;
    }
    
    // Public method to get current target
    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }
}