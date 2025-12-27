using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public enum HitboxType
    {
        AOE,        // Sphere
        Cone,       // Angle-based
        Straight    // Box forward
    }

    [System.Serializable]
    public class WeaponData
    {
        [Header("Basic")]
        public string weaponName;
        public float damage = 25f;
        public float cooldown = 1f;

        [Header("Hitbox")]
        public HitboxType hitboxType;

        [Tooltip("AOE radius OR Cone radius")]
        public float radius = 2f;

        [Tooltip("Cone angle (degrees)")]
        [Range(0f, 180f)]
        public float angle = 60f;

        [Tooltip("Straight attack length")]
        public float length = 2f;

        [Tooltip("Straight attack width")]
        public float width = 1f;
    }

    [Header("Weapons")]
    public WeaponData[] weapons = new WeaponData[4];
    private int selectedWeaponIndex = 0;
    private int previousWeaponIndex = -1;

    [Header("Attack Settings")]
    public Transform attackOrigin;
    public LayerMask enemyLayer;
    public KeyCode attackKey = KeyCode.Mouse0;

    [Header("Debug")]
    public bool showDebugLogs = true;
    public bool showDebugGizmos = true;

    [Header("Attack Visual")]
    public Material attackLineMaterial;
    public float attackLineDuration = 0.15f;
    public Color attackColor = Color.red;

    [Header("Range Visualization")]
    public bool showRangeIndicator = true;
    public Color rangeColor = new Color(0f, 1f, 0f, 0.3f);
    public Material rangeIndicatorMaterial;
    
    private GameObject rangeIndicator;
    private LineRenderer rangeLineRenderer;
    private MeshRenderer rangeMeshRenderer;
    private MeshFilter rangeMeshFilter;

    // Cooldown
    private bool canAttack = true;
    private float cooldownTimer = 0f;

    void Start()
    {
        CreateRangeIndicator();
        LogSelectedWeapon();
    }

    void Update()
    {
        HandleWeaponSwitch();
        HandleCooldown();
        HandleAttackInput();
        UpdateRangeIndicator();
    }

    // -----------------------------
    // RANGE INDICATOR
    // -----------------------------

    void CreateRangeIndicator()
    {
        rangeIndicator = new GameObject("RangeIndicator");
        rangeIndicator.transform.SetParent(transform);
        
        rangeLineRenderer = rangeIndicator.AddComponent<LineRenderer>();
        rangeMeshRenderer = rangeIndicator.AddComponent<MeshRenderer>();
        rangeMeshFilter = rangeIndicator.AddComponent<MeshFilter>();

        // Setup LineRenderer
        rangeLineRenderer.material = rangeIndicatorMaterial != null 
            ? rangeIndicatorMaterial 
            : new Material(Shader.Find("Sprites/Default"));
        rangeLineRenderer.startColor = rangeColor;
        rangeLineRenderer.endColor = rangeColor;
        rangeLineRenderer.startWidth = 0.1f;
        rangeLineRenderer.endWidth = 0.1f;
        rangeLineRenderer.useWorldSpace = true;

        // Setup MeshRenderer
        rangeMeshRenderer.material = rangeIndicatorMaterial != null 
            ? rangeIndicatorMaterial 
            : new Material(Shader.Find("Sprites/Default"));
        rangeMeshRenderer.material.color = rangeColor;
        
        rangeIndicator.SetActive(showRangeIndicator);
    }

    void UpdateRangeIndicator()
    {
        if (!showRangeIndicator || rangeIndicator == null)
        {
            if (rangeIndicator != null)
                rangeIndicator.SetActive(false);
            return;
        }

        rangeIndicator.SetActive(true);
        Vector3 origin = attackOrigin != null ? attackOrigin.position : transform.position;
        WeaponData weapon = weapons[selectedWeaponIndex];

        rangeLineRenderer.enabled = false;
        rangeMeshRenderer.enabled = false;

        switch (weapon.hitboxType)
        {
            case HitboxType.AOE:
                DrawAOEIndicator(origin, weapon);
                break;

            case HitboxType.Cone:
                DrawConeIndicator(origin, weapon);
                break;

            case HitboxType.Straight:
                DrawStraightIndicator(origin, weapon);
                break;
        }
    }

    void DrawAOEIndicator(Vector3 origin, WeaponData weapon)
    {
        rangeLineRenderer.enabled = true;
        rangeLineRenderer.positionCount = 37;

        for (int i = 0; i <= 36; i++)
        {
            float angle = i * 10f * Mathf.Deg2Rad;
            Vector3 pos = origin + new Vector3(
                Mathf.Cos(angle) * weapon.radius,
                0.1f,
                Mathf.Sin(angle) * weapon.radius
            );
            rangeLineRenderer.SetPosition(i, pos);
        }
    }

    void DrawConeIndicator(Vector3 origin, WeaponData weapon)
    {
        rangeLineRenderer.enabled = true;
        
        int segments = 20;
        rangeLineRenderer.positionCount = segments + 2;

        rangeLineRenderer.SetPosition(0, origin + Vector3.up * 0.1f);

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float currentAngle = Mathf.Lerp(-weapon.angle / 2f, weapon.angle / 2f, t);
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            Vector3 pos = origin + direction * weapon.radius + Vector3.up * 0.1f;
            rangeLineRenderer.SetPosition(i + 1, pos);
        }
    }

    void DrawStraightIndicator(Vector3 origin, WeaponData weapon)
    {
        rangeLineRenderer.enabled = true;
        rangeLineRenderer.positionCount = 5;

        Vector3 forward = transform.forward * weapon.length;
        Vector3 right = transform.right * (weapon.width * 0.5f);
        Vector3 offset = Vector3.up * 0.1f;

        rangeLineRenderer.SetPosition(0, origin - right + offset);
        rangeLineRenderer.SetPosition(1, origin + forward - right + offset);
        rangeLineRenderer.SetPosition(2, origin + forward + right + offset);
        rangeLineRenderer.SetPosition(3, origin + right + offset);
        rangeLineRenderer.SetPosition(4, origin - right + offset);
    }

    // -----------------------------
    // INPUT
    // -----------------------------

    void HandleWeaponSwitch()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0) selectedWeaponIndex++;
        if (scroll < 0) selectedWeaponIndex--;

        if (Input.GetKeyDown(KeyCode.Alpha1)) selectedWeaponIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) selectedWeaponIndex = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) selectedWeaponIndex = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) selectedWeaponIndex = 3;

        selectedWeaponIndex = Mathf.Clamp(selectedWeaponIndex, 0, weapons.Length - 1);

        // Log when weapon changes
        if (selectedWeaponIndex != previousWeaponIndex)
        {
            previousWeaponIndex = selectedWeaponIndex;
            LogSelectedWeapon();
        }
    }

    void LogSelectedWeapon()
    {
        if (showDebugLogs && weapons != null && selectedWeaponIndex < weapons.Length)
        {
            WeaponData weapon = weapons[selectedWeaponIndex];
            Debug.Log(
                $"[PlayerCombat] ═══ WEAPON SELECTED ═══\n" +
                $"Slot: {selectedWeaponIndex + 1}\n" +
                $"Name: {weapon.weaponName}\n" +
                $"Type: {weapon.hitboxType}\n" +
                $"Damage: {weapon.damage}\n" +
                $"Cooldown: {weapon.cooldown}s\n" +
                (weapon.hitboxType == HitboxType.AOE ? $"Radius: {weapon.radius}m" :
                 weapon.hitboxType == HitboxType.Cone ? $"Radius: {weapon.radius}m, Angle: {weapon.angle}°" :
                 $"Length: {weapon.length}m, Width: {weapon.width}m")
            );
        }
    }

    void HandleCooldown()
    {
        if (!canAttack)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= weapons[selectedWeaponIndex].cooldown)
            {
                cooldownTimer = 0f;
                canAttack = true;

                if (showDebugLogs)
                    Debug.Log($"[PlayerCombat] {weapons[selectedWeaponIndex].weaponName} ready!");
            }
        }
    }

    void DrawAttackLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("AttackLine");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        lr.material = attackLineMaterial != null
            ? attackLineMaterial
            : new Material(Shader.Find("Sprites/Default"));

        lr.startColor = attackColor;
        lr.endColor = attackColor;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.useWorldSpace = true;

        Destroy(lineObj, attackLineDuration);
    }

    void HandleAttackInput()
    {
        if (canAttack && Input.GetKeyDown(attackKey))
        {
            canAttack = false;
            PerformAttack(weapons[selectedWeaponIndex]);
        }
    }

    // -----------------------------
    // ATTACK LOGIC
    // -----------------------------

    void PerformAttack(WeaponData weapon)
    {
        Vector3 origin = attackOrigin != null ? attackOrigin.position : transform.position;

        if (showDebugLogs)
        {
            Debug.Log(
                $"[PlayerCombat] attack with {weapon.weaponName} | " +
                $"Type: {weapon.hitboxType}, Damage: {weapon.damage}"
            );
        }

        switch (weapon.hitboxType)
        {
            case HitboxType.AOE:
                AttackAOE(origin, weapon);
                break;

            case HitboxType.Cone:
                AttackCone(origin, weapon);
                break;

            case HitboxType.Straight:
                AttackStraight(origin, weapon);
                break;
        }
    }

    void AttackAOE(Vector3 origin, WeaponData weapon)
    {
        Collider[] hits = Physics.OverlapSphere(origin, weapon.radius, enemyLayer);
        DrawAttackLine(origin, origin + Vector3.up * weapon.radius);

        if (showDebugLogs)
            Debug.Log($"[AOE] Enemies hit: {hits.Length}");

        DamageEnemies(hits, weapon.damage);
    }

    void AttackCone(Vector3 origin, WeaponData weapon)
    {
        Collider[] hits = Physics.OverlapSphere(origin, weapon.radius, enemyLayer);

        int hitCount = 0;

        Vector3 left = Quaternion.Euler(0, -weapon.angle / 2f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, weapon.angle / 2f, 0) * transform.forward;

        DrawAttackLine(origin, origin + left * weapon.radius);
        DrawAttackLine(origin, origin + right * weapon.radius);

        foreach (Collider col in hits)
        {
            Vector3 dir = (col.transform.position - origin).normalized;
            float angleToEnemy = Vector3.Angle(transform.forward, dir);

            if (angleToEnemy <= weapon.angle * 0.5f)
            {
                DamageEnemy(col, weapon.damage);
                hitCount++;
            }
        }

        if (showDebugLogs)
            Debug.Log($"[Cone] Enemies hit: {hitCount}");
    }

    void AttackStraight(Vector3 origin, WeaponData weapon)
    {
        Vector3 center = origin + transform.forward * (weapon.length * 0.5f);
        Vector3 halfExtents = new Vector3(weapon.width * 0.5f, 1f, weapon.length * 0.5f);

        Collider[] hits = Physics.OverlapBox(
            center,
            halfExtents,
            transform.rotation,
            enemyLayer
        );

        if (showDebugLogs)
            Debug.Log($"[Straight] Enemies hit: {hits.Length}");

        DrawAttackLine(origin, origin + transform.forward * weapon.length);

        DamageEnemies(hits, weapon.damage);
    }

    // -----------------------------
    // DAMAGE
    // -----------------------------

    void DamageEnemies(Collider[] cols, float damage)
    {
        foreach (Collider col in cols)
        {
            DamageEnemy(col, damage);
        }
    }

    void DamageEnemy(Collider col, float damage)
    {
        EnemyBehavior enemy = col.GetComponentInParent<EnemyBehavior>();

        //if (enemy == null || enemy.IsDead)
           // return;

        if (showDebugLogs)
            Debug.Log($"[Hit] {enemy.name} takes {damage} damage");

        enemy.TakeDamage(damage);
    }

    // -----------------------------
    // GIZMOS
    // -----------------------------

    void OnDrawGizmos()
    {
        if (!showDebugGizmos || weapons == null || weapons.Length == 0)
            return;

        Vector3 origin = attackOrigin != null ? attackOrigin.position : transform.position;
        WeaponData weapon = weapons[selectedWeaponIndex];

        Gizmos.color = Color.red;

        switch (weapon.hitboxType)
        {
            case HitboxType.AOE:
                Gizmos.DrawWireSphere(origin, weapon.radius);
                break;

            case HitboxType.Cone:
                Vector3 left = Quaternion.Euler(0, -weapon.angle / 2f, 0) * transform.forward;
                Vector3 right = Quaternion.Euler(0, weapon.angle / 2f, 0) * transform.forward;

                Gizmos.DrawLine(origin, origin + left * weapon.radius);
                Gizmos.DrawLine(origin, origin + right * weapon.radius);
                Gizmos.DrawWireSphere(origin, weapon.radius);
                break;

            case HitboxType.Straight:
                Vector3 center = origin + transform.forward * (weapon.length * 0.5f);
                Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(weapon.width, 2f, weapon.length));
                Gizmos.matrix = Matrix4x4.identity;
                break;
        }
    }

    void OnDestroy()
    {
        if (rangeIndicator != null)
            Destroy(rangeIndicator);
    }
}