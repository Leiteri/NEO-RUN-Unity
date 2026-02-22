using UnityEngine;

public class CameraFlyController : MonoBehaviour
{

    public float moveDuration = 2f;

    private Vector3 startPos = new Vector3(0f, 2.5f, -6f);
    private Quaternion startRot = Quaternion.Euler(20f, 0f, 0f);

    private Vector3 endPos = new Vector3(0f, 7f, -10f);
    private Quaternion endRot = Quaternion.Euler(28f, 0f, 0f);

    private float timer = 0f;
    private bool isPlaying = false;

    void Start()
    {
        ResetCamera();
    }

    void Update()
    {
        if (!isPlaying) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / moveDuration);

        transform.position = Vector3.Lerp(startPos, endPos, t);
        transform.rotation = Quaternion.Slerp(startRot, endRot, t);

        if (t >= 1f)
            isPlaying = false;
    }

    public void PlayCamera()
    {
        timer = 0f;
        isPlaying = true;
    }

    public void ResetCamera()
    {
        isPlaying = false;
        transform.position = startPos;
        transform.rotation = startRot;
    }
}
