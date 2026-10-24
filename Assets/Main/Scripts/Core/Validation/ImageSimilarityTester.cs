using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class ImageSimilarityTester : MonoBehaviour
{
    [SerializeField] private RenderTexture currentMask;
    [SerializeField] public Texture2D targetMask;
    [SerializeField] private RawImage Image_hint;
    [SerializeField] private UnityEvent<float> onSimilarityCalculated = new UnityEvent<float>();

    [Header("UI")]
    public TextMeshProUGUI similar_show;

    void LateUpdate()
    {
        if (targetMask != null)
            Image_hint.texture = targetMask;
    }

    public void AddTarget(Texture2D mask)
    {
        targetMask = mask;
        StartCoroutine(CheckSimilarDelay());
    }

    private IEnumerator CheckSimilarDelay()
    {
        yield return new WaitForSeconds(0.5f);

        CheckSimilarity();
    }

    [ContextMenu("Check Similarity")]
    public void CheckSimilarity()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Hay bam Play truoc khi kiem tra.");
            return;
        }

        if (currentMask == null || targetMask == null)
        {
            Debug.LogError("Chua gan Current Mask hoac Target Mask.");
            return;
        }

        if (!currentMask.IsCreated())
        {
            Debug.LogError("RenderTexture chua san sang.");
            return;
        }

        if (!targetMask.isReadable)
        {
            Debug.LogError("Hay bat Read/Write cho anh Target Mask.");
            return;
        }

        if (currentMask.width != targetMask.width ||
            currentMask.height != targetMask.height)
        {
            Debug.LogError("Hai anh phai co cung chieu rong va chieu cao.");
            return;
        }

        RenderTexture previousTexture = RenderTexture.active;
        Texture2D currentImage = null;

        try
        {
            // Doc anh hien tai tu RenderTexture.
            RenderTexture.active = currentMask;

            currentImage = new Texture2D(
                currentMask.width,
                currentMask.height,
                TextureFormat.RGB24,
                false
            );

            currentImage.ReadPixels(
                new Rect(0, 0, currentMask.width, currentMask.height),
                0,
                0,
                false
            );

            // Lay danh sach diem anh cua hai anh.
            Color32[] currentPixels = currentImage.GetPixels32();
            Color32[] targetPixels = targetMask.GetPixels32();

            int intersection = 0;
            int union = 0;

            for (int i = 0; i < currentPixels.Length; i++)
            {
                // Mask trang-den: dung kenh do de phan loai pixel.
                bool currentIsWhite = currentPixels[i].r >= 20;
                bool targetIsWhite = targetPixels[i].r >= 20;

                if (currentIsWhite && targetIsWhite)
                {
                    intersection++;
                }

                if (currentIsWhite || targetIsWhite)
                {
                    union++;
                }
            }

            if (union == 0)
            {
                Debug.LogWarning("Ca hai mask deu rong. Khong hop le.");
                return;
            }

            float similarity = (float)intersection / union;

            Debug.Log(
                $"IoU: {similarity * 100f:F2}% | " +
                $"Giao: {intersection} | Hop: {union}"
            );

            double value_show = Math.Round(similarity * 100f, 2);
            similar_show.text = value_show.ToString() + "%";

            onSimilarityCalculated.Invoke(similarity);

            Debug.Log("Test");
        }
        finally
        {
            RenderTexture.active = previousTexture;

            if (currentImage != null)
            {
                Destroy(currentImage);
            }
        }
    }
}