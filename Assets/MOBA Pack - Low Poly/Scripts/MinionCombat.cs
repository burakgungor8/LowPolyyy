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
    public LayerMask enemyLayerMask = -1; // Hangi layer'ları düşman olarak görsün
    public float detectionRange = 3f;
    
    // Events
    public Action OnMinionDeath;
    
    // States
    private enum MinionState { Moving, Fighting, Dead }
    private MinionState currentState = MinionState.Moving;
    
    void Start()
    {
        InitializeComponents();
        InitializeHealth();
        SetupNavigation();
    }
    
    void InitializeComponents()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component missing on " + gameObject.name);
        }
        else
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange * 0.8f; // Saldırı menzilinden biraz önce dur
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
        if (waypoints != null && waypoints.Length > 0 && agent != null)
        {
            agent.SetDestination(waypoints[0].position);
        }
    }
    
    void Update()
    {
        if (currentState == MinionState.Dead) return;
        
        attackTimer -= Time.deltaTime;
        
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
        if (agent == null || waypoints == null || waypoints.Length == 0) return;
        
        // Hedefe ulaştı mı?
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentWaypointIndex++;
            
            if (currentWaypointIndex >= waypoints.Length)
            {
                // Son waypoint'e ulaştı - düşman base'ine saldır
                reachedDestination = true;
                Debug.Log(gameObject.name + " reached enemy base!");
                return;
            }
            
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }
    
    void CheckForEnemies()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRange, enemyLayerMask);
        Transform nearestEnemy = null;
        float nearestDistance = Mathf.Infinity;
        
        foreach (Collider enemy in enemies)
        {
            // Kendi takımından değilse
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
            
            // NavMeshAgent'ı durdur
            if (agent != null)
            {
                agent.isStopped = true;
            }
        }
    }
    
    void HandleCombat()
    {
        if (target == null || IsTargetDead())
        {
            // Hedef yok veya öldü, tekrar hareket et
            target = null;
            currentState = MinionState.Moving;
            
            if (agent != null)
            {
                agent.isStopped = false;
            }
            return;
        }
        
        float distance = Vector3.Distance(transform.position, target.position);
        
        if (distance <= attackRange)
        {
            // Saldırı menzilinde
            LookAtTarget();
            
            if (attackTimer <= 0f)
            {
                Attack();
                attackTimer = attackCooldown;
            }
        }
        else if (distance > detectionRange)
        {
            // Çok uzaklaştı, bırak gitsin
            target = null;
            currentState = MinionState.Moving;
            
            if (agent != null)
            {
                agent.isStopped = false;
            }
        }
        else
        {
            // Hedefe yaklaş
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
            direction.y = 0; // Y ekseninde dönmesin
            
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
        
        Debug.Log(gameObject.name + " attacked " + target.name);
        
        // Düşmana hasar ver
        MinionController enemyMinion = target.GetComponent<MinionController>();
        if (enemyMinion != null)
        {
            enemyMinion.TakeDamage(damage);
        }
        
        // Kuleye hasar ver (Tower script'in varsa)
        ITakeDamage damageable = target.GetComponent<ITakeDamage>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
        
        // Attack animasyonu burada tetiklenebilir
        // GetComponent<Animator>()?.SetTrigger("Attack");
    }
    
    bool IsEnemy(GameObject obj)
    {
        MinionController otherMinion = obj.GetComponent<MinionController>();
        if (otherMinion != null)
        {
            return otherMinion.team != this.team;
        }
        
        // Kule kontrolü - layer veya tag ile
        if (obj.CompareTag("Tower"))
        {
            // Kule takım kontrolü burada yapılabilir
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
        
        // Ölüm animasyonu
        // GetComponent<Animator>()?.SetTrigger("Die");
        
        Destroy(gameObject, 0.5f); // Küçük bir gecikme ile yok et
    }
    
    public bool IsDead()
    {
        return currentHealth <= 0;
    }
    
    // Debug için görselleştirme
    void OnDrawGizmosSelected()
    {
        // Waypoint'leri göster
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
        }
        
        // Saldırı ve tespit menzilini göster
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}

// Interface for damage system
public interface ITakeDamage
{
    void TakeDamage(int damage);
}