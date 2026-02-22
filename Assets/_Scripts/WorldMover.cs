using UnityEngine;

public class WorldMover : MonoBehaviour
{
    void Update()
    {
        if (RoadGenerator.instance == null) return;

        float speed = RoadGenerator.instance.speed;

        if (speed <= 0) return;

        transform.position -= Vector3.forward * speed * Time.deltaTime;
    }
}
