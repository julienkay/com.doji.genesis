using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static Genesis.Editor.IOUtils;

namespace Genesis.Editor {

    public static class DepthSkyboxPrefabUtility {

        /// <summary>
        /// Given an input image file, generates depth and creates a prefab for a DepthSkybox.
        /// </summary>
        public static void CreateSkyboxAsset(string filePath) {
            string name = Path.GetFileNameWithoutExtension(filePath);
            string assetPath = Path.Combine(StagingAreaPath, $"{name}");
            Directory.CreateDirectory(assetPath);

            string targetPath = Path.Combine(assetPath, $"{name}_rgb{Path.GetExtension(filePath)}");
            Texture2D skybox = ImportImageFile(filePath, targetPath);
            var range = GenerateDepth(assetPath, name, skybox);
            CreateSkyboxPrefab(skybox, assetPath, name, range);
        }

        private static Vector2 GenerateDepth(string assetPath, string name, Texture2D skybox) {
            int progressId = Progress.Start("Generating depth texture", "Generating a depth texture from skybox...");

            DepthEstimator depthEstimator = new DepthEstimator();
            depthEstimator.GenerateDepth(skybox);
            var depthTexture = depthEstimator.PostProcessDepth();

            string depthTextureFile = Path.Combine(assetPath, $"{name}_depth.asset");
            CreateAsset(depthTexture, depthTextureFile);

            // for some reason, saving out as exr or png and reimporting produces what looks like precision artifacts
            //byte[] bytes = depthTexture.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat);

            depthEstimator.Dispose();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Progress.Remove(progressId);

            return new Vector2(depthEstimator.MinDepth, depthEstimator.MaxDepth);
        }

        /// <summary>
        /// Creates the given asset and overwrites any existing asset with the same name
        /// </summary>
        private static void CreateAsset(Object obj, string path) {
            var existing = AssetDatabase.LoadAssetAtPath(path, obj.GetType());
            if (existing != null) {
                AssetDatabase.DeleteAsset(path);
            }

            AssetDatabase.CreateAsset(obj, path);
        }

        public static void CreateSkyboxPrefab(Texture2D skybox, string assetPath, string name, Vector2 range) {
            int progressId = Progress.Start("Creating skybox assets", "Your skybox assets are being created...");

            GameObject skyboxPrefab = (GameObject)Resources.Load("Prefabs/DepthSkybox");

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(skyboxPrefab);
            MeshRenderer renderer = instance.GetComponent<MeshRenderer>();

            string depthPath = Path.Combine(assetPath, $"{name}_depth.asset");
            Texture2D skyboxDepth = (Texture2D)AssetDatabase.LoadMainAssetAtPath(depthPath);

            Material m = new Material(renderer.sharedMaterial);
            renderer.sharedMaterial = m;
            m.SetTexture("_MainTex", skybox);
            m.SetTexture("_Depth", skyboxDepth);
            m.SetFloat("_Min", range.x);
            m.SetFloat("_Max", range.y);

            string materialPath = Path.Combine(assetPath, $"{name}_material.mat");
            CreateAsset(m, materialPath);

            string prefabPath = Path.Combine(assetPath, $"{name}.prefab");
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null) {
                AssetDatabase.DeleteAsset(prefabPath);
            }
            GameObject variant = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
            PrefabUtility.InstantiatePrefab(variant);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Progress.Remove(progressId);
        }

        private static Texture2D ImportImageFile(string filePath, string targetPath) {
            int progressId = Progress.Start($"Importing image", "Your panorama is being imported...");

            File.Copy(filePath, targetPath);

            AssetDatabase.Refresh();
            Texture2D skyboxTextureAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(targetPath);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(targetPath);
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);

            Progress.Remove(progressId);

            return skyboxTextureAsset;
        }
    }
}