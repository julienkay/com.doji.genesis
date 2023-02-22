using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static Genesis.Editor.IOUtils;

namespace Genesis.Editor {

    public static class DepthSkyboxPrefabUtility {

        /// <summary>
        /// Imput given png fileb, generates depth and creates a prefab for a DepthSkybox.
        /// </summary>
        public static void CreateSkyboxAsset(string pngFilePath) {
            string name = Path.GetFileNameWithoutExtension(pngFilePath);
            string assetPath = Path.Combine(StagingAreaPath, $"{name}");
            Directory.CreateDirectory(assetPath);

            string pngTargetPath = Path.Combine(assetPath, $"{name}_rgb.png");
            Texture2D skybox = ImportImageFile(pngFilePath, pngTargetPath);
            GenerateDepth(assetPath, name, skybox);
            CreateSkyboxPrefab(assetPath, name);
        }

        /// <summary>
        /// Downloads skybox from Skybox Lab, generates depth and creates a prefab for a DepthSkybox.
        /// </summary>
        public static async Task CreateSkyboxAsset(string id, string name) {
            string assetPath = Path.Combine(StagingAreaPath, $"{name}");
            Directory.CreateDirectory(assetPath);

            string imageFile = Path.Combine(assetPath, $"{name}_rgb.png");
            Texture2D skybox = await DownloadSkyboxById(imageFile, id);
            GenerateDepth(assetPath, name, skybox);
            CreateSkyboxPrefab(assetPath, name);
        }

        private static void GenerateDepth(string assetPath, string name, Texture2D skybox) {
            int progressId = Progress.Start("Generating depth texture", "Generating a depth texture from skybox...");

            DepthEstimator depthEstimator = new DepthEstimator();
            depthEstimator.GenerateDepth(skybox);
            var depthTexture = depthEstimator.GetNormalizedDepth();

            string depthTextureFile = Path.Combine(assetPath, $"{name}_depth.asset");
            AssetDatabase.CreateAsset(depthTexture, depthTextureFile);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // for some reason, saving out as exr or png and reimporting produces 
            //byte[] bytes = depthTexture.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat);

            depthEstimator.Dispose();
            Progress.Remove(progressId);
        }

        public static void CreateSkyboxPrefab(string assetPath, string name) {
            GameObject skyboxPrefab = (GameObject)Resources.Load("Prefabs/DepthSkybox");

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(skyboxPrefab);
            MeshRenderer renderer = instance.GetComponent<MeshRenderer>();

            string skyboxPath = Path.Combine(assetPath, $"{name}_rgb.png");
            string depthPath = Path.Combine(assetPath, $"{name}_depth.asset");

            Texture2D skybox = (Texture2D)AssetDatabase.LoadMainAssetAtPath(skyboxPath);
            Texture2D skyboxDepth = (Texture2D)AssetDatabase.LoadMainAssetAtPath(depthPath);

            Material m = new Material(renderer.sharedMaterial);
            renderer.sharedMaterial = m;
            m.SetTexture("_MainTex", skybox);
            m.SetTexture("_Depth", skyboxDepth);

            string materialPath = Path.Combine(assetPath, $"{name}_material.mat");
            AssetDatabase.CreateAsset(m, materialPath);

            string prefabPath = Path.Combine(assetPath, $"{name}.prefab");
            GameObject variant = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);

            PrefabUtility.InstantiatePrefab(variant);
        }

        /// <summary>
        /// Downloads a skybox by its ID and saves it inside the project as a .png file.
        /// This method assumes that the generation has already completed and that the
        /// skybox is available on the server and returns null otherwise.
        /// </summary>
        private static async Task<Texture2D> DownloadSkyboxById(string path, string id) {
            int progressId = Progress.Start($"Downloading skybox {id}", "Your skybox is being downloaded...");

            // download the skybox 
            Texture2D skybox = await AssetForge.Instance.GetSkyboxById(id);
            if (skybox == null) {
                Progress.Remove(progressId);
                return null;
            }

            File.WriteAllBytes(path, skybox.EncodeToPNG());
            Object.DestroyImmediate(skybox);

            AssetDatabase.Refresh();
            Texture2D skyboxTextureAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            Progress.Remove(progressId);

            return skyboxTextureAsset;
        }

        private static Texture2D ImportImageFile(string pngFilePath, string pngTargetPath) {
            int progressId = Progress.Start($"Importing image", "Your panorama is being imported...");

            File.Copy(pngFilePath, pngTargetPath);

            AssetDatabase.Refresh();
            Texture2D skyboxTextureAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(pngTargetPath);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(pngTargetPath);
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            AssetDatabase.ImportAsset(pngTargetPath, ImportAssetOptions.ForceUpdate);

            Progress.Remove(progressId);

            return skyboxTextureAsset;
        }
    }
}