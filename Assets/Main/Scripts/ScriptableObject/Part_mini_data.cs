using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Part_mini_data", menuName = "ScriptableObjects/Part_mini_data")]
public class Part_mini_data : ScriptableObject
{
    public string partID;
    public GameObject partPrefab;
    public Sprite icon;
}
