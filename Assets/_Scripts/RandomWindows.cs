using UnityEngine;

public class RandomWindows : MonoBehaviour
{
    public Renderer[] windows;
    [Range(0f, 1f)]
    public float chance = 0.5f;

    public Color emissionColor = new Color(1f, 0.8f, 0.4f);
    public float emissionIntensity = 4f;

    void Start()
    {
        foreach (var window in windows)
        {
            bool isOn = Random.value < chance;

            var block = new MaterialPropertyBlock();
            window.GetPropertyBlock(block);

            if (isOn)
                block.SetColor("_EmissionColor", emissionColor * emissionIntensity);
            else
                block.SetColor("_EmissionColor", Color.black);

            window.SetPropertyBlock(block);
        }
    }
}