using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public enum PlayerState
    {
        WaitingStart,
        Turning,
        Running,
        Jumping,
        Sliding,
        Dead
    }


    [Header("Lane")]
    public float laneOffset = 2f;
    public float laneMoveSpeed = 18f;

    [Header("References")]
    public CameraFlyController cameraController;
    public BackpackScreen backpackScreen;

    [Header("Shield Settings")]
    public GameObject shieldVisual;
    private bool isInvulnerable = false;


    [Header("Jump")]
    public float jumpForce = 7f;
    public float extraFallGravity = -30f;
    public float coyoteTime = 0.15f;
    public float jumpBuffer = 0.15f;

    [Header("Visual Juice")]
    public float leanAmount = 15f;

    [Header("Slide")]
    public float slideDuration = 0.7f;

    [Header("Turn")]
    public float turnAngle = 180f;
    public float turnDuration = 0.5f;
    public float delayBeforeRun = 0.2f;

    [Header("Ground")]
    public Transform groundCheck;
    public float groundRadius = 0.3f;
    public LayerMask groundLayer;

    [Header("Audio")]
    public AudioClip deathSound;
    [Range(0f, 1f)] public float deathVolume = 0.8f;
    public AudioClip shieldBreakSound;

    [Header("Movement Audio")]
    public AudioClip jumpSound;
    public AudioClip slideSound;

    [Header("Footsteps Audio")]
    public AudioClip groundStepSound;
    public AudioClip roofStepSound;
    [Range(0f, 1f)] public float footstepVolume = 0.3f;

    [Header("Revive Effects")]
    public Renderer[] characterParts;

    private Rigidbody rb;
    private Animator animator;
    private CapsuleCollider col;

    private PlayerState state;

    private int currentLane;
    private int previousLane;
    private float targetLaneX;

    private bool isGrounded;
    private float lastGroundTime;
    private float lastJumpPressed;


    private float normalHeight;
    private Vector3 normalCenter;
    private Vector3 startPos;
    private Quaternion startRot;
    private bool wasGrounded;
    private Coroutine shieldBreakCoroutine;


    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();

        normalHeight = col.height;
        normalCenter = col.center;

        startPos = transform.position;
        startRot = transform.rotation;

        lastJumpPressed = -999f;
        lastGroundTime = -999f;
    }
    private void Start()
    {
        StartCoroutine(RandomIdleRoutine());
    }
    void OnSwipe(Vector2 direction)
    {
        if (direction == Vector2.left && state == PlayerState.Running)
        {
            ChangeLane(-1);
        }
        else if (direction == Vector2.right && state == PlayerState.Running)
        {
            ChangeLane(1);
        }
        else if (direction == Vector2.up)
        {
            lastJumpPressed = Time.time;
        }
        else if (direction == Vector2.down && state == PlayerState.Running)
        {
            StartCoroutine(SlideRoutine());
        }
    }

    private void OnEnable()
    {
        if (SwipeManager.instance != null)
            SwipeManager.instance.OnSwipe += OnSwipe;
    }

    private void OnDisable()
    {
        if (SwipeManager.instance != null)
            SwipeManager.instance.OnSwipe -= OnSwipe;
    }

    void FixedUpdate()
    {
        GroundCheckLogic();

        if (isGrounded && !wasGrounded)
        {
            PlayLandingLogic();
        }
        wasGrounded = isGrounded;

        animator.SetBool("IsGrounded", isGrounded);

        if (state == PlayerState.Running || state == PlayerState.Jumping || state == PlayerState.Sliding)
        {
            Vector3 pos = rb.position;

            float newX = Mathf.MoveTowards(
                pos.x,
                targetLaneX,
                laneMoveSpeed * Time.fixedDeltaTime
            );

            rb.MovePosition(new Vector3(newX, pos.y, startPos.z));

            float xDifference = targetLaneX - pos.x;

            float targetRoll = xDifference * leanAmount;

            Quaternion targetRotation = Quaternion.Euler(0, 0, -targetRoll);

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }

        JumpLogic();
        ApplyExtraGravity();

        if (isGrounded && state == PlayerState.Jumping)
            state = PlayerState.Running;
    }

    void PlayLandingLogic()
    {

        if (state == PlayerState.WaitingStart || state == PlayerState.Dead)
        {
            return;
        }

        AudioClip clipToPlay = groundStepSound;
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;

        bool isHit = Physics.Raycast(rayStart, Vector3.down, out hit, 2.0f);

        if (isHit)
        {
            if (hit.collider.CompareTag("Roof"))
            {
                clipToPlay = roofStepSound;
            }
        }

        if (SoundManager.Instance != null && clipToPlay != null)
        {
            SoundManager.Instance.PlayVariableSFX(clipToPlay, footstepVolume * 1.5f);
        }

    }

    public PlayerState GetCurrentState()
    {
        return state;
    }

    void GroundCheckLogic()
    {
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundRadius,
            groundLayer
        );

        if (isGrounded)
            lastGroundTime = Time.time;
    }

    void ApplyExtraGravity()
    {
        if (!isGrounded && rb.linearVelocity.y < 0)
        {
            rb.AddForce(Vector3.up * extraFallGravity,
                ForceMode.Acceleration);
        }
    }

    void JumpLogic()
    {
        bool canJump =
            Time.time - lastGroundTime <= coyoteTime &&
            Time.time - lastJumpPressed <= jumpBuffer;

        if (canJump && state == PlayerState.Running)
        {
            if (SoundManager.Instance != null && jumpSound != null)
            {
                SoundManager.Instance.PlaySFX(jumpSound, 0.5f);
            }
            state = PlayerState.Jumping;

            animator.SetTrigger("Jump");

            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                0,
                rb.linearVelocity.z);

            rb.AddForce(Vector3.up * jumpForce,
                ForceMode.Impulse);

            lastJumpPressed = -999f;
        }
    }

    void ChangeLane(int dir)
    {
        previousLane = currentLane;
        currentLane = Mathf.Clamp(currentLane + dir, -1, 1);
        targetLaneX = currentLane * laneOffset;
    }

    IEnumerator SlideRoutine()
    {
        state = PlayerState.Sliding;

        animator.SetTrigger("Slide");

        yield return new WaitForSeconds(0.15f);

        if (SoundManager.Instance != null && slideSound != null)
        {
            SoundManager.Instance.PlaySFX(slideSound, 0.35f);
        }

        col.height = normalHeight * 0.5f;
        col.center = new Vector3(
            normalCenter.x,
            normalCenter.y * 0.1f,
            normalCenter.z);

        yield return new WaitForSeconds(slideDuration - 0.15f);

        col.height = normalHeight;
        col.center = normalCenter;

        if (state != PlayerState.Dead)
            state = PlayerState.Running;
    }

    public void StartGame()
    {
        if (state != PlayerState.WaitingStart) return;
        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        state = PlayerState.Turning;

        animator.SetTrigger("Turn180");

        Quaternion startRot = transform.rotation;

        Quaternion targetRot = startRot * Quaternion.Euler(0, turnAngle, 0);

        float t = 0f;
        while (t < turnDuration)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t / turnDuration);
            yield return null;
        }

        // Жестко фиксируем финал, чтобы не было "недоворота" (о чем говорили раньше)
        transform.rotation = targetRot;

        yield return new WaitForSeconds(delayBeforeRun);
        animator.SetTrigger("Run");
        state = PlayerState.Running;
        RoadGenerator.instance.StartLevel();
    }


    void Die()
    {
        if (state == PlayerState.Dead) return;

        state = PlayerState.Dead;
        rb.linearVelocity = Vector3.zero;
        animator.SetTrigger("Death");

        if (SoundManager.Instance != null && deathSound != null)
        {
        SoundManager.Instance.PlaySFX(deathSound, deathVolume);
        }

        if (backpackScreen != null) backpackScreen.ShowSad();

        RoadGenerator.instance.StopLevel();

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.HandlePlayerDeath();
        }

        StartCoroutine(ShowGameOverPanelRoutine());

    }

    public void PlayDeathImpactSound()
    {
        if (SoundManager.Instance != null && deathSound != null)
        {
            SoundManager.Instance.PlaySFX(deathSound, 0.2f);
        }
    }
    IEnumerator ShowGameOverPanelRoutine()
    {
        yield return new WaitForSeconds(1.0f);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDeathMenu();
        }
    }

    public void ResetPlayer()
    {
        Time.timeScale = 1f;
        StopAllCoroutines();

        if (SpeedManager.Instance != null)
        {
            SpeedManager.Instance.StartRun();
        }

        if (BuffManager.Instance != null)
        {
            BuffManager.Instance.ResetAllBuffs();
        }

        if (MapGenerator.Instance != null)
        {
            MapGenerator.Instance.ResetMaps();
        }

        state = PlayerState.WaitingStart;

        StartCoroutine(RandomIdleRoutine());

        currentLane = 0;
        previousLane = 0;
        targetLaneX = 0;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 fixedStartPos = new Vector3(0f, 0.52f, -2f);
        Quaternion fixedStartRot = Quaternion.Euler(0f, 180f, 0f);

        rb.position = fixedStartPos;
        rb.rotation = fixedStartRot;

        transform.position = fixedStartPos;
        transform.rotation = fixedStartRot;

        col.height = normalHeight;
        col.center = normalCenter;

        animator.Rebind();
        animator.Update(0f);

        foreach (var parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger)
            {
                animator.ResetTrigger(parameter.name);
            }
        }

        if (backpackScreen != null) backpackScreen.ShowSmile();

        if (cameraController != null)
        {
            cameraController.ResetCamera();
        }
    }

    public void RevivePlayer()
    {
        Time.timeScale = 1f;
        state = PlayerState.Running;

        animator.ResetTrigger("Death");

        animator.SetTrigger("Run");

        rb.linearVelocity = Vector3.zero;

        transform.position = new Vector3(transform.position.x, 0.52f, transform.position.z);

        StartCoroutine(ReviveInvulnerabilityRoutine());
    }

    IEnumerator ReviveInvulnerabilityRoutine()
    {
        isInvulnerable = true;

        float blinkTime = 2f;
        float blinkInterval = 0.2f;

        while (blinkTime > 0)
        {
            foreach (Renderer part in characterParts)
            {
                if (part != null) part.enabled = !part.enabled;
            }

            blinkTime -= blinkInterval;
            yield return new WaitForSeconds(blinkInterval);
        }

        foreach (Renderer part in characterParts)
        {
            if (part != null) part.enabled = true;
        }

        isInvulnerable = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Lose"))
        {
            TryUseShield();
        }

        if (other.CompareTag("NotLose"))
        {
            currentLane = previousLane;
            targetLaneX = currentLane * laneOffset;
        }
    }

    void TryUseShield()
    {
        if (BuffManager.Instance != null && BuffManager.Instance.hasShield)
        {
            StartShieldBreak();
        }
        else if (!isInvulnerable)
        {
            Die();
        }
    }

    public void StartShieldBreak()
    {
        if (shieldBreakCoroutine != null)
        {
            StopCoroutine(shieldBreakCoroutine);
        }
        shieldBreakCoroutine = StartCoroutine(ShieldBreakRoutine());
    }
    public void ActivateShieldVisual()
    {
        if (shieldBreakCoroutine != null)
        {
            StopCoroutine(shieldBreakCoroutine);
            shieldBreakCoroutine = null;
        }

        isInvulnerable = false;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true);
        }
    }

    IEnumerator ShieldBreakRoutine()
    {
        isInvulnerable = true;

        if (SoundManager.Instance != null && shieldBreakSound != null)
            SoundManager.Instance.PlaySFX(shieldBreakSound, 0.7f);

        if (BuffManager.Instance != null)
            BuffManager.Instance.ResetShield();

        if (shieldVisual != null)
        {
            float flashDuration = 1.5f;
            float flashInterval = 0.1f;
            float timer = 0;

            while (timer < flashDuration)
            {
                shieldVisual.SetActive(!shieldVisual.activeSelf);
                yield return new WaitForSeconds(flashInterval);
                timer += flashInterval;
            }

            shieldVisual.SetActive(false);
        }

        isInvulnerable = false;
        shieldBreakCoroutine = null;
    }
    IEnumerator RandomIdleRoutine()
    {
        while (state == PlayerState.WaitingStart)
        {
            yield return new WaitForSeconds(Random.Range(7f, 12f));

            if (state == PlayerState.WaitingStart)
            {
                int randomIndex = Random.Range(1, 4);
                animator.SetInteger("IdleIndex", randomIndex);

                yield return new WaitForSeconds(2f);

                animator.SetInteger("IdleIndex", 0);
            }
        }
    }
    public void PlayFootstep()
    {
        if (!isGrounded || state == PlayerState.Dead) return;

        AudioClip clipToPlay = groundStepSound;

        RaycastHit hit;

        Vector3 rayStart = transform.position + Vector3.up * 0.5f;

        bool isHit = Physics.Raycast(rayStart, Vector3.down, out hit, 2.0f);
        Debug.DrawRay(rayStart, Vector3.down * 2.0f, isHit ? Color.green : Color.red);

        if (isHit)
        {
            if (hit.collider.CompareTag("Roof"))
            {
                clipToPlay = roofStepSound;
            }
        }

        if (SoundManager.Instance != null && clipToPlay != null)
        {
            SoundManager.Instance.PlayVariableSFX(clipToPlay, footstepVolume);
        }
    }
}