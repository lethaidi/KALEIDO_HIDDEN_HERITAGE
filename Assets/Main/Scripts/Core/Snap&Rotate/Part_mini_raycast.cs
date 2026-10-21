
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Part_mini_raycast : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Part_mini_rotate_manager partManager;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask raycastLayers = ~0;
    [SerializeField] private float maxDistance = 100f;

    private Part_mini_trigger selectedPart;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Update()
    {
        if (!TryGetPointerDown(out Vector2 position))
            return;

        // Không chọn object 3D khi click UI.
        if (IsPointerOverUI(position))
            return;

        HandleRaycast(position);
    }

    private bool TryGetPointerDown(out Vector2 position)
    {
        position = Vector2.zero;

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            position = Touchscreen.current.primaryTouch
                .position.ReadValue();
            return true;
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            position = Mouse.current.position.ReadValue();
            return true;
        }

        return false;
    }

    private bool IsPointerOverUI(Vector2 position)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData data =
            new PointerEventData(EventSystem.current)
            {
                position = position
            };

        List<RaycastResult> results =
            new List<RaycastResult>();

        EventSystem.current.RaycastAll(data, results);

        // Chỉ bỏ qua khi có UI được raycast.
        foreach (var result in results)
        {
            if (result.module is UnityEngine.UI.GraphicRaycaster)
                return true;
        }

        return false;
    }

    private void HandleRaycast(Vector2 position)
    {
        if (targetCamera == null)
            return;

        Ray ray = targetCamera.ScreenPointToRay(position);

        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red, 1f);

        // Raycast vật thể đầu tiên, kể cả vật cản.
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, raycastLayers, QueryTriggerInteraction.Collide))
        {
            Part_mini_trigger trigger = hit.collider.GetComponentInParent<Part_mini_trigger>();

            if (trigger != null)
            {
                SelectPart(trigger);
                return;
            }
        }

        DeselectPart();
    }

    private void SelectPart(Part_mini_trigger part)
    {
        selectedPart = part;
        part.OnClicked();

        Debug.Log("Selected: " + part.name);
    }

    private void DeselectPart()
    {
        if (selectedPart != null)
        {
            selectedPart.OnClickedBackground();
            selectedPart = null;
        }
        else if (partManager != null)
        {
            partManager.DisableAllMinis();
        }
    }
}
