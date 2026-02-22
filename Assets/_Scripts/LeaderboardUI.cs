using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Leaderboards;
using TMPro;
using Unity.Services.Authentication;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject rowPrefab;
    public Transform contentParent;

    public async void OpenLeaderboard()
    {
        this.gameObject.SetActive(true);

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        try
        {
            var options = new GetScoresOptions { Limit = 100 };
            var scores = await LeaderboardsService.Instance.GetScoresAsync("Global_Highscore", options);

            foreach (var entry in scores.Results)
            {
                GameObject newRow = Instantiate(rowPrefab, contentParent);
                var texts = newRow.GetComponentsInChildren<TextMeshProUGUI>();

                string currentPlayerId = AuthenticationService.Instance.PlayerId;

                string playerName = string.IsNullOrEmpty(entry.PlayerName) ? $"Runner_{entry.PlayerId.Substring(0, 4)}" : entry.PlayerName.Split('#')[0];

                texts[0].text = $"{entry.Rank + 1}. {playerName}";
                texts[1].text = Mathf.FloorToInt((float)entry.Score).ToString();

                if (entry.PlayerId == currentPlayerId)
                {
                    foreach (var t in texts)
                    {
                        t.color = Color.yellow;
                        t.fontStyle = FontStyles.Bold;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Не удалось загрузить таблицу: " + e.Message);
        }
        var playerEntry = await LeaderboardsService.Instance.GetPlayerScoreAsync("Global_Highscore");

        if (playerEntry != null)
        {
            Debug.Log($"Твое место: {playerEntry.Rank + 1}");
        }
    }

    public void CloseLeaderboard()
    {
        this.gameObject.SetActive(false);
    }
}