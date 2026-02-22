using UnityEngine;
using System.Collections;

public class BackpackScreen : MonoBehaviour
{
    private Renderer screenRenderer;
    private MaterialPropertyBlock propBlock;

    [Header("Settings")]
    public int materialIndex = 0;
    public PlayerController player; 

    void Awake()
    {
        screenRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        ShowSmile();
        StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 6f));

            if (player != null && !IsPlayerDead())
            {
                ShowBlink();
                yield return new WaitForSeconds(0.22f);

                if (!IsPlayerDead())
                {
                    ShowSmile();
                }
            }
        }
    }

    private bool IsPlayerDead()
    {
        return player.GetCurrentState() == PlayerController.PlayerState.Dead;
    }

    public void SetEmotion(float offsetX, float offsetY)
    {
        screenRenderer.GetPropertyBlock(propBlock, materialIndex);
        propBlock.SetVector("_BaseMap_ST", new Vector4(1f, 0.25f, offsetX, offsetY));
        screenRenderer.SetPropertyBlock(propBlock, materialIndex);
    }

    public void ShowBuffJoy()
    {
        StartCoroutine(JoyRoutine());
    }

    private IEnumerator JoyRoutine()
    {

        screenRenderer.GetPropertyBlock(propBlock, materialIndex);
        propBlock.SetVector("_BaseMap_ST", new Vector4(1f, 0.25f, 0f, 0f));
        screenRenderer.SetPropertyBlock(propBlock, materialIndex);

        yield return new WaitForSeconds(1f);

        if (!IsPlayerDead())
        {
            ShowSmile();
        }
    }

    public void ShowSmile() => SetEmotion(0f, 0.75f);
    public void ShowBlink() => SetEmotion(0f, 0.5f);
    public void ShowSad() => SetEmotion(0f, 0.25f);
}