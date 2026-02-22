using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;
using System;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance;

    [Header("Ad IDs")]
    private string interstitialId = "R-M-18763851-2";
    private string rewardedId = "R-M-18763851-1";
    //private string interstitialId = "demo-interstitial-yandex";
    //private string rewardedId = "demo-rewarded-yandex";

    private Interstitial interstitialAd;
    private RewardedAd rewardedAd;

    private bool isInterstitialLoaded = false;
    private bool isRewardedLoaded = false;
    private bool isShowingAd = false;

    private int deathCount = 0;
    private const int deathsBeforeInterstitial = 5;
    private bool pendingInterstitial = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadInterstitial();
        LoadRewarded();
    }

    public void LoadInterstitial()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        isInterstitialLoaded = false;

        InterstitialAdLoader loader = new InterstitialAdLoader();

        loader.OnAdLoaded += (sender, args) =>
        {
            interstitialAd = args.Interstitial;
            isInterstitialLoaded = true;

            Debug.Log("Interstitial loaded");

            if (pendingInterstitial && !isShowingAd)
            {
                TryShowInterstitial();
            }

            interstitialAd.OnAdDismissed += (s, e) =>
            {
                Debug.Log("Interstitial dismissed");

                isShowingAd = false;
                isInterstitialLoaded = false;

                LoadInterstitial();
            };
        };

        loader.OnAdFailedToLoad += (sender, args) =>
        {
            isInterstitialLoaded = false;
            Debug.LogError("Interstitial failed: " + args.Message);
        };

        var config = new AdRequestConfiguration.Builder(interstitialId).Build();
        loader.LoadAd(config);
    }

    public bool IsRewardedReady()
    {
        return rewardedAd != null && isRewardedLoaded && !isShowingAd;
    }

    public void LoadRewarded()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        isRewardedLoaded = false;

        RewardedAdLoader loader = new RewardedAdLoader();

        loader.OnAdLoaded += (sender, args) =>
        {
            rewardedAd = args.RewardedAd;
            isRewardedLoaded = true;

            Debug.Log("[ADS][REWARDED] Loaded successfully");

            rewardedAd.OnRewarded += (s, reward) =>
            {
                Debug.Log("[ADS][REWARDED] Reward event fired");

                deathCount = 0;

                if (PlayerController.Instance != null)
                {
                    PlayerController.Instance.RevivePlayer();
                }
            };

            rewardedAd.OnAdDismissed += (s, e) =>
            {
                Debug.Log("[ADS][REWARDED] Dismissed");

                isShowingAd = false;
                isRewardedLoaded = false;

                LoadRewarded();
            };
        };

        loader.OnAdFailedToLoad += (sender, args) =>
        {
            isRewardedLoaded = false;
            Debug.LogError("[ADS][REWARDED] Failed to load: " + args.Message);
        };

        var config = new AdRequestConfiguration.Builder(rewardedId).Build();
        loader.LoadAd(config);
    }

    public void ShowReviveAd()
    {

        if (rewardedAd != null && isRewardedLoaded && !isShowingAd)
        {
            isShowingAd = true;
            rewardedAd.Show();
        }
        else
        {
            Debug.LogWarning("Rewarded not ready.");
        }
    }

    public void HandlePlayerDeath()
    {
        deathCount++;

        Debug.Log($"Deaths: {deathCount}/{deathsBeforeInterstitial}");

        if (deathCount >= deathsBeforeInterstitial)
        {
            TryShowInterstitial();
        }
    }
    private void TryShowInterstitial()
    {
        if (interstitialAd != null && isInterstitialLoaded && !isShowingAd)
        {
            Debug.Log("Showing interstitial");

            isShowingAd = true;
            pendingInterstitial = false;
            deathCount = 0;

            interstitialAd.Show();
        }
        else
        {
            Debug.Log("Interstitial not ready. Will show when loaded.");
            pendingInterstitial = true;
        }
    }
}