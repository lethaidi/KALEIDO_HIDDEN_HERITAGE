using UnityEngine;

public class Result_manager : MonoBehaviour
{
    public UI_Animation_Controller[] result = new UI_Animation_Controller[0];

    public void DisableAll()
    {
        foreach(var r in result)
        {
            r.SetVisible(false);
        }
    }

    public void EnableIndex(int index)
    {
        DisableAll();

        result[index].SetVisible(true);
    }
}
