using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class Part_mini_rotate_manager : MonoBehaviour
{
    public Part_Base_rotation_controller part_main;
    public Snap_system snap_System;

    public Part_Base_rotation_controller[] part_minis =
        Array.Empty<Part_Base_rotation_controller>();

    public Part_Base_rotation_controller temp;

    public bool canDeletePart = false;

    public Slot_part_interact slotPartInteract;

    [Header("UI")]
    public Image delete_But;
    public Transform Snap_part_board;
    public GameObject slot_prefab;
    public GameObject[] slot_snap_part =
        Array.Empty<GameObject>();

    public Button_highlight_click button_Highlight_Click;

    void Awake()
    {
        if (slotPartInteract == null)
        {
            slotPartInteract = FindAnyObjectByType<Slot_part_interact>();
        }
    }

    public void AddPart(Part_Base_rotation_controller part)
    {
        if (part == null)
            return;

        Part_mini partMini = part.GetComponent<Part_mini>();

        if (partMini == null || partMini.partMiniData == null)
        {
            Debug.LogError("Part Mini chưa có Part_mini_data!");
            return;
        }

        if (Snap_part_board == null || slot_prefab == null)
        {
            Debug.LogError("Thiếu Snap_part_board hoặc slot_prefab!");
            return;
        }

        if (part_minis == null)
            part_minis = Array.Empty<Part_Base_rotation_controller>();

        if (slot_snap_part == null)
            slot_snap_part = Array.Empty<GameObject>();

        // Không thêm trùng một đối tượng.
        if (Array.IndexOf(part_minis, part) >= 0)
            return;

        // Tạo Slot UI trước.
        GameObject slot = Instantiate(slot_prefab, Snap_part_board);

        Part_slot_snap slotData = slot.GetComponent<Part_slot_snap>();
        slotData.AddData(partMini.partMiniData);

        // Cập nhật hai mảng tương ứng.
        int oldLength = part_minis.Length;

        Array.Resize(ref part_minis, oldLength + 1);
        Array.Resize(ref slot_snap_part, oldLength + 1);

        part_minis[oldLength] = part;
        slot_snap_part[oldLength] = slot;

        // Khởi tạo sau khi dữ liệu đã được gán.
        if (button_Highlight_Click != null)
            button_Highlight_Click.InitButtons();

        // Đăng ký sự kiện cho button
        Part_mini_trigger part_trigger = part_minis[oldLength].gameObject.GetComponent<Part_mini_trigger>();
        Part_interact_ani part_Interact_Ani = part_minis[oldLength].gameObject.GetComponent<Part_interact_ani>();
        Button but = slot_snap_part[oldLength].GetComponent<Button>();
        if (but != null)
        {
            but.onClick.AddListener(() =>
            {
                part_trigger.ChangeState();
            });
            but.onClick.AddListener(() =>
            {
                part_Interact_Ani.PlayAnimate();
            });
        }
    }

    public void RemovePart(Part_Base_rotation_controller part)
    {
        if (part == null || part_minis == null)
            return;

        int index = Array.IndexOf(part_minis, part);

        if (index < 0)
            return;

        if (index == button_Highlight_Click.button_index) DisableAllMinis();

        Part_mini partMini = part.GetComponent<Part_mini>();

        if (partMini != null && partMini.partMiniData != null)
        {
            if (slotPartInteract != null)
            {
                slotPartInteract.RestoreSlot(
                    partMini.partMiniData.partID
                );
            }

            if (snap_System != null &&
                snap_System.snapPoints != null &&
                partMini.snap_point_index >= 0 &&
                partMini.snap_point_index < snap_System.snapPoints.Length)
            {
                Transform snapPoint =
                    snap_System.snapPoints[partMini.snap_point_index];

                if (snapPoint != null)
                    snapPoint.gameObject.SetActive(true);
            }
        }

        // Xóa Slot UI đúng với Part đã chọn.
        if (slot_snap_part != null && index < slot_snap_part.Length)
        {
            GameObject slot = slot_snap_part[index];

            if (slot != null)
            {
                slot.SetActive(false);
                Destroy(slot);
            }

            List<GameObject> slots = new List<GameObject>(slot_snap_part);

            slots.RemoveAt(index);
            slot_snap_part = slots.ToArray();
        }
        
        if (button_Highlight_Click != null)
        {
            button_Highlight_Click.RemoveButtonAt(index);
        }

        // Loại Part khỏi mảng.
        List<Part_Base_rotation_controller> parts = new List<Part_Base_rotation_controller>(part_minis);

        parts.RemoveAt(index);

        part_minis = parts.ToArray();
    }

    public void EnablePartIndex(Part_Base_rotation_controller part)
    {
        if (part == null || part_main == null || part_minis == null)
            return;

        if (canDeletePart)
        {
            ChangeDeletePartState();

            RemovePart(part);
            Destroy(part.gameObject);

            return;
        }

        Part_mini_trigger trigger = part.gameObject.GetComponent<Part_mini_trigger>();
        if (trigger != null && trigger.canInteract == false) return;

        part_main.SetRotationEnabled(false);

        foreach (var p in part_minis)
        {
            if (p == null)
                continue;

            p.SetRotationEnabled(p == part);

            // Tắt điều kiện Trigger
            if (p != part)
            {
                Part_mini_trigger part_trigger = p.gameObject.GetComponent<Part_mini_trigger>();
                part_trigger.canInteract = false;
            }
        }

        temp = part;
    }

    public void DisableAllMinis()
    {
        temp = null;

        if (part_minis != null)
        {
            foreach (var part in part_minis)
            {
                if (part != null)
                    part.SetRotationEnabled(false);
            }
        }

        if (part_main != null)
            part_main.SetRotationEnabled(true);
    }

    public void ChangeDeletePartState()
    {
        canDeletePart = !canDeletePart;
        ChangeDeleteButState(canDeletePart);
    }

    private void ChangeDeleteButState(bool click)
    {
        if (delete_But == null)
            return;

        delete_But.color = click ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;
    }

    public void ResetAllPartRotate()
    {
        if (part_main != null) part_main.ResetRotation();

        foreach (var p in part_minis)
        {
            p.ResetRotation();
        }
    }
}
