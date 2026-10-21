using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Part_Base_rotation_controller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;

    [Header("Rotation Settings")]
    [SerializeField, Range(0.1f, 5f)]
    private float rotationSpeed = 1.5f;

    [SerializeField] public bool enableRotation = true;

    [Header("Optional")]
    [SerializeField] private bool useInertia = true;

    [SerializeField, Min(0f)]
    private float inertiaDamping = 6f;

    private bool isDragging;
    private Vector2 lastPointerPosition;
    private Vector3 angularVelocity;

    private int activeTouchId = -1;

    [Header("Validation")]
    public PuzzleValidationController puzzleValidationController;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (puzzleValidationController == null)
            puzzleValidationController = Object.FindAnyObjectByType<PuzzleValidationController>();
    }

    private void Update()
    {
        if (!enableRotation)
            return;

        HandleInput();
        HandleInertia();
    }

    private void HandleInput()
    {
        // Ưu tiên touch khi chạy Android.
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.isPressed)
        {
            HandleTouch();
            return;
        }

        // Kết thúc touch khi nhấc tay.
        if (activeTouchId != -1)
        {
            EndDrag();
            activeTouchId = -1;
            return;
        }

        HandleMouse();
    }

    private void HandleMouse()
    {
        if (Mouse.current == null)
            return;

        Vector2 pointerPosition =
            Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            BeginDrag(pointerPosition);
        }

        if (Mouse.current.leftButton.isPressed && isDragging)
        {
            Drag(pointerPosition);
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            EndDrag();
        }
    }

    private void HandleTouch()
    {
        var touch = Touchscreen.current.primaryTouch;

        Vector2 pointerPosition =
            touch.position.ReadValue();

        if (activeTouchId == -1)
        {
            activeTouchId = touch.touchId.ReadValue();
            BeginDrag(pointerPosition);
        }

        if (isDragging)
        {
            Drag(pointerPosition);
        }
    }

    private void BeginDrag(Vector2 pointerPosition)
    {
        // Không xoay nếu nhấn vào UI
        if (IsPointerOverUI(pointerPosition))
            return;

        isDragging = true;
        lastPointerPosition = pointerPosition;
        angularVelocity = Vector3.zero;

        puzzleValidationController?.BeginInteraction();
    }

    private void Drag(Vector2 pointerPosition)
    {
        if (!isDragging)
            return;

        // Nếu kéo chuột vào UI thì dừng xoay
        if (IsPointerOverUI(pointerPosition))
        {
            EndDrag();
            return;
        }

        Vector2 delta = pointerPosition - lastPointerPosition;
        lastPointerPosition = pointerPosition;

        if (delta.sqrMagnitude < 0.01f)
            return;

        ApplyArcballRotation(delta);
    }

    private void ApplyArcballRotation(Vector2 delta)
    {
        if (targetCamera == null)
            return;

        // Chuẩn hóa theo độ phân giải màn hình.
        float screenSize = Mathf.Min(Screen.width, Screen.height);

        if (screenSize <= 0f)
            return;

        Vector2 normalizedDelta = delta / screenSize;

        float angleX = normalizedDelta.y * 180f * rotationSpeed;
        float angleY = -normalizedDelta.x * 180f * rotationSpeed;

        // Trục xoay dựa trên camera.
        Vector3 cameraRight = targetCamera.transform.right;
        Vector3 cameraUp = targetCamera.transform.up;

        Quaternion rotationX =
            Quaternion.AngleAxis(angleX, cameraRight);

        Quaternion rotationY =
            Quaternion.AngleAxis(angleY, cameraUp);

        Quaternion rotationDelta = rotationY * rotationX;

        transform.rotation = rotationDelta * transform.rotation;

        if (useInertia && Time.deltaTime > 0f)
        {
            Vector3 axis;
            float angle;

            rotationDelta.ToAngleAxis(out angle, out axis);

            if (angle > 180f)
                angle -= 360f;

            angularVelocity =
                axis.normalized * (angle / Time.deltaTime);
        }
    }

    private void EndDrag()
    {
        // Tránh gọi EndInteraction khi chưa bắt đầu kéo
        if (!isDragging)
            return;

        isDragging = false;

        if (!useInertia)
            angularVelocity = Vector3.zero;

        puzzleValidationController?.EndInteraction();
    }

    private void HandleInertia()
    {
        if (isDragging || !useInertia)
            return;

        if (angularVelocity.sqrMagnitude < 0.01f)
        {
            angularVelocity = Vector3.zero;
            return;
        }

        float angle = angularVelocity.magnitude * Time.deltaTime;

        Quaternion rotation =
            Quaternion.AngleAxis(
                angle,
                angularVelocity.normalized
            );

        transform.rotation = rotation * transform.rotation;

        angularVelocity = Vector3.Lerp(
            angularVelocity,
            Vector3.zero,
            inertiaDamping * Time.deltaTime
        );
    }

    public void SetRotationEnabled(bool enabled)
    {
        enableRotation = enabled;

        if (!enabled)
        {
            isDragging = false;
            angularVelocity = Vector3.zero;
            activeTouchId = -1;
        }
    }

    public void ResetRotation()
    {
        transform.rotation = Quaternion.identity;
        angularVelocity = Vector3.zero;
        isDragging = false;
        activeTouchId = -1;
    }

    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData pointerData =
            new PointerEventData(EventSystem.current)
            {
                position = screenPosition
            };

        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                return true;
            }
        }

        return false;
    }
}
