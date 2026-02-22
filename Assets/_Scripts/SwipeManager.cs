using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeManager : MonoBehaviour
{
    public static SwipeManager instance;

    public delegate void SwipeDelegate(Vector2 direction);
    public event SwipeDelegate OnSwipe;

    private PlayerControls controls;

    private Vector2 startPosition;

    [Header("Settings")]
    [Tooltip("Минимальная дистанция в пикселях. 50-100 обычно комфортно.")]
    [SerializeField] private float deadzone = 50f;
    [SerializeField] private float maximumTime = 0.5f;
    private float startTime;

    private void Awake()
    {
        instance = this;
        controls = new PlayerControls();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Start()
    {
        controls.Player.PrimaryContact.started += ctx => {
            startPosition = controls.Player.PrimaryPosition.ReadValue<Vector2>();
            startTime = (float)ctx.startTime;
        };

        controls.Player.PrimaryContact.canceled += ctx => DetectSwipe(ctx);
    }

    private void DetectSwipe(InputAction.CallbackContext context)
    {
        Vector2 endPosition = controls.Player.PrimaryPosition.ReadValue<Vector2>();
        float duration = (float)context.time - startTime;

        Vector2 diff = endPosition - startPosition;
        float distance = diff.magnitude;

        if (distance < deadzone || duration > maximumTime) return;

        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            OnSwipe?.Invoke(diff.x > 0 ? Vector2.right : Vector2.left);
        }
        else
        {
            OnSwipe?.Invoke(diff.y > 0 ? Vector2.up : Vector2.down);
        }
    }
}