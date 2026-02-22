using UnityEngine;

public class SpeedManager : MonoBehaviour
{
    public static SpeedManager Instance;

    [System.Serializable]
    public class SpeedLevel
    {
        public float distanceRequired;
        public float speed;
    }

    [Header("Speed Levels")]
    public SpeedLevel[] levels;

    private int currentLevel = 0;
    private float distanceTravelled = 0f;
    private bool isRunning = false;
    public int CurrentLevel => currentLevel;

    public float speed => isRunning && RoadGenerator.instance != null ? RoadGenerator.instance.speed : 0f;


    private void Awake()
    {
        Instance = this;
    }

    public void StartRun()
    {
        distanceTravelled = 0f;
        currentLevel = 0;
        isRunning = true;

        if (levels != null && levels.Length > 0)
        {
            RoadGenerator.instance.maxSpeed = levels[0].speed;
            RoadGenerator.instance.speed = levels[0].speed;
        }

        ApplySpeed();
    }

    public void StopRun()
    {
        isRunning = false;
    }

    void Update()
    {
        if (!isRunning) return;

        distanceTravelled += RoadGenerator.instance.speed * Time.deltaTime;

        CheckSpeedLevel();
    }

    void CheckSpeedLevel()
    {
        for (int i = levels.Length - 1; i >= 0; i--)
        {
            if (distanceTravelled >= levels[i].distanceRequired)
            {
                if (currentLevel != i)
                {
                    currentLevel = i;
                    ApplySpeed();
                }
                break;
            }
        }
    }

    public float GetDistance()
    {
        return distanceTravelled;
    }

    void ApplySpeed()
    {
        RoadGenerator.instance.maxSpeed = levels[currentLevel].speed;
        RoadGenerator.instance.speed = levels[currentLevel].speed;

        Debug.Log($"<color=cyan>Уровень повышен!</color> Теперь уровень: {currentLevel}, Скорость: {levels[currentLevel].speed}");
    }
}
