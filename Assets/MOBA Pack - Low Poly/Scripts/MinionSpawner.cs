using UnityEngine;
using System.Collections;

public class MinionSpawner : MonoBehaviour
{
    [Header("Minyon Prefabları")]
    public GameObject teamAMinionPrefab; 
    public GameObject teamBMinionPrefab; 
    
    [Header("Spawn Noktaları")]
    public Transform teamASpawnPoint;  
    public Transform teamBSpawnPoint;  
    
    [Header("Waypoint'ler (Lane Yolu)")]
    public Transform[] teamAWaypoints; 
    public Transform[] teamBWaypoints; 
    
    [Header("Spawn Ayarları")]
    public float spawnInterval = 20f;  
    public int minionsPerWave = 3;    
    public float minionSpawnDelay = 1f;  
    public bool autoSpawn = true; 
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private float spawnTimer;
    private int activeMinionsTeamA = 0;
    private int activeMinionsTeamB = 0;
    
    void Start()
    {
        spawnTimer = spawnInterval;

        if (autoSpawn)
        {
            SpawnWave();
        }
        
        ValidateSetup();
    }
    
    void Update()
    {
        if (!autoSpawn) return;
        
        spawnTimer -= Time.deltaTime;
        
        if (spawnTimer <= 0f)
        {
            SpawnWave();
            spawnTimer = spawnInterval;
        }
    }
    
    void ValidateSetup()
    {
        bool hasError = false;
        
        if (teamAMinionPrefab == null)
        {
            Debug.LogError("Team A Minion Prefab atanmamış!");
            hasError = true;
        }
        
        if (teamBMinionPrefab == null)
        {
            Debug.LogError("Team B Minion Prefab atanmamış!");
            hasError = true;
        }
        
        if (teamASpawnPoint == null)
        {
            Debug.LogError("Team A Spawn Point atanmamış!");
            hasError = true;
        }
        
        if (teamBSpawnPoint == null)
        {
            Debug.LogError("Team B Spawn Point atanmamış!");
            hasError = true;
        }
        
        if (teamAWaypoints == null || teamAWaypoints.Length == 0)
        {
            Debug.LogError("Team A Waypoints boş!");
            hasError = true;
        }
        
        if (teamBWaypoints == null || teamBWaypoints.Length == 0)
        {
            Debug.LogError("Team B Waypoints boş!");
            hasError = true;
        }
        
        if (!hasError)
        {
            Debug.Log("Minyon spawner kurulumu başarılı!");
        }
    }
    
    public void SpawnWave()
    {
        StartCoroutine(SpawnWaveCoroutine());
    }
    
    IEnumerator SpawnWaveCoroutine()
    {
        Debug.Log("Yeni minyon dalgası spawn ediliyor.");
        
        for (int i = 0; i < minionsPerWave; i++)
        {
            SpawnMinion(teamAMinionPrefab, teamASpawnPoint, teamAWaypoints, "TeamA");
            yield return new WaitForSeconds(minionSpawnDelay);
        }
        
        for (int i = 0; i < minionsPerWave; i++)
        {
            SpawnMinion(teamBMinionPrefab, teamBSpawnPoint, teamBWaypoints, "TeamB");
            yield return new WaitForSeconds(minionSpawnDelay);
        }
    }
    
    void SpawnMinion(GameObject prefab, Transform spawnPoint, Transform[] waypoints, string team)
    {
        if (prefab == null || spawnPoint == null || waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning($"Minyon spawn edilemedi - eksik referans ({team})");
            return;
        }
    
        GameObject minion = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        
        MinionController controller = minion.GetComponent<MinionController>();
        if (controller != null)
        {
            if (team == "TeamA")
            {
                controller.team = MinionController.TeamType.TeamA;
                activeMinionsTeamA++;
            }
            else
            {
                controller.team = MinionController.TeamType.TeamB;
                activeMinionsTeamB++;
            }
            
            controller.waypoints = waypoints;
            
            controller.OnMinionDeath += () => OnMinionDied(team);
        }
        
        if (team == "TeamA")
        {
            int teamALayer = LayerMask.NameToLayer("TeamA");
            if (teamALayer != -1) minion.layer = teamALayer;
        }
        else
        {
            int teamBLayer = LayerMask.NameToLayer("TeamB");
            if (teamBLayer != -1) minion.layer = teamBLayer;
        }
        
        Debug.Log($"{team} minyonu spawn edildi!");
    }
    
    void OnMinionDied(string team)
    {
        if (team == "TeamA")
        {
            activeMinionsTeamA--;
        }
        else
        {
            activeMinionsTeamB--;
        }
        
        Debug.Log($"{team} minyonu öldü. Aktif minyonlar: A={activeMinionsTeamA}, B={activeMinionsTeamB}");
    }
    
    [ContextMenu("Test Spawn Wave")]
    public void TestSpawnWave()
    {
        SpawnWave();
    }
    
    [ContextMenu("Spawn Team A Only")]
    public void SpawnTeamAOnly()
    {
        StartCoroutine(SpawnTeamOnly("TeamA"));
    }
    
    [ContextMenu("Spawn Team B Only")]
    public void SpawnTeamBOnly()
    {
        StartCoroutine(SpawnTeamOnly("TeamB"));
    }
    
    IEnumerator SpawnTeamOnly(string team)
    {
        for (int i = 0; i < minionsPerWave; i++)
        {
            if (team == "TeamA")
            {
                SpawnMinion(teamAMinionPrefab, teamASpawnPoint, teamAWaypoints, "TeamA");
            }
            else
            {
                SpawnMinion(teamBMinionPrefab, teamBSpawnPoint, teamBWaypoints, "TeamB");
            }
            yield return new WaitForSeconds(minionSpawnDelay);
        }
    }
    
    void OnGUI()
    {
        if (!showDebugInfo || !Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Box("Minyon Spawner Debug");
        
        GUILayout.Label($"Sonraki dalga: {spawnTimer:F1} saniye");
        GUILayout.Label($"Aktif Team A minyonları: {activeMinionsTeamA}");
        GUILayout.Label($"Aktif Team B minyonları: {activeMinionsTeamB}");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Manuel Dalga Spawn Et"))
        {
            SpawnWave();
        }
        
        if (GUILayout.Button("Sadece Team A"))
        {
            SpawnTeamAOnly();
        }
        
        if (GUILayout.Button("Sadece Team B"))
        {
            SpawnTeamBOnly();
        }
        
        GUILayout.EndArea();
    }

    void OnDrawGizmos()
    {
        if (teamAWaypoints != null && teamAWaypoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < teamAWaypoints.Length; i++)
            {
                if (teamAWaypoints[i] != null)
                {
                    Gizmos.DrawWireSphere(teamAWaypoints[i].position, 0.5f);
                    
                    if (i < teamAWaypoints.Length - 1 && teamAWaypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(teamAWaypoints[i].position, teamAWaypoints[i + 1].position);
                    }
                }
            }
        }
        
        if (teamBWaypoints != null && teamBWaypoints.Length > 0)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < teamBWaypoints.Length; i++)
            {
                if (teamBWaypoints[i] != null)
                {
                    Gizmos.DrawWireSphere(teamBWaypoints[i].position, 0.5f);
                    
                    if (i < teamBWaypoints.Length - 1 && teamBWaypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(teamBWaypoints[i].position, teamBWaypoints[i + 1].position);
                    }
                }
            }
        }
        
        if (teamASpawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(teamASpawnPoint.position, Vector3.one);
        }
        
        if (teamBSpawnPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(teamBSpawnPoint.position, Vector3.one);
        }
    }
}