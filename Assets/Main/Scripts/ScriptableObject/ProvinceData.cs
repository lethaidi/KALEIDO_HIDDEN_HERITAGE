using UnityEngine;

[CreateAssetMenu(fileName = "ProvinceData", menuName = "World Map/Province Data")]
public class ProvinceData : ScriptableObject
{
    public int provinceID;
    public bool isComplete;
    public Transform standPoint;

}
