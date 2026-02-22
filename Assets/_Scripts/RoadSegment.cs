using UnityEngine;
using System.Collections.Generic;

public class RoadSegment : MonoBehaviour
{
    public GameObject[] housePrefabs;

    public float roadHalfWidth = 6f;
    public float segmentLength = 20f;

    [Header("House placement")]
    public float houseSpacing = 5f;
    [Range(0f, 1f)]
    public float spawnChance = 1f;

    private List<GameObject> spawnedHouses = new List<GameObject>();

    void OnEnable()
    {
        SpawnHouses();
    }

    void OnDisable()
    {
        DespawnHouses();
    }

    void SpawnHouses()
    {
        int slots = Mathf.FloorToInt(segmentLength / houseSpacing);

        for (int i = 0; i < slots; i++)
        {
            if (Random.value > spawnChance)
                continue;

            float z = transform.position.z + i * houseSpacing;

            GameObject prefab = housePrefabs[Random.Range(0, housePrefabs.Length)];

            SpawnHouse(prefab, -roadHalfWidth, z, false);
            SpawnHouse(prefab, roadHalfWidth, z, true);
        }
    }

    void SpawnHouse(GameObject prefab, float x, float z, bool mirrored)
    {
        Vector3 pos = new Vector3(x, 0f, z);

        GameObject house = PoolManager.instance.Spawn(
            prefab,
            pos,
            Quaternion.identity
        );

        house.transform.SetParent(transform);

        Vector3 scale = house.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (mirrored ? -1 : 1);
        house.transform.localScale = scale;

        spawnedHouses.Add(house);
    }

    void DespawnHouses()
    {
        foreach (var house in spawnedHouses)
        {
            PoolManager.instance.Despawn(house);
        }
        spawnedHouses.Clear();
    }
}