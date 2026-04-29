using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RotateOnInput : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 240f;
    [SerializeField] private float smoothTime = 0.03f;   // LOW = responsive, was 0.07 (too sluggish)
    [SerializeField] private float inertiaDamping = 2f;
    [SerializeField] private float deadzone = 0.1f;

    [Header("Auto Rotation")]
    [SerializeField] private float autoRotateSpeed = 60f;

    [Header("Input Actions")]
    [SerializeField] private InputActionProperty rotateAxisAction;
    [SerializeField] private InputActionProperty autoRotateAction;

    // public setters for VR slider panel
    public void SetRotationSpeed(float v) => rotationSpeed = v;
    public void SetSmoothTime(float v) => smoothTime = Mathf.Max(0.01f, v);
    public void SetInertiaDamping(float v) => inertiaDamping = v;
    public void SetAutoRotateSpeed(float v) => autoRotateSpeed = v;

    private XRGrabInteractable grabInteractable;
    private bool isGrabbed = false;
    private bool isAutoRotating = false;

    private float smoothedInput = 0f;
    private float inputVelocity = 0f;   // SmoothDamp ref — only ONE, no chaining

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        rotateAxisAction.action?.Enable();
        autoRotateAction.action?.Enable();
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        rotateAxisAction.action?.Disable();
        autoRotateAction.action?.Disable();
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        isAutoRotating = false;
        // clear smoothing state so rotation starts fresh on grab
        smoothedInput = 0f;
        inputVelocity = 0f;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        // smoothedInput keeps its value → becomes inertia coast
    }

    private void Update()
    {
        // toggle auto-rotate (only when not holding the model)
        if (!isGrabbed && autoRotateAction.action != null && autoRotateAction.action.WasPressedThisFrame())
            isAutoRotating = !isAutoRotating;

        // ── auto rotation ────────────────────────────────────────────────────
        if (isAutoRotating && !isGrabbed)
        {
            transform.Rotate(Vector3.up, autoRotateSpeed * Time.deltaTime, Space.World);
            smoothedInput = 0f;
            inputVelocity = 0f;
            return;                 // skip everything else — keeps auto smooth
        }

        // ── read thumbstick ──────────────────────────────────────────────────
        float rawInput = 0f;

        if (isGrabbed && rotateAxisAction.action != null)
        {
            float x = rotateAxisAction.action.ReadValue<Vector2>().x;

            // deadzone: strip noise below threshold, rescale remainder to 0-1
            if (Mathf.Abs(x) > deadzone)
                rawInput = Mathf.Sign(x) * (Mathf.Abs(x) - deadzone) / (1f - deadzone);
        }

        // ── single SmoothDamp on raw input (FIX: was double-smoothed before) ─
        smoothedInput = Mathf.SmoothDamp(smoothedInput, rawInput, ref inputVelocity, smoothTime);

        // ── apply rotation ───────────────────────────────────────────────────
        transform.Rotate(Vector3.up, -smoothedInput * rotationSpeed * Time.deltaTime, Space.World);

        // ── inertia coast after release ──────────────────────────────────────
        if (!isGrabbed)
        {
            smoothedInput = Mathf.Lerp(smoothedInput, 0f, inertiaDamping * Time.deltaTime);
            if (Mathf.Abs(smoothedInput) < 0.001f) smoothedInput = 0f;
        }
    }
}