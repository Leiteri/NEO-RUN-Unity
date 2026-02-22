using UnityEngine;

public class CoinController : MonoBehaviour
{
    public Collider col;
    public Renderer rend;
    [HideInInspector] public float baseY;

    private Transform playerTransform;
    private float moveSpeed = 20f;
    private float initialMoveSpeed = 20f;
    public bool isBeingMagnetized = false;

    [Header("Audio")]
    public AudioClip coinSound;
    [Range(0f, 1f)] public float coinVolume = 0.5f;

    private Vector3 initialLocalPosition;

    void Awake()
    {
        if (col == null) col = GetComponent<Collider>();
        if (rend == null) rend = GetComponent<Renderer>();

        initialLocalPosition = transform.localPosition;
        baseY = transform.position.y;
    }

    void OnEnable()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        CoinRotationManager.Register(this);

        transform.localPosition = initialLocalPosition;

        ResetCoin();
    }

    void Update()
    {
        if (!isBeingMagnetized)
        {
            if (BuffManager.Instance != null && BuffManager.Instance.magnetActive)
            {
                if (playerTransform == null)
                {
                    GameObject p = GameObject.FindGameObjectWithTag("Player");
                    if (p != null) playerTransform = p.transform;
                }

                if (playerTransform != null)
                {
                    float distance = Vector3.Distance(transform.position, playerTransform.position);
                    if (distance < 8f)
                    {
                        isBeingMagnetized = true;
                    }
                }
            }
        }
        else
        {
            if (playerTransform != null)
            {
                moveSpeed += Time.deltaTime * 50f;
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position + Vector3.up, moveSpeed * Time.deltaTime);
            }
        }
    }

    void OnDisable()
    {
        CoinRotationManager.Unregister(this);
        isBeingMagnetized = false;
    }

    public void ResetCoin()
    {
        if (col != null) col.enabled = true;
        if (rend != null) rend.enabled = true;

        isBeingMagnetized = false;
        moveSpeed = initialMoveSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (UIManager.Instance != null)
                UIManager.Instance.AddCoins(1);

            if (SoundManager.Instance != null && coinSound != null)
            {
                SoundManager.Instance.PlayCoinSound(coinSound, coinVolume);
            }

            gameObject.SetActive(false);
        }
    }
}