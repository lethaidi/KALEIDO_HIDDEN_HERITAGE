using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Board_win : MonoBehaviour
{
    [Header("Data")]
    private String wave_name_data;
    private String result_infor_data;
    private Sprite result_image_data;
    public Wave_data wave_data;

    [Header("Setting")]
    public TextMeshProUGUI wave_name;
    public TextMeshProUGUI result_infor;
    public Image result_image;

    public void Init(Wave_data wave_Data)
    {
        wave_data = wave_Data;

        wave_name_data = wave_Data.wave_name;
        result_infor_data = wave_Data.infor;
        result_image_data = wave_Data.image_result;

        Setup();
    }

    private void Setup()
    {
        wave_name.text = wave_name_data;
        result_infor.text = result_infor_data;
        result_image.sprite = result_image_data;
    }
}
