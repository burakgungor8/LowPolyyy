using UnityEngine;
using UnityEngine.AI;

public class QuickMinionTest : MonoBehaviour
{
    [Header("Test Ayarları")]
    public Transform[] testWaypoints;
    public float speed = 3f;
    
    private NavMeshAgent agent;
    private int currentWP = 0;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        
        if (testWaypoints.Length > 0)
        {
            agent.SetDestination(testWaypoints[0].position);
            Debug.Log("Test başladı! İlk waypoint'e gidiyor...");
        }
    }
    
    void Update()
    {
        if (agent.remainingDistance < 1f && !agent.pathPending)
        {
            currentWP++;
            if (currentWP < testWaypoints.Length)
            {
                agent.SetDestination(testWaypoints[currentWP].position);
                Debug.Log($"Waypoint {currentWP}'e gidiyor...");
            }
            else
            {
                Debug.Log("Tüm waypoint'ler tamamlandı!");
            }
        }
    }
}