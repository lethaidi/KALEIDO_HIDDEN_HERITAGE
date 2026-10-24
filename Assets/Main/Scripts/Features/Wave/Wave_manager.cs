using UnityEngine;
using UnityEngine.UI;

public enum WaveState
{
    Idle,
    Start,
    Win
}

public class Wave_manager : MonoBehaviour
{
    [Header("Wave state")]
    public WaveState waveState = WaveState.Idle;

    #region 
    [Header("Wave data")]
    public Wave_data[] wave_datas;
    public int wave_index;
    [SerializeField] private Transform part_main_spawnPoint;
    private GameObject part_main;
    public GameObject wave_screen;
    public GameObject wave_board;
    public Transform wave_grid;
    public Wave_slot[] wave_Slots = new Wave_slot[0];
    #endregion

    #region 
    [Header("Slot data")]
    public Slot_part_interact slot_Part_Interact;
    public Slot_manage slot_Manage;
    #endregion

    #region 
    [Header("Validation")]
    public ImageSimilarityTester imageSimilarityTester;
    public PuzzleValidationController puzzleValidationController;
    #endregion

    #region 
    [Header("UI")]
    public GameObject masked;
    public Result_manager result_Manager;
    #endregion

    void Awake()
    {
        // Add data vào các wave_slot để biểu hiện trạng thái của wave đã pass hay chưa
        if (wave_grid != null)
        {
            wave_Slots = new Wave_slot[wave_grid.childCount];
            //Tìm danh sách con của grid
            for (int i = 0; i < wave_grid.childCount; i++)
            {
                wave_Slots[i] = wave_grid.GetChild(i).gameObject.GetComponent<Wave_slot>();

                if (i >= wave_datas.Length) continue;
                if (wave_Slots[i] != null && wave_datas[i] != null) wave_Slots[i].AddData(wave_datas[i]);
            }
        }
    }

    public void Replay()
    {
        StartWave(wave_index);
    }

    public void StartNextWave()
    {
        StartWave(wave_index + 1);
    }
    public void StartWave(int index)
    {
        if (index + 1 > wave_datas.Length) return;
        wave_index = index;

        if (wave_board != null) wave_board.SetActive(false);
        if (wave_screen != null) wave_screen.SetActive(true);
        if (result_Manager != null) result_Manager.DisableAll();

        //Reset all old data & Restore new data
        if (slot_Manage != null) slot_Manage.SpawnSlot(wave_datas[index].part_mini_count);

        if (part_main != null) Destroy(part_main);

        if (slot_Part_Interact != null)
        {
            slot_Part_Interact.AddPartMiniList(wave_datas[index].part_minis);
            slot_Part_Interact.RestoreAll();
        }

        GameObject part_m = Instantiate(
            wave_datas[index].part_main.partPrefab,
            part_main_spawnPoint.position,
            part_main_spawnPoint.rotation,
            part_main_spawnPoint
        );

        part_main = part_m;

        //Set up data cho winboard
        Board_win board_data = puzzleValidationController.validator.win_board.GetComponent<Board_win>();
        board_data.Init(wave_datas[wave_index]);

        //Set up ảnh mẫu validate
        imageSimilarityTester.AddTarget(wave_datas[index].image_hint);
        puzzleValidationController.ResetValidation();
        puzzleValidationController.validator.Win(false);
        
        ChangeState(WaveState.Start); ;
    }

    public void ChangeState(WaveState state)
    {
        waveState = state;

        if (waveState == WaveState.Win) PassWave();
    }

    public void PassWave()
    {
        //Chặn Input lên màn hình một khoảng delay
        if (masked != null) masked.SetActive(true);
        //Rung mainPart -> Ani xử lí spawn ảnh
        Part_main_ani part_Main_Ani = part_main.GetComponent<Part_main_ani>();
        if (part_Main_Ani != null) part_Main_Ani.PlayAnimate();
    }

    public void SpawnResultImage()
    {
        if (result_Manager != null) result_Manager.EnableIndex(wave_index);
    }
}
