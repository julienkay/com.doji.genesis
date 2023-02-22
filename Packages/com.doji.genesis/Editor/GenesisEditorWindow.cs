using System.IO;
using UnityEditor;
using UnityEngine;
using static Genesis.Editor.IOUtils;
using static Genesis.Editor.DepthSkyboxPrefabUtility;
using System.Linq;

namespace Genesis.Editor {

    public partial class GenesisEditorWindow : EditorWindow {
    
        /*[MenuItem("Genesis/Genesis Editor &g", true, 0)]
        protected static bool ValidateGenesisEditor() {
            return true;
        }

        [MenuItem("Genesis/Genesis Editor &g", false, 0)]
        public static void OpenEditorWindow() {
            var window = GetWindow<GenesisEditorWindow>(false, SR.DefaultWindowHeader, true);
            window.minSize = new Vector2(350f, 600f);
        }*/

        private string _prompt;
        private Vector2 _promptScrollPos = Vector2.zero;
        private PromptType _promptType;

        private void OnGUI() {
            GUI.SetNextControlName(string.Empty);

            EditorGUILayout.BeginVertical(new GUIStyle() { padding = new RectOffset(10, 10, 10, 10) });


            DrawPromptField();
            DrawButtons();

            DrawSkyboxLibrary();

            GUILayout.EndVertical();
        }

        private void DrawSkyboxLibrary() { }

        private void DrawPromptField() {
            EditorGUILayout.PrefixLabel("Prompt");
            _promptScrollPos = EditorGUILayout.BeginScrollView(_promptScrollPos, GUILayout.Height(100));

            _prompt = GUILayout.TextArea(_prompt, Styles.PromptTextArea, GUILayout.ExpandHeight(true));

            EditorGUILayout.EndScrollView();
        }

        private void DrawButtons() {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawGenerateButton();
            DrawPromptOptions();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawGenerateButton() {
            // generate from prompt
            if (GUILayout.Button(SR.GenerateButton, GUILayout.Width(150))) {
                GenerateSkybox(new SkyboxPrompt(_prompt, _promptType));
            }
        }

        private void DrawPromptOptions() {
            _promptType = (PromptType)EditorGUILayout.EnumPopup(_promptType, GUILayout.Width(130));
        }

        public async void GenerateSkybox(SkyboxPrompt prompt) {
            int progressId = Progress.Start($"Generating skybox", "Your skybox is currently being generated...");

            // kick off generation
            SkyboxGenerationResponse gen = await AssetForge.Instance.RequestSkybox(prompt);
            if (gen == null) {
                return;
            }

#if UNITY_EDITOR
            Debug.Log($"Generating skybox {gen.Id}");
#endif

            // wait for result
            SkyboxImageResponse img = await AssetForge.Instance.WaitForSkyboxGeneration(gen.Id);

            Progress.Remove(progressId);

            if (img == null) {
                return;
            }
            string name = GetSafeFileName(img.Title);
            await CreateSkyboxAsset(gen.Id, name);
        }

        private string GetSafeFileName(string name) {
           char[] invalidFileNameChars = Path.GetInvalidFileNameChars();

            // Builds a string out of valid chars
            return new string(name.Where(ch => !invalidFileNameChars.Contains(ch)).ToArray());
        }
    }
}