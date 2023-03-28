using UnityEditor;
using UnityEngine;
using static Genesis.Editor.DepthSkyboxPrefabUtility;

namespace Genesis.Editor {

    public class GenesisImportPanoramaEditorWindow : EditorWindow {

        [MenuItem("Genesis/Import equirectangular panorama from disk", false, 0)]
        public static void ImportViaPath() {
            string[] filter = new string[] { "Image files", "png,jpg,jpeg", "All files", "*" };
            string path = EditorUtility.OpenFilePanelWithFilters("Select panorama", "", filter);

            if (string.IsNullOrEmpty(path)) {
                return;
            }

            CreateSkyboxAsset(path);
        }
    }
}