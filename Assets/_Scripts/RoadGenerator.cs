using UnityEngine;
using System.Collections.Generic;

public class RoadGenerator : MonoBehaviour
{
    public static RoadGenerator instance;

    public GameObject RoadPrefab;
    private List<GameObject> roads = new List<GameObject>();
    public float maxSpeed = 10;
    public float speed = 0;
    public int maxRoadCount = 5;


    [Header("UI Panels")]
    public GameObject pauseOverlay; // Перетащи сюда свой PauseOverlay

    [Header("Character")]
    public Animator characterAnimator;

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        ResetLevel();
        //StartLevel();
    }

    // Update is called once per frame
    void Update()
    {
        // Если целевая скорость 0, то и текущую ставим в 0 и выходим
        // Это предотвратит движение до вызова StartLevel()
        if (maxSpeed <= 0)
        {
            speed = 0;
            return;
        }

        // Плавно разгоняемся только если игра запущена
        // (maxSpeed теперь управляется через SpeedManager)
        speed = Mathf.MoveTowards(speed, maxSpeed, Time.deltaTime * 2f);

        if (speed <= 0) return;

        // Движение дороги
        //foreach (GameObject road in roads)
       // {
        //    road.transform.position -= new Vector3(0, 0, speed * Time.deltaTime);
       // }

        // Логика удаления и спавна дороги (как была раньше)
        if (roads.Count > 0 && roads[0].transform.position.z < -20)
        {
            PoolManager.instance.Despawn(roads[0]);
            roads.RemoveAt(0);
            CreateNextRoad();
        }
    }

    private void CreateNextRoad()
    {
        Vector3 pos = Vector3.zero;
        if (roads.Count > 0) { pos = roads[roads.Count - 1].transform.position + new Vector3(0, 0, 20);  }
        GameObject go = PoolManager.instance.Spawn(RoadPrefab, pos, Quaternion.identity);
        go.transform.SetParent(transform);
        roads.Add(go);

    }

    public void StartLevel()
    {
        SwipeManager.instance.enabled = true;
        SpeedManager.Instance.StartRun();
    }


    public void ResetLevel()
    {
        maxSpeed = 0;
        speed = 0;

        while (roads.Count > 0)
        {
            PoolManager.instance.Despawn(roads[0]);
            roads.RemoveAt(0);
        }

        for (int i = 0; i < maxRoadCount; i++)
        {
            CreateNextRoad();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ResetSessionData();
        }

        if (SwipeManager.instance != null) SwipeManager.instance.enabled = false;
        MapGenerator.Instance.ResetMaps();

    }

    public void StopLevel()
    {
        maxSpeed = 0;
        speed = 0;

        if (SwipeManager.instance != null)
            SwipeManager.instance.enabled = false;

        SpeedManager.Instance.StopRun();
    }

    public void PressPause()
    {
        Time.timeScale = 0f;
        if (characterAnimator != null) characterAnimator.speed = 0f;

        pauseOverlay.SetActive(true);
    }

    public void PressResume()
    {
        Time.timeScale = 1f;
        if (characterAnimator != null) characterAnimator.speed = 1f;

        pauseOverlay.SetActive(false);
    }

}
