using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;

    private const string LeaderboardId = "Global_Highscore";

    async void Awake()
    {
        Instance = this;

        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Успешный вход в UGS!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка инициализации сервисов: " + e.Message);
        }

        string savedName = PlayerPrefs.GetString("PlayerName", "");
        if (string.IsNullOrEmpty(savedName))
        {
            string randomName = "Runner#" + Random.Range(1000, 9999);
            UpdatePlayerName(randomName);
            PlayerPrefs.SetString("PlayerName", randomName);
        }
    }

    public async void UpdatePlayerName(string newName)
    {
        try
        {
            await Unity.Services.Authentication.AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
            Debug.Log("Имя обновлено на: " + newName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка смены имени: " + e.Message);
        }
    }
    public async void SubmitScore(int score)
    {
        try
        {
            await LeaderboardsService.Instance.AddPlayerScoreAsync(LeaderboardId, score);
            Debug.Log($"Очки {score} отправлены в таблицу лидеров!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка при отправке очков: " + e.Message);
        }
    }
}