using UnityEngine;

public class BuffSpawner : MonoBehaviour
{
    private GameObject currentBuff;

    public void TrySpawnBuff(GameObject[] prefabs, float chance)
    {
        CleanUp();

        if (Random.value > chance) return;

        if (prefabs == null || prefabs.Length == 0) return;

        GameObject selectedPrefab = prefabs[Random.Range(0, prefabs.Length)];

        currentBuff = PoolManager.instance.Spawn(selectedPrefab, transform.position, transform.rotation);

        currentBuff.transform.SetParent(this.transform);
    }

    public void CleanUp()
    {
        if (currentBuff != null)
        {
            if (currentBuff.activeSelf)
            {
                PoolManager.instance.Despawn(currentBuff);
            }
            currentBuff = null;
        }
    }
}