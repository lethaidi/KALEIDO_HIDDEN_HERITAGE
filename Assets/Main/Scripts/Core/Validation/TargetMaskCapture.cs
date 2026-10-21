using System.IO;
using UnityEngine;

public class TargetMaskCapture : MonoBehaviour
{
    [SerializeField] private RenderTexture sourceTexture;

    [ContextMenu("Save Target Mask")]
    private void SaveTargetMask()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Hay bam Play truoc khi chup.");
            return;
        }

        if (sourceTexture == null || !sourceTexture.IsCreated())
        {
            Debug.LogError("Chua gan RenderTexture hoac texture chua san sang.");
            return;
        }

        // Ghi nho noi Unity dang ve de khoi phuc sau khi doc anh.
        RenderTexture previousTexture = RenderTexture.active;
        Texture2D capturedImage = null;

        try
        {
            RenderTexture.active = sourceTexture;

            // Tao anh co cung kich thuoc voi RenderTexture.
            capturedImage = new Texture2D(
                sourceTexture.width,
                sourceTexture.height,
                TextureFormat.RGB24,
                false
            );

            // Doc toan bo anh tu RenderTexture.
            capturedImage.ReadPixels(
                new Rect(0, 0, sourceTexture.width, sourceTexture.height),
                0,
                0,
                false
            );

            // Tao thu muc luu anh trong Assets.
            string folderPath = Path.Combine(
                Application.dataPath,
                "Main/Validation_targets"
            );

            Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(
                folderPath,
                "Wave01_Target.png"
            );

            // Chuyen thanh PNG va ghi ra file.
            byte[] pngData = capturedImage.EncodeToPNG();
            File.WriteAllBytes(filePath, pngData);

            // Bao Unity cap nhat file moi trong cua so Project.
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log("Da luu anh dap an: " + filePath);
        }
        finally
        {
            RenderTexture.active = previousTexture;

            if (capturedImage != null)
            {
                Destroy(capturedImage);
            }
        }
#endif
    }
}
