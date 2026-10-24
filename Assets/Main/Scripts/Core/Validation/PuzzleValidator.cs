using UnityEngine;

public class PuzzleValidator : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)]
    private float similarityThreshold = 0.9f;

    [SerializeField] private bool isInteracting;
    [SerializeField] private bool isCompleted;

    // Script khác được đọc dùng để quyết định chiến thắng.
    public bool IsCompleted => isCompleted;

    public bool CanValidate =>
    Application.isPlaying &&
    isActiveAndEnabled &&
    !isCompleted &&
    !isInteracting;

    [Header("UI")]
    public GameObject win_board;

    public Wave_manager wave_Manager;

    void OnEnable()
    {
        if (win_board != null) Win(false);
    }

    public void Win(bool active)
    {
        if (win_board == null) return;
        win_board.SetActive(active);

        Board_win board_data = win_board.GetComponent<Board_win>();
        if (board_data.wave_data == null) return;
        board_data.wave_data.isCompleted = true;
    }

    public void Evaluate(float similarity)
    {
        Debug.Log($"Evaluate nhận điểm: {similarity}, đối tượng: {gameObject.name}");

        if (!Application.isPlaying || !isActiveAndEnabled)
            return;

        if (isCompleted || isInteracting)
            return;

        if (similarity >= similarityThreshold)
        {
            isCompleted = true;
            Debug.Log("WIN");
            
            if (wave_Manager != null) wave_Manager.ChangeState(WaveState.Win);
        }
    }

    public void SetInteractionActive(bool value)
    {
        isInteracting = value;
    }

    [ContextMenu("Reset Validation")]
    public void ResetValidation()
    {
        if (!Application.isPlaying)
            return;

        isCompleted = false;
        isInteracting = false;
    }
}