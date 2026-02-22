using UnityEngine;

public class BuffItem : MonoBehaviour
{
    [Header("Settings")]
    public BuffType type;
    public float duration = 10f; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (BuffManager.Instance != null)
            {
                BuffManager.Instance.ActivateBuff(type, duration);
            }

            gameObject.SetActive(false);
        }
    }
}