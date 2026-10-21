using UnityEngine;

public class Part_mini_trigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Part_mini_rotate_manager partManager;
    [SerializeField] private Part_Base_rotation_controller part;
    public bool canInteract = false;

    private void Awake()
    {
        if (partManager == null)
            partManager = Object.FindAnyObjectByType<Part_mini_rotate_manager>();
    }
    
    public void OnClicked()
    {  
        Debug.Log("Mini Trigger Clicked: " + name);

        if (partManager != null && part != null)
            partManager.EnablePartIndex(part);
    }

    public void OnClickedBackground()
    {   /*
        Debug.Log("Background Clicked!");

        if (partManager != null)
            partManager.DisableAllMinis(); */
    }

    public void ChangeState()
    {
        canInteract = !canInteract;

        if (canInteract == true)
        {
            if (partManager != null && part != null)
            partManager.EnablePartIndex(part);
        }
        else
        {
            if (partManager != null)
            partManager.DisableAllMinis();
        }
    }
}
