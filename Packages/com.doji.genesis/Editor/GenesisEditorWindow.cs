using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Profiling.Memory.Experimental;
using static Genesis.Editor.IOUtils;

namespace Genesis.Editor {

    public partial class GenesisEditorWindow : EditorWindow {
    
        [MenuItem("Genesis/ &g", true, 0)]
        protected static bool ValidateGenesisEditor() {
            return true;
        }

        [MenuItem("Genesis/Genesis Editor &g", false, 0)]
        public static void OpenEditorWindow() {
            var window = GetWindow<GenesisEditorWindow>(false, SR.DefaultWindowHeader, true);
            window.minSize = new Vector2(350f, 600f);
        }

        /*[MenuItem("Genesis/Import From Skybox Lab via ID", false, 0)]
        public static void ImportViaID() {

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

            string assetPath = Path.Combine(StagingAreaPath, $"{DateTime.Now:yyyy-MM-dd--HH-mm-ss}_{gen.Id}");
            Directory.CreateDirectory(assetPath);

            SkyboxAsset skybox = CreateSkyboxAsset(assetPath, gen.Id);

            // wait for result
            SkyboxImageResponse img = await AssetForge.Instance.WaitForSkyboxGeneration(gen.Id);

            Progress.Remove(progressId);

            if (img == null) {
                return;
            }

            // download skybox and save inside project ass png
            var textureAsset = await DownloadSkyboxById(assetPath, img.Id);

            // update Skybox asset metadata
            skybox.TextureAsset = textureAsset;
            skybox.Title = img.Title;
            skybox.FileUrl = img.FileUrl;
            skybox.ThumbUrl = img.ThumbUrl;
        }

        /// <summary>
        /// Creates a new object holding only the ID of the skybox to be generated so far.
        /// </summary>
        private SkyboxAsset CreateSkyboxAsset(string assetPath, string id) {
            SkyboxAsset skyboxAsset = CreateInstance<SkyboxAsset>();
            // path has to start at "Assets"
            string path = Path.Combine(assetPath, $"{id}.asset");
            AssetDatabase.CreateAsset(skyboxAsset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return skyboxAsset;
        }

        /// <summary>
        /// Downloads a skybox by its ID and saves it inside the project as a .png file.
        /// This method assumes that the generation has already completed and that the
        /// skybox is available on the server and returns null otherwise.
        /// </summary>
        private async Task<Texture2D> DownloadSkyboxById(string assetPath, string id) {
            int progressId = Progress.Start($"Downloading skybox{id}", "Your Skybox is being downloaded...");

            // download the skybox 
            Texture2D skybox = await AssetForge.Instance.GetSkyboxById(id);
            if (skybox == null) {
                Progress.Remove(progressId);
                return null;
            }

            string imageFile = Path.Combine(assetPath, $"{id}.png");
            File.WriteAllBytes(imageFile, skybox.EncodeToPNG());
            DestroyImmediate(skybox);

            //TODO: Modify Texture import settings (4k resolution, Trilinear filtering, ..)
            AssetDatabase.Refresh();
            Texture2D skyboxTextureAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(imageFile);

            Progress.Remove(progressId);

            return skyboxTextureAsset;
        }
    }
}