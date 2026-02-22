using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum BuffType { Shield, Multiplier, Magnet, AllLanes }

public class BuffManager : MonoBehaviour
{
    public static BuffManager Instance;

    private Dictionary<BuffType, Coroutine> activeBuffs = new Dictionary<BuffType, Coroutine>();

    [System.Serializable]
    public class BuffSettings
    {
        public BuffType type;
        public float baseDuration = 8f;
        public float durationPerLevel = 5f;
        public int maxLevel = 3;
    }

    [System.Serializable]
    public class BuffVisuals
    {
        public string name;
        public BuffType type;

        public GameObject[] levelBars;

        public UnityEngine.UI.Button buyButton;

        public TextMeshProUGUI priceText;

        public int[] prices = { 100, 300, 500 };
    }

    [Header("Game Settings")]
    public List<BuffSettings> buffSettingsList;

    [Header("Buff Audio")]
    public AudioClip shieldActivationSound;
    public AudioClip magnetActivationSound;
    public AudioClip multiplierActivationSound;
    public AudioClip allLanesActivationSound;
    [Range(0f, 1f)] public float buffVolume = 1f;

    [Header("UI References")]
    public List<BuffVisuals> buffVisualsList;

    [Header("Duration UI Bars")]
    public UnityEngine.UI.Image shieldTimeBar;
    public UnityEngine.UI.Image multiplierTimeBar;
    public UnityEngine.UI.Image magnetTimeBar;

    [Header("Dynamic UI Stack")]
    public GameObject shieldIndicatorObj;
    public GameObject multiplierIndicatorObj;
    public GameObject magnetIndicatorObj;

    private Dictionary<BuffType, UnityEngine.UI.Image> durationBars = new Dictionary<BuffType, UnityEngine.UI.Image>();

    [Header("Active States")]
    public bool hasShield;
    public bool magnetActive;
    public float scoreMultiplier = 1f;
    public bool allLanesActive;

    [Header("Backpack Reference")]
    public BackpackScreen backpack;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateShopUI();

        if (shieldTimeBar != null) durationBars.Add(BuffType.Shield, shieldTimeBar);
        if (multiplierTimeBar != null) durationBars.Add(BuffType.Multiplier, multiplierTimeBar);
        if (magnetTimeBar != null) durationBars.Add(BuffType.Magnet, magnetTimeBar);

        if (shieldIndicatorObj) shieldIndicatorObj.SetActive(false);
        if (multiplierIndicatorObj) multiplierIndicatorObj.SetActive(false);
        if (magnetIndicatorObj) magnetIndicatorObj.SetActive(false);
    }

    public void ActivateBuff(BuffType type, float durationFromItem)
    {
        if (activeBuffs.ContainsKey(type))
        {
            StopCoroutine(activeBuffs[type]);
            activeBuffs.Remove(type);
        }

        float finalDuration = CalculateDuration(type, durationFromItem);
        Coroutine buffCoroutine = StartCoroutine(BuffRoutine(type, finalDuration));
        activeBuffs.Add(type, buffCoroutine);

        if (backpack != null)
        {
            backpack.ShowBuffJoy();
        }
    }

    private IEnumerator BuffRoutine(BuffType type, float duration)
    {
        ApplyBuffEffect(type, true);

        GameObject currentIndicator = GetIndicatorObject(type);

        if (currentIndicator != null)
        {
            currentIndicator.SetActive(true);
            currentIndicator.transform.SetAsLastSibling();
        }

        float remainingTime = duration;
        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            if (durationBars.ContainsKey(type))
            {
                durationBars[type].fillAmount = remainingTime / duration;
            }
            yield return null;
        }

        ApplyBuffEffect(type, false);

        if (currentIndicator != null) currentIndicator.SetActive(false);

        activeBuffs.Remove(type);
    }

    private GameObject GetIndicatorObject(BuffType type)
    {
        switch (type)
        {
            case BuffType.Shield: return shieldIndicatorObj;
            case BuffType.Multiplier: return multiplierIndicatorObj;
            case BuffType.Magnet: return magnetIndicatorObj;
            default: return null;
        }
    }

    private float CalculateDuration(BuffType type, float defaultDuration)
    {
        if (type == BuffType.AllLanes) return 10f;

        BuffSettings settings = buffSettingsList.Find(s => s.type == type);
        if (settings != null)
        {
            int level = GetBuffLevel(type);
            return settings.baseDuration + (level * settings.durationPerLevel);
        }
        return defaultDuration;
    }

    public void ResetAllBuffs()
    {
        StopAllCoroutines();
        activeBuffs.Clear();

        if (shieldIndicatorObj) shieldIndicatorObj.SetActive(false);
        if (multiplierIndicatorObj) multiplierIndicatorObj.SetActive(false);
        if (magnetIndicatorObj) magnetIndicatorObj.SetActive(false);

        ApplyBuffEffect(BuffType.Shield, false);
        ApplyBuffEffect(BuffType.Magnet, false);
        ApplyBuffEffect(BuffType.Multiplier, false);
        ApplyBuffEffect(BuffType.AllLanes, false);
    }

    public void ResetShield()
    {
        if (activeBuffs.ContainsKey(BuffType.Shield))
        {
            StopCoroutine(activeBuffs[BuffType.Shield]);
            activeBuffs.Remove(BuffType.Shield);
        }

        hasShield = false;

        ApplyBuffEffect(BuffType.Shield, false);

        if (shieldIndicatorObj != null)
        {
            shieldIndicatorObj.SetActive(false);

            LayoutRebuilder.ForceRebuildLayoutImmediate(shieldIndicatorObj.transform.parent as RectTransform);
        }
    }


    public void UpgradeBuff(string typeName)
    {
        if (System.Enum.TryParse(typeName, out BuffType type))
        {
            BuffVisuals visuals = buffVisualsList.Find(v => v.type == type);
            int currentLevel = GetBuffLevel(type);

            if (currentLevel >= 3) return;

            int cost = 0;
            if (visuals != null && currentLevel < visuals.prices.Length)
            {
                cost = visuals.prices[currentLevel];
            }

            if (UIManager.Instance != null)
            {
                if (UIManager.Instance.TrySpendCoins(cost))
                {
                    PlayerPrefs.SetInt("BuffLevel_" + type.ToString(), currentLevel + 1);
                    PlayerPrefs.Save();

                    UpdateShopUI();
                    Debug.Log($"Куплен апгрейд {type}. Списано {cost} монет.");
                }
                else
                {
                    Debug.Log("Недостаточно монет для покупки!");
                }
            }
        }
    }

    public void UpdateShopUI()
    {
        foreach (var visual in buffVisualsList)
        {
            int currentLevel = GetBuffLevel(visual.type);

            if (visual.levelBars != null)
            {
                for (int i = 0; i < visual.levelBars.Length; i++)
                {
                    visual.levelBars[i].SetActive(i < currentLevel);
                }
            }

            if (visual.buyButton != null)
            {
                if (currentLevel >= 3)
                {
                    visual.buyButton.interactable = false;
                    if (visual.priceText != null) visual.priceText.text = "MAX";
                }
                else
                {
                    visual.buyButton.interactable = true;
                    if (visual.priceText != null && currentLevel < visual.prices.Length)
                    {
                        visual.priceText.text = visual.prices[currentLevel].ToString();
                    }
                }
            }
        }
    }

    public int GetBuffLevel(BuffType type)
    {
        return PlayerPrefs.GetInt("BuffLevel_" + type.ToString(), 0);
    }

    private void ApplyBuffEffect(BuffType type, bool active)
    {
        AudioClip clipToPlay = null;
        PlayerController player = FindFirstObjectByType<PlayerController>();

        switch (type)
        {
            case BuffType.Shield:
                hasShield = active;
                clipToPlay = shieldActivationSound;

                if (player != null)
                {
                    if (active)
                        player.ActivateShieldVisual();
                    else
                        if (player.shieldVisual != null) player.shieldVisual.SetActive(false);
                }
                break;

            case BuffType.Magnet:
                magnetActive = active;
                clipToPlay = magnetActivationSound;
                break;

            case BuffType.Multiplier:
                scoreMultiplier = active ? 2f : 1f;
                clipToPlay = multiplierActivationSound;
                break;

            case BuffType.AllLanes:
                allLanesActive = active;
                clipToPlay = allLanesActivationSound;
                break;
        }

        if (active && clipToPlay != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(clipToPlay, buffVolume);
        }
    }

    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        UpdateShopUI();
        Debug.Log("Весь прогресс сброшен!");
    }
}