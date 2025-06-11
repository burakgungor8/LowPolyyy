using UnityEngine;
using UnityEngine.AI;
using System;

public class MinionController : MonoBehaviour
{
    [Header("Team Settings")]
    public TeamType team;
    public enum TeamType { TeamA, TeamB }
    
    [Header("Navigation")]
    public Transform[] waypoints;
    public float moveSpeed = 2f;
    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private bool reachedDestination = false;
    
    [Header("Combat")]
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public int damage = 10;
    private Transform target;
    private float attackTimer = 0f;
    
    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;
    
    [Header("HP Bar")]
    public GameObject healthBarPrefab;
    public Transform healthBarPosition;
    private FloatingHealthBar healthBar;
    
    [Header("Detection")]
    public LayerMask enemyLayerMask = -1;
    public float detectionRange = 3f;
    
    [Header("Debug - Kontrol İçin")]
    public bool showDebugLogs = true;
    
    // Events
    public Action OnMinionDeath;
    
    // States
    private enum MinionState { Moving, Fighting, Dead }
    private MinionState currentState = MinionState.Moving;
    
    void Start()
    {
        InitializeComponents();
        InitializeHealth();
        
        Debug.Log($"{name}: Waypoints = {(waypoints?.Length ?? 0)}");
        
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError($"{name}: Waypoints YOK! Spawner'dan gelmedi.");
            return;
        }
        
        if (waypoints[0] == null)
        {
            Debug.LogError($"{name}: İlk waypoint null!");
            return;
        }
        
        SetupNavigation();
        
        if (agent != null)
        {
            currentState = MinionState.Moving;
            agent.SetDestination(waypoints[0].position);
            Debug.Log($"{name}: İlk waypoint'e gidiyor! Hedef: {waypoints[0].position}");
        }
    }
    
    void InitializeComponents()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError($"{name}: NavMeshAgent component missing!");
            return;
        }
        
        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange * 0.8f;
        agent.autoBraking = true;
        
        if (showDebugLogs)
        {
            Debug.Log($"{name}: NavMeshAgent ayarlandı (Speed: {moveSpeed})");
        }
    }
    
    void InitializeHealth()
    {
        currentHealth = maxHealth;
        
        if (healthBarPrefab != null && healthBarPosition != null)
        {
            GameObject hb = Instantiate(healthBarPrefab, healthBarPosition.position, Quaternion.identity, transform);
            healthBar = hb.GetComponent<FloatingHealthBar>();
            if (healthBar != null)
            {
                healthBar.followTarget = healthBarPosition;
                healthBar.SetMaxHealth(maxHealth);
            }
        }
    }
    
    void SetupNavigation()
    {
        if (waypoints != null && waypoints.Length > 0 && agent != null && waypoints[0] != null)
        {
            agent.SetDestination(waypoints[0].position);
            Debug.Log($"{name}: Navigation kuruldu, hedef: {waypoints[0].position}");
        }
    }
    
    void Update()
    {
        if (currentState == MinionState.Dead) return;
        
        attackTimer -= Time.deltaTime;
        
        if (showDebugLogs && Time.frameCount % 120 == 0)
        {
            if (agent != null)
            {
                Debug.Log($"{name}: Durum={currentState}, Waypoint={currentWaypointIndex}/{waypoints?.Length}, Mesafe={agent.remainingDistance:F1}");
            }
        }
        
        switch (currentState)
        {
            case MinionState.Moving:
                HandleMovement();
                CheckForEnemies();
                break;
                
            case MinionState.Fighting:
                HandleCombat();
                break;
        }
    }
    
    void HandleMovement()
    {
        if (agent == null || waypoints == null || waypoints.Length == 0) 
        {
            if (showDebugLogs)
                Debug.LogWarning($"⚠️ {name}: Agent veya waypoints eksik!");
            return;
        }

        if (agent.pathPending)
        {
            return;
        }
        
        if (agent.remainingDistance < 0.8f)
        {
            if (showDebugLogs)
                Debug.Log($"{name}: Waypoint {currentWaypointIndex} tamamlandı!");
            
            currentWaypointIndex++;
            
            if (currentWaypointIndex >= waypoints.Length)
            {
                reachedDestination = true;
                if (showDebugLogs)
                    Debug.Log($"{name}: Son waypoint'e ulaştı! Düşman base'ine saldırıyor!");
                return;
            }
            
            if (waypoints[currentWaypointIndex] != null)
            {
                agent.SetDestination(waypoints[currentWaypointIndex].position);
                if (showDebugLogs)
                    Debug.Log($"{name}: Yeni hedef - Waypoint {currentWaypointIndex}: {waypoints[currentWaypointIndex].position}");
            }
            else
            {
                Debug.LogError($"{name}: Waypoint {currentWaypointIndex} null!");
            }
        }
    }
    
    void CheckForEnemies()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRange, enemyLayerMask);
        Transform nearestEnemy = null;
        float nearestDistance = Mathf.Infinity;
        
        foreach (Collider enemy in enemies)
        {
            if (IsEnemy(enemy.gameObject))
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy.transform;
                }
            }
        }
        
        if (nearestEnemy != null)
        {
            target = nearestEnemy;
            currentState = MinionState.Fighting;
            
            if (agent != null)
            {
                agent.isStopped = true;
            }
            
            if (showDebugLogs)
                Debug.Log($"⚔️ {name}: Düşman tespit edildi, savaş moduna geçiliyor!");
        }
    }
    
    void HandleCombat()
    {
        if (target == null || IsTargetDead())
        {
            target = null;
            currentState = MinionState.Moving;
            
            if (agent != null)
            {
                agent.isStopped = false;
                if (currentWaypointIndex < waypoints.Length && waypoints[currentWaypointIndex] != null)
                {
                    agent.SetDestination(waypoints[currentWaypointIndex].position);
                }
            }
            
            if (showDebugLogs)
                Debug.Log($"🚶 {name}: Savaş bitti, harekete devam!");
            return;
        }
        
        float distance = Vector3.Distance(transform.position, target.position);
        
        if (distance <= attackRange)
        {
            LookAtTarget();
            
            if (attackTimer <= 0f)
            {
                Attack();
                attackTimer = attackCooldown;
            }
        }
        else if (distance > detectionRange)
        {
            target = null;
            currentState = MinionState.Moving;
            
            if (agent != null)
            {
                agent.isStopped = false;
            }
        }
        else
        {
            if (agent != null)
            {
                agent.isStopped = false;
                agent.SetDestination(target.position);
            }
        }
    }
    
    void LookAtTarget()
    {
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;
            
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }
        }
    }
    
    void Attack()
    {
        if (target == null) return;
        
        if (showDebugLogs)
            Debug.Log($"⚔️ {name} saldırdı: {target.name}");
        
        MinionController enemyMinion = target.GetComponent<MinionController>();
        if (enemyMinion != null)
        {
            enemyMinion.TakeDamage(damage);
        }
        
        ITakeDamage damageable = target.GetComponent<ITakeDamage>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
    }
    
    bool IsEnemy(GameObject obj)
    {
        MinionController otherMinion = obj.GetComponent<MinionController>();
        if (otherMinion != null)
        {
            return otherMinion.team != this.team;
        }
        
        if (obj.CompareTag("Tower"))
        {
            return obj.layer != gameObject.layer;
        }
        
        return false;
    }
    
    bool IsTargetDead()
    {
        if (target == null) return true;
        
        MinionController enemyMinion = target.GetComponent<MinionController>();
        if (enemyMinion != null)
        {
            return enemyMinion.IsDead();
        }
        
        return false;
    }
    
    public void TakeDamage(int amount)
    {
        if (currentState == MinionState.Dead) return;
        
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        if (healthBar != null)
        {
            healthBar.TakeDamage(amount);
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        currentState = MinionState.Dead;
        
        if (agent != null)
        {
            agent.isStopped = true;
        }
        
        OnMinionDeath?.Invoke();
        
        if (showDebugLogs)
            Debug.Log($"💀 {name}: Öldü!");
        
        Destroy(gameObject, 0.5f);
    }
    
    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    [ContextMenu("Debug Current State")]
    public void DebugCurrentState()
    {
        Debug.Log($"🔍 {name} Debug:");
        Debug.Log($"   State: {currentState}");
        Debug.Log($"   Waypoints: {waypoints?.Length ?? 0}");
        Debug.Log($"   Current WP: {currentWaypointIndex}");
        Debug.Log($"   Agent: {(agent != null ? "✅" : "❌")}");
        if (agent != null)
        {
            Debug.Log($"   Remaining Distance: {agent.remainingDistance:F1}");
            Debug.Log($"   Has Path: {agent.hasPath}");
            Debug.Log($"   Path Pending: {agent.pathPending}");
        }
    }
    
    [ContextMenu("Force Move to Next Waypoint")]
    public void ForceNextWaypoint()
    {
        if (waypoints != null && currentWaypointIndex < waypoints.Length - 1 && agent != null)
        {
            currentWaypointIndex++;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
            Debug.Log($"🎯 Manuel: Waypoint {currentWaypointIndex}'e gidiyor");
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            Gizmos.color = team == TeamType.TeamA ? Color.blue : Color.red;
            
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawWireSphere(waypoints[i].position, 0.5f);
                    
                    if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    }
                }
            }
            
            if (Application.isPlaying && currentWaypointIndex < waypoints.Length && waypoints[currentWaypointIndex] != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(waypoints[currentWaypointIndex].position, 0.8f);
            }
        }
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        if (Application.isPlaying && agent != null && agent.hasPath)
        {
            Gizmos.color = Color.green;
            Vector3[] pathCorners = agent.path.corners;
            
            for (int i = 0; i < pathCorners.Length - 1; i++)
            {
                Gizmos.DrawLine(pathCorners[i], pathCorners[i + 1]);
            }
        }
    }
}

public interface ITakeDamage
{
    void TakeDamage(int damage);
}

public class MinionHealth : MonoBehaviour
{
    private MinionController controller;
    
    void Start()
    {
        controller = GetComponent<MinionController>();
    }
    
    public void TakeDamage(int amount)
    {
        if (controller != null)
        {
            controller.TakeDamage(amount);
        }
    }
    
    public bool IsDead()
    {
        if (controller != null)
        {
            return controller.IsDead();
        }
        return true;
    }
}