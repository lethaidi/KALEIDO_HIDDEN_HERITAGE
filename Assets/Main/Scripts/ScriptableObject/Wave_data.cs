using UnityEngine;

[CreateAssetMenu(fileName = "Wave_data", menuName = "ScriptableObjects/Wave_data")]
public class Wave_data : ScriptableObject
{
    public string wave_ID;
    public string wave_name;
    public string hint;
    public string infor;
    public Texture2D image_hint;
    public Sprite image_result;
    public Part_main_data part_main;
    public Part_mini_data[] part_minis;
    public int part_mini_count;
    public bool isCompleted =  false;
}
