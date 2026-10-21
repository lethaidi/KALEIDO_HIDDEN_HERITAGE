using UnityEngine;
using UnityEngine.InputSystem;

public class Snap_system : MonoBehaviour
{
    public Transform[] snapPoints; // Mảng các snapPoint
    public GameObject partPrefab;
    public int snapIndex; // Chỉ số snapPoint được chọn
    public Part_mini_rotate_manager partManager;
    public Slot_part_interact slotPartInteract;

    void Awake()
    {
        if (slotPartInteract == null)
        {
            slotPartInteract = Object.FindAnyObjectByType<Slot_part_interact>();
        }
    }

    void Update()
    {
    }

    public void SnapPartToSnapPoint(int snapIndex, Transform spawn_point)
    {
        if (partPrefab == null || snapPoints == null || snapIndex < 0 || snapIndex >= snapPoints.Length)
        {
            Debug.Log("Invalid partPrefab or snapPoints or snapIndex.");
            return;
        }
        
        snapPoints[snapIndex].gameObject.SetActive(false);

        GameObject part = partPrefab;
        // Spawn part tại vị trí snapPoint và part này là con của object chứa script Snap_system
        GameObject spawnedPart = Instantiate(part, spawn_point.position, spawn_point.rotation, transform);

        // Thêm data cho part
        if (spawnedPart != null)
        {
            Part_mini p_data = spawnedPart.GetComponent<Part_mini>();
            p_data.snap_point_index = snapIndex;
        }

        // Thêm part mới vào danh sách quản lý của partManager
        if (partManager != null)
        {
            Part_Base_rotation_controller partController = spawnedPart.GetComponent<Part_Base_rotation_controller>();
            if (partController != null)
            {
                partManager.AddPart(partController);
            }
        }

        // Đánh dấu là slot của slotPartInteract trống
        if (slotPartInteract != null)
        {
            slotPartInteract.part_slots[slotPartInteract.partSlotIndex].SlotEmpty();
        }

        partPrefab = null;
    }

    public void ChooseSnapPointIndex(int snapIndex, Transform spawn_point)
    {
        this.snapIndex = snapIndex;

        SnapPartToSnapPoint(snapIndex, spawn_point);
    }
}
