using UnityEngine;
using UnityEngine.InputSystem;

public class Snap_point_raycaster : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask raycastMask = ~0;

    private void Awake()
    {
        if (mainCamera == null) mainCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        // Chuột trên PC
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckInteraction(Mouse.current.position.ReadValue());

            return;
        }

        // Cảm ứng trên Mobile
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            CheckInteraction(Touchscreen.current.primaryTouch.position.ReadValue());
        }
    }

    private void CheckInteraction(Vector2 screenPosition)
    {
        Debug.Log("Clicked at: " + screenPosition);

        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is NULL!");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, raycastMask, QueryTriggerInteraction.Collide))
        {
            Debug.Log("Raycast hit: " + hit.collider.name);

            Snap_point_trigger trigger = hit.collider.GetComponentInParent<Snap_point_trigger>();

            if (trigger != null)
            {
                Debug.Log("Found SnapPointTrigger!");

                trigger.OnClicked();
            }
        }
        else
        {
            Debug.Log("Raycast không trúng Object nào!");
        }
    }
}