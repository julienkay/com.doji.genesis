using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static Genesis.Editor.IOUtils;
using static Genesis.Editor.DepthSkyboxPrefabUtility;

namespace Genesis.Editor {

    public class GenesisImportByIDEditorWindow : EditorWindow {

        [MenuItem("Genesis/Import from Skybox Lab via ID", false, 0)]
        public static void ImportViaID() {
            var window = GetWindow<GenesisImportByIDEditorWindow>(false, "Import by ID", true);
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
            window.ShowPopup();
        }


        [MenuItem("Genesis/Import equirectangular panorama from disk", false, 0)]
        public static void ImportViaPath() {
            string path = EditorUtility.OpenFilePanel("Select panorama", "", "png");

            if (string.IsNullOrEmpty(path)) {
                return;
            }

            byte[] data = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(1, 1);

            ImageConversion.LoadImage(tex, data);

            if (tex.width != tex.height * 2) {
                EditorUtility.DisplayDialog("Unsupported format", "This asset requires a panoramic image in equirectangular projection with an aspect ratio of 2:1.", "OK");
                return;
            }

            throw new System.NotImplementedException();
        }

        private string _id;
        private string Name {
            get {
                if (string.IsNullOrEmpty(_name)) {
                    return _id;
                } else {
                    return _name;
                }
            }
        }
        private string _name;

        void OnGUI() {
            EditorGUILayout.LabelField("Enter ID here: ", GUILayout.Width(80));
            _id = EditorGUILayout.TextField( _id);
            EditorGUILayout.LabelField("Asset name (Optional): ", GUILayout.Width(160));
            _name = EditorGUILayout.TextField(_name);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Import", GUILayout.Width(80))) {
                ImportSkyboxAssetByID(_id);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private async void ImportSkyboxAssetByID(string id) {
            if (string.IsNullOrEmpty(id)) {
                EditorUtility.DisplayDialog("Invalid ID", "The ID can't be empty", "OK");
                return;
            }

            if (!GUID.TryParse(id, out var _)) {
                EditorUtility.DisplayDialog("Invalid ID", "This does not seem to be a valid ID", "OK");
                return;
            }

            await CreateSkyboxAsset(id, Name);
        }
    }
}