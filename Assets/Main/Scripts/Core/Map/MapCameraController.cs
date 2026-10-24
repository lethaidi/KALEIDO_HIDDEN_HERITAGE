using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using ETouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using InputTouchPhase = UnityEngine.InputSystem.TouchPhase;

[RequireComponent(typeof(Camera))]
public class MapCameraController : MonoBehaviour
{
    // Keep existing enum values and field names to preserve Inspector data.
    public enum ZoomMethod { FieldOfView, PositionY }

    [Header("1. Pan")]
    [SerializeField] private float pcPanSpeed = 25f;
    [SerializeField] private float mobilePanSpeed = 0.8f;
    [SerializeField] private bool allowLeftMouseDrag = true;

    [Header("2. Zoom")]
    [SerializeField] private ZoomMethod zoomMethod = ZoomMethod.FieldOfView;
    [SerializeField] private float pcZoomSpeed = 400f;
    [SerializeField] private float mobileZoomSpeed = 5f;
    [SerializeField] private float minZoom = 25f;
    [SerializeField] private float maxZoom = 80f;

    [Header("3. Bounds X & Z (camera position)")]
    [SerializeField] private float minX = -20f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minZ = -45f;
    [SerializeField] private float maxZ = -15f;

    private Camera cam;
    private readonly List<RaycastResult> uiHits = new List<RaycastResult>();
    private readonly HashSet<int> blockedTouches = new HashSet<int>();
    private bool mouseDragging;
    private bool touchWasActive;
    private bool pinchReady;
    private int pinchId0, pinchId1;
    private float previousPinchDistance;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        float initialZoom = zoomMethod == ZoomMethod.FieldOfView
            ? cam.fieldOfView : transform.position.y;
        minZoom = Mathf.Min(minZoom, initialZoom);
        maxZoom = Mathf.Max(maxZoom, initialZoom);
    }

    private void OnEnable() { EnhancedTouchSupport.Enable(); }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
        ResetGestures();
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused) ResetGestures();
    }

    private void ResetGestures()
    {
        mouseDragging = false;
        touchWasActive = false;
        pinchReady = false;
        blockedTouches.Clear();
    }

    private void Update()
    {
        if (ETouch.activeTouches.Count > 0)
        {
            touchWasActive = true;
            mouseDragging = false;
            HandleMobileInput();
            return;
        }
        if (touchWasActive)
        {
            touchWasActive = false;
            pinchReady = false;
            blockedTouches.Clear();
            return;
        }
        HandlePCInput();
    }

    private bool IsOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;
        var pointer = new PointerEventData(EventSystem.current) { position = screenPosition };
        uiHits.Clear();
        EventSystem.current.RaycastAll(pointer, uiHits);
        foreach (var hit in uiHits)
            if (hit.module is GraphicRaycaster) return true;
        return false;
    }

    private void HandlePCInput()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;
        Vector2 position = mouse.position.ReadValue();
        bool pressed = mouse.rightButton.wasPressedThisFrame ||
            (allowLeftMouseDrag && mouse.leftButton.wasPressedThisFrame);
        bool held = mouse.rightButton.isPressed ||
            (allowLeftMouseDrag && mouse.leftButton.isPressed);
        if (pressed) mouseDragging = !IsOverUI(position);
        if (!held) mouseDragging = false;

        if (mouseDragging && !IsOverUI(position))
        {
            // Pixel delta: no deltaTime multiplication. Separate calibration
            // keeps the old Inspector speed values useful as a starting point.
            Vector2 delta = mouse.delta.ReadValue();
            ApplyPan(-delta.x * pcPanSpeed * 0.001f,
                     -delta.y * pcPanSpeed * 0.001f);
        }
        if (!IsOverUI(position))
        {
            float scroll = mouse.scroll.ReadValue().y;
            // Scroll units vary by device/platform; tune pcZoomSpeed if needed.
            if (Mathf.Abs(scroll) > 0.001f)
                ApplyZoom(scroll * pcZoomSpeed * 0.0001f);
        }
    }

    private void HandleMobileInput()
    {
        var touches = ETouch.activeTouches;
        foreach (var touch in touches)
        {
            if (touch.phase == InputTouchPhase.Began)
            {
                blockedTouches.Remove(touch.touchId);
                if (IsOverUI(touch.screenPosition)) blockedTouches.Add(touch.touchId);
            }
        }

        if (touches.Count == 1)
        {
            pinchReady = false;
            var touch = touches[0];
            if (touch.phase == InputTouchPhase.Moved &&
                !blockedTouches.Contains(touch.touchId) && !IsOverUI(touch.screenPosition))
            {
                ApplyPan(-touch.delta.x * mobilePanSpeed * 0.01f,
                         -touch.delta.y * mobilePanSpeed * 0.01f);
            }
        }
        else if (touches.Count == 2)
        {
            var a = touches[0];
            var b = touches[1];
            bool ended = a.phase == InputTouchPhase.Ended || a.phase == InputTouchPhase.Canceled ||
                         b.phase == InputTouchPhase.Ended || b.phase == InputTouchPhase.Canceled;
            if (ended || blockedTouches.Contains(a.touchId) || blockedTouches.Contains(b.touchId) ||
                IsOverUI(a.screenPosition) || IsOverUI(b.screenPosition))
            {
                pinchReady = false;
                return;
            }
            float distance = Vector2.Distance(a.screenPosition, b.screenPosition);
            if (pinchReady && a.touchId == pinchId0 && b.touchId == pinchId1)
                ApplyZoom((distance - previousPinchDistance) * mobileZoomSpeed * 0.01f);
            previousPinchDistance = distance;
            pinchId0 = a.touchId;
            pinchId1 = b.touchId;
            pinchReady = true;
        }
        else pinchReady = false;

        foreach (var touch in touches)
            if (touch.phase == InputTouchPhase.Ended || touch.phase == InputTouchPhase.Canceled)
                blockedTouches.Remove(touch.touchId);
    }

    private void ApplyPan(float moveX, float moveZ)
    {
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x + moveX, minX, maxX);
        position.z = Mathf.Clamp(position.z + moveZ, minZ, maxZ);
        transform.position = position;
    }

    private void ApplyZoom(float amount)
    {
        if (zoomMethod == ZoomMethod.FieldOfView)
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - amount, minZoom, maxZoom);
        else
        {
            Vector3 position = transform.position;
            position.y = Mathf.Clamp(position.y - amount, minZoom, maxZoom);
            transform.position = position;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((minX + maxX) * 0.5f, transform.position.y,
                                     (minZ + maxZ) * 0.5f);
        Gizmos.DrawWireCube(center, new Vector3(Mathf.Abs(maxX - minX), 0.5f,
                                               Mathf.Abs(maxZ - minZ)));
    }
}
