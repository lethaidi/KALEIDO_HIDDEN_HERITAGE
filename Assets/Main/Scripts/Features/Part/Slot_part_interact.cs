using UnityEngine;

public class Slot_part_interact : MonoBehaviour
{
    [Header("Part Data")]
    public Part_mini_data[] part_mini_datas = new Part_mini_data[0];
    public Part_mini_data currentPartMiniData;

    [Header("Slot Data")]
    public Part_slot[] part_slots = new Part_slot[0];
    public int partSlotIndex = 0;

    [Header("Snap System")]
    public Snap_system snapSystem;
    
    public void ClickSlot(int index)
    {
        if (part_slots[index].isEmpty == true)
        {
            Debug.Log("Slot is empty");
            return;
        }
        // Kiemr tra có snapSystem + Tìm
        if (snapSystem == null)
        {
            snapSystem = Object.FindAnyObjectByType<Snap_system>();

            if (snapSystem == null) return;
        }

        if (index == partSlotIndex)
        {
            partSlotIndex = -1;
            UnEnableAllSlotHighlights();

            snapSystem.partPrefab = null;

            Debug.Log("Slot is already selected");
            return;
        }

        UnEnableAllSlotHighlights();

        currentPartMiniData = part_mini_datas[index];
        partSlotIndex = index;
        snapSystem.partPrefab = currentPartMiniData.partPrefab;

        EnableSlotHighlight(index);
    }

    private void UnEnableAllSlotHighlights()
    {
        foreach (var slot in part_slots)
        {
            slot.hightlight.SetActive(false);
        }
    }

    private void EnableSlotHighlight(int index)
    {
        part_slots[index].hightlight.SetActive(true);
    }

    public void RestoreSlot(string ID)
    {
        foreach (var part_mini_data in part_mini_datas)
        {
            if (part_mini_data.partID == ID)
            {
                int index = System.Array.IndexOf(part_mini_datas, part_mini_data); // Tìm chỉ số của phần tử trong mảng
                part_slots[index].SlotFilled();
            }
        }
    }

    public void RestoreAll()
    {
        foreach (var part_slot in part_slots)
        {
            part_slot.SlotFilled();
        }
    }

    public void ResetPartMiniList()
    {
        // Reset mảng part_mini rỗng
        part_mini_datas = System.Array.Empty<Part_mini_data>();
    }

    public void AddPartMiniList(Part_mini_data[] part_Mini_Datas)
    {
        ResetPartMiniList();

        part_mini_datas = part_Mini_Datas;

        //Add vào partSlot
        for (int i=0; i<part_slots.Length; i++)
        {
            part_slots[i].part_Mini_Data = part_mini_datas[i];
        }
    }
}
