using UnityEngine;
using System.Collections;

public class Part_main_ani : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform visualTarget;

    [Header("Charge")]
    [Min(0.01f)] public float chargeDuration = 0.7f;
    public float chargeScale = 1.08f;

    [Header("Shake")]
    [Min(0.01f)] public float shakeDuration = 0.65f;
    public float shakeAngle = 7f;
    public float shakePosition = 0.035f;
    public float shakeFrequency = 25f;
    public float shakeScale = 1.15f;

    [Header("Burst")]
    [Min(0.01f)] public float burstDuration = 0.18f;
    public float burstScale = 1.35f;

    [Min(0.01f)] public float disappearDuration = 0.2f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private Wave_manager wave_Manager;
    private PuzzleValidator puzzleValidator;

    private void Awake()
    {
        if (wave_Manager == null) wave_Manager = Object.FindAnyObjectByType<Wave_manager>();
        if (puzzleValidator == null) puzzleValidator = Object.FindAnyObjectByType<PuzzleValidator>();

        if (visualTarget == null)
            visualTarget = transform;

        CacheTransform();
    }

    public void PlayAnimate()
    {
        StartCoroutine(PlayReveal());
    }

    private void CacheTransform()
    {
        originalPosition = visualTarget.localPosition;
        originalRotation = visualTarget.localRotation;
        originalScale = visualTarget.localScale;
    }

    public void ResetVisual()
    {
        visualTarget.localPosition = originalPosition;
        visualTarget.localRotation = originalRotation;
        visualTarget.localScale = originalScale;
    }

    public IEnumerator PlayReveal()
    {
        ResetVisual();

        // Phase 1: Charge
        float elapsed = 0f;

        while (elapsed < chargeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / chargeDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            visualTarget.localScale =
                originalScale * Mathf.Lerp(1f, chargeScale, eased);

            yield return null;
        }

        // Phase 2: Shake
        elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / shakeDuration);
            float strength = Mathf.SmoothStep(0f, 1f, t);

            // Giảm rung ở cuối để tránh giật khi chuyển phase
            float envelope = Mathf.Sin(t * Mathf.PI);
            float amount = strength * envelope;

            float wave = elapsed * shakeFrequency * Mathf.PI * 2f;

            float rotationZ =
                Mathf.Sin(wave) * shakeAngle * amount;

            float offsetX =
                Mathf.Sin(wave) * shakePosition * amount;

            float offsetY =
                Mathf.Cos(wave * 0.7f)
                * shakePosition * 0.5f * amount;

            visualTarget.localRotation =
                originalRotation * Quaternion.Euler(0f, 0f, rotationZ);

            visualTarget.localPosition =
                originalPosition + new Vector3(offsetX, offsetY, 0f);

            visualTarget.localScale =
                originalScale *
                Mathf.Lerp(chargeScale, shakeScale, strength);

            yield return null;
        }

        // Reset vị trí và rotation
        visualTarget.localPosition = originalPosition;
        visualTarget.localRotation = originalRotation;

        // Phase 3: Burst
        yield return AnimateScale(
            visualTarget.localScale,
            originalScale * burstScale,
            burstDuration
        );

        Vector3 default_dir = new Vector3(1f, 1f, 1f);
        // Phase 4: Disappear
        yield return AnimateScale(
            visualTarget.localScale,
            default_dir,
            disappearDuration
        );

        if (wave_Manager != null) wave_Manager.SpawnResultImage();

        yield return new WaitForSeconds(1f);
        if (puzzleValidator != null) puzzleValidator.Win(true);
    }

    private IEnumerator AnimateScale(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            visualTarget.localScale =
                Vector3.LerpUnclamped(from, to, t);

            yield return null;
        }

        visualTarget.localScale = to;
    }
}
