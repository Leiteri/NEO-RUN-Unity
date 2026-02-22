using UnityEngine;
using System.Collections.Generic;

public class MapCoinController : MonoBehaviour
{
    [Tooltip("Список пустых объектов, внутри которых лежат монеты")]
    public List<GameObject> coinVariants;

    public void ActivateRandomPattern()
    {
        DeactivateAll();

        if (coinVariants.Count == 0) return;

        bool allLanesActive = BuffManager.Instance != null && BuffManager.Instance.allLanesActive;

        if (allLanesActive)
        {
            foreach (GameObject variant in coinVariants)
            {
                ActivateVariant(variant);
            }
        }
        else
        {
            if (Random.value < 0.7f)
            {
                int index = Random.Range(0, coinVariants.Count);
                ActivateVariant(coinVariants[index]);
            }
        }
    }

    private void ActivateVariant(GameObject variant)
    {
        variant.SetActive(true);

        foreach (Transform child in variant.transform)
        {
            child.gameObject.SetActive(true);
            CoinController cc = child.GetComponent<CoinController>();
            if (cc != null) cc.ResetCoin();
        }
    }

    public void DeactivateAll()
    {
        foreach (var variant in coinVariants)
        {
            variant.SetActive(false);
        }
    }
}