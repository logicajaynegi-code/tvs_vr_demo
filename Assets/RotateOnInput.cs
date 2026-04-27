using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RotateOnInput : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 240f;
    [SerializeField] private float smoothTime = 0.07f;
    [SerializeField] private float inertiaDamping = 2f; // higher = stops faster

    [Header("Input")]
    [SerializeField] private InputActionProperty rotateAxisAction;

    [SerializeField] private InputActionProperty autoRotateAction; // A button

    private XRGrabInteractable grabInteractable;
    private bool isGrabbed = false;

    private float currentVelocity;

    [SerializeField] private float autoRotateSpeed = 60f;

    private bool isAutoRotating = false;

    private float rotationVelocity = 0f;
    private float currentYRotationSpeed = 0f;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        if (autoRotateAction.action != null)
            autoRotateAction.action.Enable();
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);

        if (rotateAxisAction.action != null)
            rotateAxisAction.action.Enable();
    }

    private void OnDisable()
    {
        if (autoRotateAction.action != null)
            autoRotateAction.action.Disable();
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);

        if (rotateAxisAction.action != null)
            rotateAxisAction.action.Disable();
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        isAutoRotating = false;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        // Keep currentRotationSpeed → this becomes inertia
    }

    private float lastInputX = 0f;

    private void Update()
    {
        if (!isGrabbed && autoRotateAction.action != null && autoRotateAction.action.WasPressedThisFrame())
        {
            isAutoRotating = !isAutoRotating;
        }
        if (rotateAxisAction.action == null) return;

        float inputX = 0f;

        if (isGrabbed)
        {
            Vector2 input = rotateAxisAction.action.ReadValue<Vector2>();
            inputX = input.x;
            // ✅ DEADZONE goes HERE

            inputX = Mathf.Sign(inputX) * Mathf.Max(0, Mathf.Abs(inputX) - 0.1f);
        }

        // Smooth input instead of speed (KEY FIX)
        float smoothedInput = Mathf.SmoothDamp(
            lastInputX,
            inputX,
            ref currentVelocity,
            smoothTime
        );

        lastInputX = smoothedInput;
        // ✅ ADD HERE
        if (isAutoRotating)
        {
            lastInputX = 0f; // prevents inertia/manual conflict
            currentVelocity = 0f;
        }
        // Apply rotation
        float rotation = 0f;

        if (isAutoRotating && !isGrabbed)
        {
            // Auto rotation (constant speed)
            rotation = autoRotateSpeed;
        }
        else
        {
            // Manual rotation
            float inputStrength = Mathf.Abs(smoothedInput);

            // Boost sensitivity curve (IMPORTANT)
            float boostedInput = Mathf.Pow(inputStrength, 0.4f); // sqrt curve

            float dynamicSpeed = Mathf.Lerp(220f, rotationSpeed, boostedInput);
            rotation = -smoothedInput * dynamicSpeed ;

        }

        // Smooth rotation

        // Smooth rotation speed (NOT angle)
        currentYRotationSpeed = Mathf.SmoothDamp(
            currentYRotationSpeed,
            rotation,
            ref rotationVelocity,
            0.05f // smoothing for rotation itself
        );

        transform.Rotate(Vector3.up, currentYRotationSpeed * Time.deltaTime, Space.World);

        // Inertia when released
        if (!isGrabbed && !isAutoRotating)
        {
            lastInputX = Mathf.Lerp(lastInputX, 0f, inertiaDamping * Time.deltaTime);
            if (Mathf.Abs(lastInputX) < 0.001f)
                lastInputX = 0f;
        }
    }
}