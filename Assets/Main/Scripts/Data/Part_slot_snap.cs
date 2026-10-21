using UnityEngine;
using UnityEngine.UI;

public class Part_slot_snap : MonoBehaviour
{
    public Part_mini_data partMiniData;
    public Image icon;

    public void AddData(Part_mini_data data)
    {
        if (data == null) return;   
        partMiniData = data;

        if (partMiniData.icon != null) 
            icon.sprite = partMiniData.icon;
    }
}
