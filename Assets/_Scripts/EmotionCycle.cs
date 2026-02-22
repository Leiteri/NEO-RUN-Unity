using UnityEngine;

public class EmotionCycle : MonoBehaviour
{
    [Tooltip("Материал с эмиссией, к которому подключен атлас")]
    public Material targetMaterial;

    [Tooltip("Сколько времени держать каждую эмоцию (в секундах)")]
    public float emotionDuration = 1f;

    private int currentEmotion = 0;
    private float timer = 0f;
    private const int totalEmotions = 4;

    void Start()
    {
        if (targetMaterial == null)
        {
            Debug.LogWarning("Материал не назначен!");
            enabled = false;
            return;
        }

        targetMaterial.EnableKeyword("_EMISSION");
        targetMaterial.SetColor("_EmissionColor", Color.white);

        ShowEmotion(currentEmotion);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= emotionDuration)
        {
            timer = 0f;
            currentEmotion = (currentEmotion + 1) % totalEmotions;
            ShowEmotion(currentEmotion);
        }
    }

    void ShowEmotion(int index)
    {
        float tilingX = 1f / totalEmotions;
        float tilingY = 1f;
        Vector2 tiling = new Vector2(tilingX, tilingY);
        Vector2 offset = new Vector2(index * tilingX, 0f);

        targetMaterial.SetTextureScale("_EmissionMap", tiling);
        targetMaterial.SetTextureOffset("_EmissionMap", offset);
    }
}