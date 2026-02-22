using System.Collections.Generic;
using UnityEngine;

public class CoinRotationManager : MonoBehaviour
{
    private static List<CoinController> activeCoins = new List<CoinController>();

    [Header("Rotation")]
    public float rotationSpeed = 150f;

    [Header("Perfect Wave Settings")]
    public float bounceAmplitude = 0.5f;
    public float bounceSpeed = 2f;
    [Tooltip("Чем выше число, тем длиннее волна между монетами")]
    public float waveLength = 2.0f;

    public static void Register(CoinController coin) => activeCoins.Add(coin);
    public static void Unregister(CoinController coin) => activeCoins.Remove(coin);

    void Update()
    {
        float rotStep = -rotationSpeed * Time.deltaTime;
        float time = Time.time;

        for (int i = activeCoins.Count - 1; i >= 0; i--)
        {
            CoinController coin = activeCoins[i];
            if (coin == null) { activeCoins.RemoveAt(i); continue; }

            if (coin.gameObject.activeInHierarchy)
            {
                coin.transform.Rotate(0, -rotStep, 0, Space.Self);

                if (!coin.isBeingMagnetized)
                {
                    float wavePhase = (coin.transform.position.z / waveLength) + (time * bounceSpeed);
                    float newY = coin.baseY + Mathf.Sin(wavePhase) * bounceAmplitude;

                    Vector3 pos = coin.transform.position;
                    pos.y = newY;
                    coin.transform.position = pos;
                }
            }
        }
    }

    void OnDestroy() => activeCoins.Clear();
}