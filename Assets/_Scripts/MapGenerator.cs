using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance;

    [Header("Settings")]
    public float mapLength = 20f;
    public float destroyZ = -10f;

    [Header("Buff Spawn Settings")]
    public GameObject[] buffPrefabs;
    [Range(0, 1)] public float globalBuffChance = 0.2f;

    public enum LaneType { Any, LeftFree, RightFree, MiddleFree, AllBlocked }

    [System.Serializable]
    public class MapData
    {
        public GameObject mapPrefab;
        [Header("Speed Range")]
        public int minSpeedLevel;
        public int maxSpeedLevel = 99;

        [Header("Logic")]
        public LaneType entryRequirement;
        public LaneType exitStatus;
    }

    public List<MapData> allMaps = new List<MapData>();
    public List<GameObject> activeMaps = new List<GameObject>();

    private MapData lastMapSpawned;
    private float lastSpawnLocalZ = 0f;

    void Awake() => Instance = this;

    void Update()
    {
        if (RoadGenerator.instance == null || RoadGenerator.instance.speed <= 0) return;

        float move = RoadGenerator.instance.speed * Time.deltaTime;

        if (activeMaps.Count > 0)
        {
            float endOfMapWorldZ = transform.position.z + activeMaps[0].transform.localPosition.z + mapLength;
            if (endOfMapWorldZ < destroyZ)
            {
                RemoveFirstActiveMap();
                AddActiveMap();
            }
        }
    }

    public void ResetMaps()
    {
        while (activeMaps.Count > 0) RemoveFirstActiveMap();

        transform.position = Vector3.zero;
        lastSpawnLocalZ = 0f;
        lastMapSpawned = null;

        for (int i = 0; i < 5; i++) AddActiveMap();
    }

    void RemoveFirstActiveMap()
    {
        if (activeMaps.Count == 0) return;

        BuffSpawner[] spawners = activeMaps[0].GetComponentsInChildren<BuffSpawner>();
        foreach (var s in spawners) s.CleanUp();

        MapCoinController coinCtrl = activeMaps[0].GetComponent<MapCoinController>();
        if (coinCtrl != null) coinCtrl.DeactivateAll();

        PoolManager.instance.Despawn(activeMaps[0]);
        activeMaps.RemoveAt(0);
    }

    public void SetupContentForRoad(GameObject road)
    {
        MapCoinController coinController = road.GetComponentInChildren<MapCoinController>();

        if (coinController != null)
        {
            coinController.ActivateRandomPattern();
        }
    }

    void AddActiveMap()
    {

        int currentLevel = (activeMaps.Count < 2) ? 0 : SpeedManager.Instance.CurrentLevel;

        List<MapData> candidates = new List<MapData>();

        foreach (var m in allMaps)
        {
            if (currentLevel < m.minSpeedLevel || currentLevel > m.maxSpeedLevel)
                continue;

            if (lastMapSpawned == null ||
                m.entryRequirement == LaneType.Any ||
                m.entryRequirement == lastMapSpawned.exitStatus)
            {
                candidates.Add(m);
            }
        }

        if (candidates.Count == 0)
        {
            candidates = allMaps.FindAll(m => currentLevel >= m.minSpeedLevel && currentLevel <= m.maxSpeedLevel);
        }

        if (candidates.Count == 0)
        {
            candidates = allMaps.FindAll(m => m.minSpeedLevel == 0);
        }

        if (candidates.Count == 0) return;

        MapData chosen = candidates[Random.Range(0, candidates.Count)];
        GameObject go = PoolManager.instance.Spawn(chosen.mapPrefab, Vector3.zero, Quaternion.identity);

        go.transform.SetParent(this.transform);
        go.transform.localPosition = new Vector3(0, 0, lastSpawnLocalZ);

        activeMaps.Add(go);
        lastMapSpawned = chosen;
        lastSpawnLocalZ += mapLength;

        MapCoinController coinCtrl = go.GetComponent<MapCoinController>();
        if (coinCtrl != null)
        {
            coinCtrl.ActivateRandomPattern();
        }
        else
        {
            Debug.LogWarning($"[MapGenerator] На префабе '{chosen.mapPrefab.name}' нет MapCoinController!");
        }

        BuffSpawner[] spawners = go.GetComponentsInChildren<BuffSpawner>();
        if (spawners != null && spawners.Length > 0)
        {
            foreach (var s in spawners)
            {
                if (s != null)
                {
                    s.TrySpawnBuff(buffPrefabs, globalBuffChance);
                }
            }
        }
    }
}