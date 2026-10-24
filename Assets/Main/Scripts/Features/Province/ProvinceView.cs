using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProvinceView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ProvinceData provinceData;
    [SerializeField] private Transform standPoint;
    [SerializeField] private MascotController mascotController;
    [SerializeField] private UI_Animation_Controller province_board;
    private Part_interact_ani part_Interact_Ani;
    bool openBoard = false;

    [Header("Wave Data")]
    public Wave_data wave_data;
    public Image icon;

    void LateUpdate()
    {
        if (wave_data != null && wave_data.isCompleted && icon != null)
        {
            icon.sprite = wave_data.image_result;
        }
    }

    private void Awake()
    {
        if (provinceData == null || standPoint == null)
        {
            Debug.LogError(
                $"{name}: Chưa gán ProvinceData hoặc StandPoint.", this);

            return;
        }

        provinceData.standPoint = standPoint;

        if (mascotController == null)
        {
            Debug.LogError(
                $"{name}: Chưa gán MascotController.", this);
        }

        if (province_board == null) province_board = GetComponentInChildren<UI_Animation_Controller>();
        if (part_Interact_Ani == null) part_Interact_Ani = GetComponent<Part_interact_ani>();
    }

    public void OutClick()
    {
        if (province_board != null)
        {
            province_board.SetVisible(false);
            openBoard = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (provinceData == null ||
            standPoint == null ||
            mascotController == null)
        {
            return;
        }

        if (part_Interact_Ani != null) part_Interact_Ani.PlayAnimate();

        mascotController.DisableAllProvinceView();

        if (province_board != null)
        {
            province_board.SetVisible(!openBoard);
            openBoard = !openBoard;
        }
    }

    public void MoveToProvinceTarget(int index)
    {
        if (province_board != null)
        {
            province_board.SetVisible(!openBoard);
            openBoard = !openBoard;
        }

        if (mascotController != null)
            mascotController.MoveToProvince(provinceData, index);
    }
}
