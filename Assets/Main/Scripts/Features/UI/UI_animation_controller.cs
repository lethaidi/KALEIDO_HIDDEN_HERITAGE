using UnityEngine;
using System.Collections;

public enum UIStartAction
{
    None,
    Show,
    Hide,
    ShowInstant,
    HideInstant,
    MoveToTarget,
    MoveBack
}

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class UI_Animation_Controller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform transformTarget;

    [Header("Start Action")]
    [SerializeField] private UIStartAction startAction =
        UIStartAction.Show;

    [Header("Popup Animation")]
    [Min(0f)] public float showDuration = 0.35f;
    [Min(0f)] public float hideDuration = 0.2f;

    [Range(0f, 1f)]
    public float hiddenScale = 0.65f;

    [Header("Popup Bounce")]
    [Range(1f, 1.5f)]
    public float popScale = 1.12f;

    [Range(0.01f, 1f)]
    public float popTime = 0.7f;

    [Header("Movement")]
    [Min(0f)] public float moveDuration = 0.5f;

    [SerializeField] private AnimationCurve moveCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Active Management")]
    [SerializeField] private bool deactivateAfterHide = true;

    private Vector3 originalPosition;
    private Vector3 originalScale;

    private Coroutine visibilityCoroutine;
    private Coroutine moveCoroutine;

    private bool initialized;
    private bool isHiding;
    private bool suppressNextOnEnable;

    // =========================
    // UNITY LIFECYCLE
    // =========================

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (initialized) return;

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        originalPosition = rectTransform.position;
        originalScale = rectTransform.localScale;

        initialized = true;
    }

    private void OnEnable()
    {
        Initialize();

        if (suppressNextOnEnable)
        {
            suppressNextOnEnable = false;
            return;
        }

        ExecuteStartAction();
    }

    private void OnDisable()
    {
        visibilityCoroutine = null;
        moveCoroutine = null;
        isHiding = false;
        suppressNextOnEnable = false;
    }

    // =========================
    // START ACTION
    // =========================

    private void ExecuteStartAction()
    {
        switch (startAction)
        {
            case UIStartAction.Show:
                PlayShow();
                break;

            case UIStartAction.Hide:
                PlayHide();
                break;

            case UIStartAction.ShowInstant:
                ShowInstant();
                break;

            case UIStartAction.HideInstant:
                HideInstant();
                break;

            case UIStartAction.MoveToTarget:
                MoveToTarget();
                break;

            case UIStartAction.MoveBack:
                MoveBack();
                break;
        }
    }

    // =========================
    // ACTIVE MANAGEMENT
    // =========================

    public void SetVisible(bool visible)
    {
        if (visible)
            PlayShow();
        else
            PlayHide();
    }

    public void Toggle()
    {
        bool shouldShow =
            !gameObject.activeSelf ||
            isHiding ||
            canvasGroup.alpha < 0.5f;

        SetVisible(shouldShow);
    }

    // =========================
    // POPUP SHOW
    // =========================

    public void PlayShow()
    {
        Initialize();

        // Nếu object đang tắt, bật lên.
        // OnEnable sẽ tự chạy animation Show.
        if (!gameObject.activeSelf)
        {
            suppressNextOnEnable = true;

            gameObject.SetActive(true);

            if (!gameObject.activeInHierarchy)
            {
                suppressNextOnEnable = false;
                return;
            }
        }

        if (!gameObject.activeInHierarchy)
            return;

        StopVisibilityAnimation();

        isHiding = false;

        // Luôn bắt đầu popup từ trạng thái ẩn
        rectTransform.localScale =
            originalScale * hiddenScale;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (showDuration <= 0f)
        {
            ShowInstant();
            return;
        }

        visibilityCoroutine =
            StartCoroutine(PopupShow());
    }

    private IEnumerator PopupShow()
    {
        float firstDuration =
            showDuration * popTime;

        float secondDuration =
            showDuration - firstDuration;

        Vector3 hiddenSize =
            originalScale * hiddenScale;

        Vector3 expandedSize =
            originalScale * popScale;

        float elapsed = 0f;

        // Phase 1: Popup phóng to
        while (elapsed < firstDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / firstDuration
            );

            t = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.localScale =
                Vector3.Lerp(
                    hiddenSize,
                    expandedSize,
                    t
                );

            canvasGroup.alpha = t;

            yield return null;
        }

        rectTransform.localScale = expandedSize;
        canvasGroup.alpha = 1f;

        // Phase 2: Bounce trở về kích thước gốc
        elapsed = 0f;

        while (elapsed < secondDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / secondDuration
            );

            t = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.localScale =
                Vector3.Lerp(
                    expandedSize,
                    originalScale,
                    t
                );

            yield return null;
        }

        rectTransform.localScale = originalScale;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        visibilityCoroutine = null;
    }

    // =========================
    // POPUP HIDE
    // =========================

    public void PlayHide()
    {
        Initialize();

        if (!gameObject.activeInHierarchy)
            return;

        StopVisibilityAnimation();

        isHiding = true;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (hideDuration <= 0f)
        {
            HideInstant();

            if (deactivateAfterHide)
                gameObject.SetActive(false);

            return;
        }

        visibilityCoroutine =
            StartCoroutine(PopupHide());
    }

    private IEnumerator PopupHide()
    {
        float elapsed = 0f;

        Vector3 startScale =
            rectTransform.localScale;

        Vector3 targetScale =
            originalScale * hiddenScale;

        float startAlpha = canvasGroup.alpha;

        while (elapsed < hideDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / hideDuration
            );

            t = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    t
                );

            canvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    0f,
                    t
                );

            yield return null;
        }

        rectTransform.localScale = targetScale;
        canvasGroup.alpha = 0f;

        visibilityCoroutine = null;
        isHiding = false;

        if (deactivateAfterHide)
            gameObject.SetActive(false);
    }

    // =========================
    // MOVEMENT
    // =========================

    public void MoveToTarget()
    {
        if (transformTarget == null)
        {
            Debug.LogWarning(
                "[UI Animation] Target is null!",
                this
            );
            return;
        }

        MoveTo(transformTarget);
    }

    public void MoveTo(RectTransform target)
    {
        Initialize();

        if (target == null ||
            !gameObject.activeInHierarchy)
            return;

        StopMoveAnimation();

        if (moveDuration <= 0f)
        {
            rectTransform.position = target.position;
            return;
        }

        moveCoroutine = StartCoroutine(
            AnimateMove(target.position)
        );
    }

    public void MoveBack()
    {
        Initialize();

        if (!gameObject.activeInHierarchy)
            return;

        StopMoveAnimation();

        if (moveDuration <= 0f)
        {
            rectTransform.position = originalPosition;
            return;
        }

        moveCoroutine = StartCoroutine(
            AnimateMove(originalPosition)
        );
    }

    private IEnumerator AnimateMove(Vector3 targetPosition)
    {
        Vector3 startPosition =
            rectTransform.position;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / moveDuration
            );

            float eased = moveCurve != null
                ? moveCurve.Evaluate(t)
                : Mathf.SmoothStep(0f, 1f, t);

            rectTransform.position =
                Vector3.LerpUnclamped(
                    startPosition,
                    targetPosition,
                    eased
                );

            yield return null;
        }

        rectTransform.position = targetPosition;
        moveCoroutine = null;
    }

    // =========================
    // INSTANT
    // =========================

    public void ShowInstant()
    {
        Initialize();

        if (!gameObject.activeSelf)
        {
            suppressNextOnEnable = true;
            gameObject.SetActive(true);
        }

        StopVisibilityAnimation();

        isHiding = false;

        rectTransform.localScale = originalScale;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void HideInstant()
    {
        Initialize();

        StopVisibilityAnimation();

        isHiding = false;

        rectTransform.localScale =
            originalScale * hiddenScale;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    // =========================
    // UTILITY
    // =========================

    public void SetTarget(RectTransform target)
    {
        transformTarget = target;
    }

    public void ResetPosition()
    {
        Initialize();

        StopMoveAnimation();

        rectTransform.position = originalPosition;
    }

    public void ResetAll()
    {
        Initialize();

        StopVisibilityAnimation();
        StopMoveAnimation();

        isHiding = false;

        rectTransform.position = originalPosition;
        rectTransform.localScale = originalScale;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void StopVisibilityAnimation()
    {
        if (visibilityCoroutine == null)
            return;

        StopCoroutine(visibilityCoroutine);
        visibilityCoroutine = null;
    }

    private void StopMoveAnimation()
    {
        if (moveCoroutine == null)
            return;

        StopCoroutine(moveCoroutine);
        moveCoroutine = null;
    }
}
