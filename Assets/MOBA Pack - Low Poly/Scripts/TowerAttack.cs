using UnityEngine;

public class TowerAttack : MonoBehaviour
{
    public float range = 10f;
    public float fireRate = 1f;
    public int damage = 20;
    public LayerMask enemyLayer;

    private float nextFireTime = 0f;
    private Transform currentTarget;

    void Update()
    {
        if (currentTarget == null || !IsTargetInRange(currentTarget))
        {
            FindFirstTargetInRange();
        }

        if (currentTarget != null && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    void FindFirstTargetInRange()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range, enemyLayer);
        Debug.LogWarning($"Found {hits.Length} targets in range.");
        foreach (Collider hit in hits)
        {
            MinionHealth mh = hit.GetComponent<MinionHealth>();
            if (mh != null && !mh.IsDead())  // Bu fonksiyon varsa, yoksa currentHealth > 0 kontrolü ekleriz
            {
                currentTarget = hit.transform;
                break; // İlk bulduğunu al
            }
        }
    }

    bool IsTargetInRange(Transform target)
    {
        return Vector3.Distance(transform.position, target.position) <= range;
    }

    void Shoot()
    {
        MinionHealth mh = currentTarget.GetComponent<MinionHealth>();
        if (mh != null)
        {
            mh.TakeDamage(damage);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
