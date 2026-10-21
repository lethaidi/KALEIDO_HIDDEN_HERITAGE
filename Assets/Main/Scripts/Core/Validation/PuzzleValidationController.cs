using System.Collections;
using UnityEngine;

public class PuzzleValidationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ImageSimilarityTester similarityTester;
    public PuzzleValidator validator;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float settleDelay = 0.3f;

    private Coroutine pendingValidation;

    private bool IsReady =>
        Application.isPlaying &&
        isActiveAndEnabled &&
        similarityTester != null &&
        similarityTester.isActiveAndEnabled &&
        validator != null;

    // Goi khi bat dau keo, xoay hoac chay animation di chuyen part.
    [ContextMenu("Test/Begin Interaction")]
    public void BeginInteraction()
    {
        if (!IsReady)
            return;

        CancelPendingValidation();
        validator.SetInteractionActive(true);
    }

    // Goi khi thao tac va chuyen dong lien quan da ket thuc.
    [ContextMenu("Test/End Interaction")]
    public void EndInteraction()
    {
        if (!IsReady)
            return;

        validator.SetInteractionActive(false);
        RequestValidation();
    }

    // He thong snap se goi ham nay khi trang thai du part thay doi.
    public void SetRequiredPartsSnapped(bool value)
    {
        if (!IsReady)
            return;

        RequestValidation();
    }

    // Huy lich cu va tao lich moi neu du dieu kien.
    [ContextMenu("Test/Request Validation")]
    public void RequestValidation()
    {
        CancelPendingValidation();

        if (!IsReady || !validator.CanValidate)
            return;

        pendingValidation = StartCoroutine(ValidateAfterDelay());
    }

    private IEnumerator ValidateAfterDelay()
    {
        // Cho mot khoang ngan de puzzle on dinh.
        yield return new WaitForSeconds(settleDelay);

        if (!IsReady || !validator.CanValidate)
        {
            pendingValidation = null;
            yield break;
        }

        // Cho camera ve xong truoc khi doc RenderTexture.
        yield return new WaitForEndOfFrame();

        pendingValidation = null;

        // Kiem tra lai vi trang thai co the da thay doi trong luc cho.
        if (!IsReady || !validator.CanValidate)
            yield break;

        similarityTester.CheckSimilarity();
    }

    private void CancelPendingValidation()
    {
        if (pendingValidation == null)
            return;

        StopCoroutine(pendingValidation);
        pendingValidation = null;
    }

    [ContextMenu("Reset Validation Controller")]
    public void ResetValidation()
    {
        CancelPendingValidation();

        if (Application.isPlaying && validator != null)
        {
            validator.ResetValidation();
        }
    }

    private void OnDisable()
    {
        CancelPendingValidation();
    }

    // Hai lenh tam thoi de thu khi chua noi he thong snap.
    [ContextMenu("Test/Mark All Parts Snapped")]
    private void TestAllPartsSnapped()
    {
        SetRequiredPartsSnapped(true);
    }

    [ContextMenu("Test/Mark Parts Missing")]
    private void TestPartsMissing()
    {
        SetRequiredPartsSnapped(false);
    }
}