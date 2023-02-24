using System.IO;
using UnityEditor;
using UnityEngine;

public class ImportLeiaPixDepthEditorWindow : EditorWindow {

    /*[MenuItem("Genesis/Import depth from LeiaPix", false, 4)]
    public static void ImportViaPath() {
        string[] filter = new string[] { "Image files", "jpg", "All files", "*" };
        string path = EditorUtility.OpenFilePanelWithFilters("Select depth image", "", filter);

        if (string.IsNullOrEmpty(path)) {
            return;
        }
        Texture2D texture = new Texture2D(1, 1);
        byte[] data = File.ReadAllBytes(path);
        ImageConversion.LoadImage(texture, data);
        Texture2D depth = NormalizeDepth(texture);
        DestroyImmediate(texture);

        AssetDatabase.CreateAsset(depth, $"Assets/{Path.GetFileNameWithoutExtension(path)}.asset");
    }*/

    private static Texture2D NormalizeDepth(Texture2D tex) {
        Texture2D normalizedDepthTex = new Texture2D(tex.width, tex.height, TextureFormat.RFloat, mipChain: false, linear: true);
        normalizedDepthTex.wrapModeU = TextureWrapMode.Repeat;
        normalizedDepthTex.wrapModeV = TextureWrapMode.Clamp;
        var targetDepthData = normalizedDepthTex.GetPixelData<float>(0);
        var depthData = tex.GetPixelData<byte>(0);
        Debug.Log(targetDepthData.Length);
        Debug.Log(depthData.Length);

        for (int i = 0; i < depthData.Length; i += 3) {
            float depthValue = depthData[i] / 255f;
            targetDepthData[i / 3] = depthValue;
        }
        normalizedDepthTex.Apply();
        return normalizedDepthTex;
    }
}
