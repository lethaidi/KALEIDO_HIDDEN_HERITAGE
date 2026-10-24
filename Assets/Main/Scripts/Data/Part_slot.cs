using UnityEngine;
using UnityEngine.UI;

public class Part_slot : MonoBehaviour
{
    [Header("Part Mini Data")]
    public Part_mini_data part_Mini_Data;

    [Header("Slot Data")]
    public GameObject hightlight;

    [Header("Part slot info")]
    public bool isEmpty = false;

    [Header("UI")]
    public Image partIcon;
    public Image back;
    void Update()
    {
        if (part_Mini_Data != null)
            partIcon.sprite = part_Mini_Data.icon;
    }

    public void SlotEmpty()
    {
        hightlight.SetActive(false);
        isEmpty = true;
        // Giảm color của icon thành xám để hiển thị rằng slot trống
        back.color = Color.gray; // Màu xám
    }

    public void SlotFilled()
    {
        isEmpty = false;
        // Khôi phục color của icon về màu trắng để hiển thị rằng slot đã được lấp đầy
        back.color = Color.white; // Màu trắng
    }
}
