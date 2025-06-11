using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Lane
{
    public string laneName = "Mid Lane";
    public Transform[] teamAWaypoints; // Team A'nın gideceği waypoint'ler
    public Transform[] teamBWaypoints; // Team B'nin gideceği waypoint'ler
    public Transform teamASpawnPoint;
    public Transform teamBSpawnPoint;
    public int maxMinionsPerTeam = 3;
    
    [HideInInspector]
    public int currentTeamAMinions = 0;
    [HideInInspector]
    public int currentTeamBMinions = 0;
}

public class MinionSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject teamAPrefab;
    public GameObject teamBPrefab;
    
    [Header("Lanes")]
    public Lane[] lanes = new Lane[3]; // Top, Mid, Bot
    
    [Header("Spawn Settings")]
    public float spawnInterval = 20f;
    public bool autoSpawn = true;
    public bool spawnOnStart = true;
    
    [Header("Wave Settings")]
    public int minionsPerWave = 3;
    public float minionSpawnDelay = 1f; // Dalga içinde minyonlar arası süre
    
    private float timer;
    private List<GameObject> allMinions = new List<GameObject>();
    
    void Start()
    {
        timer = spawnInterval;
        
        // Başlangıçta spawn et
        if (spawnOnStart)
        {
            SpawnWaveInAllLanes();
        }
        
        // Lane'lerin doğru kurulup kurulmadığını kontrol et
        ValidateLanes();
    }
    
    void Update()
    {
        if (!autoSpawn) return;
        
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnWaveInAllLanes();
            timer = spawnInterval;
        }
    }
    
    void ValidateLanes()
    {
        for (int i = 0; i < lanes.Length; i++)
        {
            Lane lane = lanes[i];
            
            if (lane.teamAWaypoints == null || lane.teamAWaypoints.Length == 0)
            {
                Debug.LogWarning($"Lane {i} ({lane.laneName}) has no Team A waypoints!");
            }
            
            if (lane.teamBWaypoints == null || lane.teamBWaypoints.Length == 0)
            {
                Debug.LogWarning($"Lane {i} ({lane.laneName}) has no Team B waypoints!");
            }
            
            if (lane.teamASpawnPoint == null)
            {
                Debug.LogWarning($"Lane {i} ({lane.laneName}) has no Team A spawn point!");
            }
            
            if (lane.teamBSpawnPoint == null)
            {
                Debug.LogWarning($"Lane {i} ({lane.laneName}) has no Team B spawn point!");
            }
        }
    }
    
    public void SpawnWaveInAllLanes()
    {
        foreach (Lane lane in lanes)
        {
            StartCoroutine(SpawnWaveInLane(lane));
        }
    }
    
    System.Collections.IEnumerator SpawnWaveInLane(Lane lane)
    {
        // Team A minyonları spawn et
        for (int i = 0; i < minionsPerWave; i++)
        {
            if (lane.currentTeamAMinions < lane.maxMinionsPerTeam)
            {
                SpawnMinion(teamAPrefab, lane, MinionController.TeamType.TeamA);
                yield return new WaitForSeconds(minionSpawnDelay);
            }
        }
        
        // Team B minyonları spawn et
        for (int i = 0; i < minionsPerWave; i++)
        {
            if (lane.currentTeamBMinions < lane.maxMinionsPerTeam)
            {
                SpawnMinion(teamBPrefab, lane, MinionController.TeamType.TeamB);
                yield return new WaitForSeconds(minionSpawnDelay);
            }
        }
    }
    
    void SpawnMinion(GameObject prefab, Lane lane, MinionController.TeamType team)
    {
        if (prefab == null) return;
        
        Transform spawnPoint = (team == MinionController.TeamType.TeamA) ? 
            lane.teamASpawnPoint : lane.teamBSpawnPoint;
            
        Transform[] waypoints = (team == MinionController.TeamType.TeamA) ? 
            lane.teamAWaypoints : lane.teamBWaypoints;
        
        if (spawnPoint == null || waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning($"Cannot spawn minion for {team} in lane {lane.laneName} - missing spawn point or waypoints");
            return;
        }
        
        // Minyon spawn et
        GameObject minion = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        allMinions.Add(minion);
        
        // MinionController'ı ayarla
        MinionController controller = minion.GetComponent<MinionController>();
        if (controller != null)
        {
            controller.team = team;
            controller.waypoints = waypoints;
            
            // Ölüm event'ini dinle
            controller.OnMinionDeath += () => OnMinionDeath(minion, lane, team);
            
            // Takım layer'ını ayarla
            int teamLayer = (team == MinionController.TeamType.TeamA) ? 
                LayerMask.NameToLayer("TeamA") : LayerMask.NameToLayer("TeamB");
            
            if (teamLayer != -1)
            {
                minion.layer = teamLayer;
            }
        }
        
        // Sayacı artır
        if (team == MinionController.TeamType.TeamA)
        {
            lane.currentTeamAMinions++;
        }
        else
        {
            lane.currentTeamBMinions++;
        }
        
        Debug.Log($"Spawned {team} minion in {lane.laneName}");
    }
    
    void OnMinionDeath(GameObject minion, Lane lane, MinionController.TeamType team)
    {
        // Listeden çıkar
        allMinions.Remove(minion);
        
        // Sayacı azalt
        if (team == MinionController.TeamType.TeamA)
        {
            lane.currentTeamAMinions--;
        }
        else
        {
            lane.currentTeamBMinions--;
        }
        
        Debug.Log($"{team} minion died in {lane.laneName}");
    }
    
    // Manuel spawn fonksiyonları
    public void SpawnWaveInLane(int laneIndex)
    {
        if (laneIndex >= 0 && laneIndex < lanes.Length)
        {
            StartCoroutine(SpawnWaveInLane(lanes[laneIndex]));
        }
    }
    
    public void ForceSpawnAll()
    {
        SpawnWaveInAllLanes();
    }
    
    // Debug bilgileri
    void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Minion Spawner Debug");
        GUILayout.Label($"Active Minions: {allMinions.Count}");
        
        foreach (Lane lane in lanes)
        {
            GUILayout.Label($"{lane.laneName}: A={lane.currentTeamAMinions}, B={lane.currentTeamBMinions}");
        }
        
        if (GUILayout.Button("Force Spawn Wave"))
        {
            ForceSpawnAll();
        }
        
        GUILayout.EndArea();
    }
}