using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public Image timerCircle;

    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject gamePanel;

    [Header("Revive Settings")]
    public GameObject reviveButton;
    private bool _hasRevivedThisRun = false;

    [Header("Menu Animation Settings")]
    public RectTransform menuBottomPart;
    public GameObject menuTopPart;
    public float slideDuration = 0.5f;
    public float slideDistance = 1000f;
    private Vector2 _bottomPartStartPos;
    private Coroutine _menuAnimationCoroutine;

    [Header("Audio Clips")]
    public AudioClip musicIntro;
    public AudioClip musicLoop;

    [Header("Level System")]
    public TextMeshProUGUI levelText;
    public Image levelProgressBar;

    public float firstLevelCost = 3000f;
    public float levelStepX = 300f;

    private int _currentLevel;
    private float _expToNextLevel;
    private float _currentExpProgress;

    public GameObject deathPanel;

    [Header("Score Settings")]
    public TextMeshProUGUI scoreText;
    public string scoreFormat = "D6";
    public float baseScoreMultiplier = 10f;
    public float counterAnimationSpeed = 8f;

    [Header("Coin UI Elements")]
    public TextMeshProUGUI totalCoinText;
    public TextMeshProUGUI sessionCoinText;

    [Header("Pause Settings")]
    public GameObject pausePanel;
    private bool _isPaused = false;

    [Header("Settings UI")]
    public Toggle musicToggle;
    public Toggle sfxToggle;

    [Header("Level Audio")]
    public AudioClip levelUpSound;
    [Range(0f, 1f)] public float levelUpVolume = 0.8f;

    private int _totalCoins;
    private int _sessionCoins;

    private float _totalScore = 0f;
    private float _displayedScore = 0f;
    private bool _isGameActive = false;
    private Vector3 _originalLevelTextScale;

    private float _highScore;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (menuBottomPart != null)
            _bottomPartStartPos = menuBottomPart.anchoredPosition;

        _currentLevel = PlayerPrefs.GetInt("UserLevel", 1);
        _currentExpProgress = PlayerPrefs.GetFloat("CurrentExpProgress", 0f);
        _totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        CalculateNextLevelCost();
        UpdateLevelUI();
        UpdateCoinUI();

        if (levelProgressBar != null)
            levelProgressBar.fillAmount = _currentExpProgress / _expToNextLevel;

        menuPanel.SetActive(true);
        gamePanel.SetActive(false);

        if (levelText != null)
            _originalLevelTextScale = levelText.transform.localScale;

        _highScore = PlayerPrefs.GetFloat("HighScore", 0f);
    }

    void Update()
    {
        if (SpeedManager.Instance == null || !_isGameActive) return;

        float currentSpeed = SpeedManager.Instance.speed;
        float activeBuffMult = (BuffManager.Instance != null) ? BuffManager.Instance.scoreMultiplier : 1f;
        float frameScore = currentSpeed * Time.deltaTime * baseScoreMultiplier * activeBuffMult;

        _totalScore += frameScore;

        if (_displayedScore < _totalScore)
        {
            _displayedScore = Mathf.Lerp(_displayedScore, _totalScore, Time.deltaTime * counterAnimationSpeed);
            if (_totalScore - _displayedScore < 1f) _displayedScore = _totalScore;
        }

        if (scoreText != null)
            scoreText.text = Mathf.FloorToInt(_displayedScore).ToString(scoreFormat);
    }

    public void StartGame()
    {
        _isGameActive = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SwitchFromAmbientToMusic(musicIntro, musicLoop);
        }

        if (_menuAnimationCoroutine != null) StopCoroutine(_menuAnimationCoroutine);
        _menuAnimationCoroutine = StartCoroutine(AnimateMenu(false));

        gamePanel.SetActive(true);
        ResetSessionData();
    }

    public void GameOver()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.StopMusicWithFade();

        _isGameActive = false;

        menuPanel.SetActive(true);
        if (_menuAnimationCoroutine != null) StopCoroutine(_menuAnimationCoroutine);
        _menuAnimationCoroutine = StartCoroutine(AnimateMenu(true));

        gamePanel.SetActive(false);
        AddExperienceAfterRun();
        ResetSessionData();
    }
    public void OnRestartButtonPressed()
    {
        StopCoroutine("AutoCloseDeathPanel");
        deathPanel.SetActive(false);
        gamePanel.SetActive(false);

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.HandlePlayerDeath();
        }

        menuPanel.SetActive(true);
        if (_menuAnimationCoroutine != null) StopCoroutine(_menuAnimationCoroutine);
        _menuAnimationCoroutine = StartCoroutine(AnimateMenu(true));

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StartAmbientWithFade();
        }

        AddExperienceAfterRun();

        PlayerController player = Object.FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.ResetPlayer();
        }

        if (RoadGenerator.instance != null)
        {
            RoadGenerator.instance.ResetLevel();
        }

        ResetSessionData();
    }

    private IEnumerator AnimateMenu(bool show)
    {
        if (menuBottomPart == null) yield break;

        Vector2 startPos = menuBottomPart.anchoredPosition;
        Vector2 targetPos = show ? _bottomPartStartPos : _bottomPartStartPos + Vector2.down * slideDistance;

        if (!show && menuTopPart != null) menuTopPart.SetActive(false);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / slideDuration;
            float smoothT = Mathf.SmoothStep(0, 1, t);
            menuBottomPart.anchoredPosition = Vector2.Lerp(startPos, targetPos, smoothT);
            yield return null;
        }

        menuBottomPart.anchoredPosition = targetPos;

        if (show && menuTopPart != null) menuTopPart.SetActive(true);

        if (!show) menuPanel.SetActive(false);
    }


    public void ResetSessionData()
    {
        _sessionCoins = 0;
        _totalScore = 0;
        _hasRevivedThisRun = false;
        _displayedScore = 0;
        UpdateCoinUI();
    }

    public void AddCoins(int amount)
    {
        _sessionCoins += amount;
        _totalCoins += amount;
        PlayerPrefs.SetInt("TotalCoins", _totalCoins);
        PlayerPrefs.Save();
        UpdateCoinUI();
    }

    private void UpdateCoinUI()
    {
        if (totalCoinText != null) totalCoinText.text = _totalCoins.ToString();
        if (sessionCoinText != null) sessionCoinText.text = _sessionCoins.ToString();
    }

    private void CalculateNextLevelCost()
    {
        _expToNextLevel = firstLevelCost + (_currentLevel - 1) * levelStepX;
    }

    public void AddExperienceAfterRun()
    {
        float scoreToProcess = _totalScore;

        if (scoreToProcess > _highScore)
        {
            _highScore = scoreToProcess;
            PlayerPrefs.SetFloat("HighScore", _highScore);
            PlayerPrefs.Save();

            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.SubmitScore(Mathf.FloorToInt(_highScore));
            }
        }

        StartCoroutine(AnimateExpGain(scoreToProcess));
        _totalScore = 0;
        _displayedScore = 0;
    }

    private IEnumerator AnimateExpGain(float gainedScore)
    {
        yield return new WaitForSeconds(0.2f);
        while (gainedScore > 0)
        {
            float spaceInCurrentLevel = _expToNextLevel - _currentExpProgress;
            float amountToAdd = Mathf.Min(gainedScore, spaceInCurrentLevel);
            float startFill = _currentExpProgress / _expToNextLevel;
            float endFill = (_currentExpProgress + amountToAdd) / _expToNextLevel;

            float t = 0;
            float duration = 0.6f;
            while (t < 1)
            {
                t += Time.deltaTime / duration;
                levelProgressBar.fillAmount = Mathf.Lerp(startFill, endFill, t);
                yield return null;
            }

            gainedScore -= amountToAdd;
            _currentExpProgress += amountToAdd;

            if (_currentExpProgress >= _expToNextLevel)
            {
                _currentExpProgress = 0;
                _currentLevel++;

                if (SoundManager.Instance != null && levelUpSound != null)
                {
                    SoundManager.Instance.PlaySFX(levelUpSound, levelUpVolume);
                }
                CalculateNextLevelCost();
                UpdateLevelUI();
                StartCoroutine(PulseLevelText());
                levelProgressBar.fillAmount = 0;
                yield return new WaitForSeconds(0.15f);
            }
        }
        PlayerPrefs.SetInt("UserLevel", _currentLevel);
        PlayerPrefs.SetFloat("CurrentExpProgress", _currentExpProgress);
        PlayerPrefs.Save();
    }

    private IEnumerator PulseLevelText()
    {
        if (levelText == null) yield break;
        Vector3 punchScale = _originalLevelTextScale * 1.5f;
        Color originalColor = levelText.color;
        Color punchColor = Color.white;
        levelText.transform.localScale = punchScale;
        levelText.color = punchColor;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 3f;
            levelText.transform.localScale = Vector3.Lerp(punchScale, _originalLevelTextScale, t);
            levelText.color = Color.Lerp(punchColor, originalColor, t);
            yield return null;
        }
        levelText.transform.localScale = _originalLevelTextScale;
        levelText.color = originalColor;
    }

    private void UpdateLevelUI()
    {
        if (levelText != null)
            levelText.text = "LVL " + _currentLevel;
    }

    public bool TrySpendCoins(int amount)
    {
        if (_totalCoins >= amount)
        {
            _totalCoins -= amount;
            PlayerPrefs.SetInt("TotalCoins", _totalCoins);
            PlayerPrefs.Save();
            UpdateCoinUI();
            return true;
        }
        return false;
    }

    public void ShowDeathMenu()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopMusicWithFade();
            SoundManager.Instance.StartAmbientQuietly(0.2f);
        }

        if (reviveButton != null)
        {
            if (reviveButton != null)
            {
                bool canRevive =
                    !_hasRevivedThisRun &&
                    AdsManager.Instance != null &&
                    AdsManager.Instance.IsRewardedReady();

                reviveButton.SetActive(canRevive);
            }
        }

        deathPanel.SetActive(true);
        StopCoroutine("AutoCloseDeathPanel");
        StartCoroutine(AutoCloseDeathPanel(3f));
    }

    private IEnumerator AutoCloseDeathPanel(float delay)
    {
        float timer = delay;
        if (timerCircle != null) timerCircle.fillAmount = 1f;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timerCircle != null) timerCircle.fillAmount = timer / delay;
            yield return null;
        }
        if (deathPanel.activeSelf) OnRestartButtonPressed();
    }
    public void TogglePause()
    {
        _isPaused = !_isPaused;

        if (_isPaused)
        {
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);

            if (SoundManager.Instance != null)
                SoundManager.Instance.PauseGameSound();
        }
        else
        {
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);

            if (SoundManager.Instance != null)
                SoundManager.Instance.ResumeGameSound();
        }
    }
    public void UpdateSettingsUI()
    {
        if (musicToggle != null)
            musicToggle.isOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;

        if (sfxToggle != null)
            sfxToggle.isOn = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;
    }

    public void OnReviveButtonPressed()
    {
        if (!CanShowRevive()) return;

        _hasRevivedThisRun = true;
        _isGameActive = true;

        StopCoroutine("AutoCloseDeathPanel");
        deathPanel.SetActive(false);
        gamePanel.SetActive(true);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ResumeMusicAfterRevive(musicIntro, musicLoop);
            SoundManager.Instance.SwitchFromAmbientToMusic(musicIntro, musicLoop);
        }

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowReviveAd();
        }

        PlayerController player = Object.FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.RevivePlayer();
        }

        if (RoadGenerator.instance != null)
        {
            RoadGenerator.instance.StartLevel();
        }

    }
    private bool CanShowRevive()
    {
        return !_hasRevivedThisRun && AdsManager.Instance != null &&
               AdsManager.Instance.IsRewardedReady();
    }
}