using UnityEngine;
using UnityEngine.UI;

public class Wave_slot : MonoBehaviour
{
    public Wave_data wave_data;
    public Image icon;

    void LateUpdate()
    {
        ChangeIconState();
    }

    public void AddData(Wave_data data)
    {
        wave_data = data;  
    }

    public void ChangeIconState()
    {
        if (wave_data != null && wave_data.isCompleted)
            icon.color = Color.grey;
    }
}
