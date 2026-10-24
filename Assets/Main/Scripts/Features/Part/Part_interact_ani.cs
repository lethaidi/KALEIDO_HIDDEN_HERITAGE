using UnityEngine;
using System.Collections;

public class Part_interact_ani : MonoBehaviour
{
    [Header("Spawn Animation")]
    [SerializeField] private bool playOnAwake = true;

    [Range(0.1f, 1f)]
    public float spawnScale = 0.65f;

    [Range(1f, 1.3f)]
    public float popScale = 1.08f;

    [Min(0.01f)]
    public float spawnDuration = 0.3f;

    [Header("Hit Animation")]
    [Range(0.5f, 1f)]
    public float shrinkScale = 0.9f;

    [Min(0.01f)]
    public float shrinkDuration = 0.1f;

    [Min(0.01f)]
    public float returnDuration = 0.2f;

    private Vector3 originalScale;
    private Coroutine currentAnimation;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Start()
    {
        if (playOnAwake)
            PlaySpawnAnimate();
    }

    private void OnDisable()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }

        transform.localScale = originalScale;
    }

    // Giữ tương thích với code cũ
    public void PlayAnimate()
    {
        PlayHitAnimate();
    }

    public void PlaySpawnAnimate()
    {
        StopCurrentAnimation();

        transform.localScale = originalScale * spawnScale;

        currentAnimation = StartCoroutine(
            AnimateScale(
                originalScale * spawnScale,
                originalScale,
                spawnDuration,
                true
            )
        );
    }

    public void PlayHitAnimate()
    {
        StopCurrentAnimation();

        currentAnimation = StartCoroutine(HitSequence());
    }

    private IEnumerator HitSequence()
    {
        yield return ScaleTo(
            originalScale * shrinkScale,
            shrinkDuration
        );

        yield return ScaleTo(
            originalScale,
            returnDuration
        );

        currentAnimation = null;
    }

    private IEnumerator ScaleTo(Vector3 target, float duration)
    {
        Vector3 start = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale = Vector3.LerpUnclamped(
                start, target, t
            );

            yield return null;
        }

        transform.localScale = target;
    }

    private IEnumerator AnimateScale(
        Vector3 start,
        Vector3 target,
        float duration,
        bool overshoot)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            if (overshoot)
            {
                // Back Out easing: pop vượt nhẹ rồi ổn định
                float c1 = 1.2f;
                float c3 = c1 + 1f;

                float p = t - 1f;

                t = 1f + c3 * p * p * p
                       + c1 * p * p;
            }
            else
            {
                t = Mathf.SmoothStep(0f, 1f, t);
            }

            transform.localScale = Vector3.LerpUnclamped(
                start, target, t
            );

            yield return null;
        }

        transform.localScale = target;
        currentAnimation = null;
    }

    private void StopCurrentAnimation()
    {
        if (currentAnimation == null)
            return;

        StopCoroutine(currentAnimation);
        currentAnimation = null;
    }
}
