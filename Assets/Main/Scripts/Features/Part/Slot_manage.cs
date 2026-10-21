using UnityEngine;
using UnityEngine.UI;

public class Slot_manage : MonoBehaviour
{
    public GameObject slot_prefab;
    public Part_slot[] part_slot_lists;
    public Slot_part_interact slot_Part_Interact;

    public void SpawnSlot(int count)
    {
        if (part_slot_lists.Length > 0)
        {
            foreach (var p in part_slot_lists)
            {
                Destroy(p.gameObject);
            }
        }

        part_slot_lists = System.Array.Empty<Part_slot>();
        part_slot_lists = new Part_slot[count];

        for (int i = 0; i < count; i++)
        {
            GameObject slot = Instantiate(slot_prefab, transform);
            part_slot_lists[i] = slot.GetComponent<Part_slot>();

            //Đăng ký sự kiện cho button
            Button but = slot.GetComponent<Button>();
            if (but != null)
            {
                int slotIndex = i;

                but.onClick.AddListener(() =>
                {
                    slot_Part_Interact.ClickSlot(slotIndex);
                });
            }
        }

        slot_Part_Interact.part_slots = part_slot_lists;
    }
}
