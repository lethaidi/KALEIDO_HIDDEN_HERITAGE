using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MapClickHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mapCamera;
    [SerializeField] private ProvinceView provinceView;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask mapLayer = ~0;
    [SerializeField] private float maxRayDistance = 1000f;

    private void Awake()
    {
        if (mapCamera == null)
            mapCamera = Camera.main;

        if (provinceView == null)
            provinceView = GetComponent<ProvinceView>();
    }

    private void Update()
    {
        if (!TryGetPointerDown(out Vector2 position, out int pointerId))
            return;

        HandleClick(position, pointerId);
    }

    private bool TryGetPointerDown(
        out Vector2 position,
        out int pointerId)
    {
        position = Vector2.zero;
        pointerId = -1;

        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (!touch.press.wasPressedThisFrame)
                    continue;

                position = touch.position.ReadValue();
                pointerId = touch.touchId.ReadValue();
                return true;
            }
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            position = Mouse.current.position.ReadValue();
            pointerId = -1;
            return true;
        }

        return false;
    }

    private void HandleClick(Vector2 position, int pointerId)
    {
        // PRIORITY 1: UI
        if (IsPointerOverUI(pointerId))
        {
            Debug.Log("Clicked UI");
            return;
        }

        if (mapCamera == null)
        {
            Debug.LogWarning("Map Camera is missing!");
            return;
        }

        // PRIORITY 2: 3D Model
        Ray ray = mapCamera.ScreenPointToRay(position);

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            maxRayDistance,
            mapLayer))
        {
            Debug.Log("Clicked 3D: " + hit.collider.name);
            return;
        }

        // PRIORITY 3: Empty Space
        if (provinceView != null)
        {
            provinceView.OutClick();
        }

        Debug.Log("Clicked empty space");
    }

    private bool IsPointerOverUI(int pointerId)
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject(pointerId);
    }
}