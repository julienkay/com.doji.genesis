using System;
using System.Collections;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading.Tasks;
using AssetForger.Utilities;
using static AssetForger.Utilities.UrlUtility;
using static AssetForger.Utilities.WebRequestAsyncUtility;
using UnityEngine.Networking;

namespace AssetForger {
    public class AssetForge : MonoBehaviour {

        public static AssetForge Instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<AssetForge>();
                }
                return _instance;
            }
            set { _instance = value; }
        }
        private static AssetForge _instance;

        private static readonly string BASE_URL = "https://assetforger.blockadelabs.com/api/skybox/";

        private static readonly string GENERATE_SKYBOX_URL = BASE_URL + "generateSkyboxImage";
        private static readonly string GET_SKYBOX_URL      = BASE_URL + "getSkyboxImage";

        public async Task<Texture2D> GenerateSkybox(SkyboxPrompt prompt) {
            // send generation POST request
            var gen = await GenerateSkyboxImage(prompt);

            if (gen.Error != null) {
                return null;
            }

            // poll until we get a HTTP 200 for the generated skybox id
            bool finishedGeneration;
            Result<SkyboxImageResponse> img;
            do {
                await Task.Delay(5000);
                img = await GetSkyboxImage(gen.Value.Id);
                finishedGeneration = img.Error != null;
            } while (!finishedGeneration);

            // download the associated image

            Uri url = img.Value.FileUrl;
            Texture2D skybox = await DownloadImage(url);
            return skybox;
        }

        public async Task<Texture2D> GetSkyboxById(string id) {
            var img = await GetSkyboxImage(id);
            if (img.Error != null) {
                return null;
            }

            Uri url = img.Value.FileUrl;
            Texture2D skybox = await DownloadImage(url);
            return skybox;
        }

        private async Task<Result<SkyboxGenerationResponse>> GenerateSkyboxImage(SkyboxPrompt prompt) {
            string promptJSON = JsonConvert.SerializeObject(prompt.CreatePromptString());
            Debug.Log(promptJSON);
            return await HttpRequestAsync<SkyboxGenerationResponse>(GENERATE_SKYBOX_URL, HTTPVerb.POST, promptJSON);
        }
        private async Task<Result<SkyboxImageResponse>> GetSkyboxImage(string id) {
            string url = GET_SKYBOX_URL + "?";
            AppendQuerytoUrl(ref url, "id", id);
            return await HttpRequestAsync<SkyboxImageResponse>(url, HTTPVerb.GET);
        }

        private async Task<Result<T>> HttpRequestAsync<T>(string url, HTTPVerb verb, string postData = null, params Tuple<string, string>[] requestHeaders) {
            Result<T> result = await WebRequestAsync<T>.SendWebRequestAsync(url, verb, postData, requestHeaders);
            return result;
        }

        internal async Task<Texture2D> DownloadImage(Uri url) {
            UnityWebRequest wr = null;
            try {
                wr = UnityWebRequestTexture.GetTexture(url, false);

                var asyncOp = wr.SendWebRequest();
                while (!asyncOp.webRequest.isDone) {
                    await Task.Yield();
                }
                return DownloadHandlerTexture.GetContent(asyncOp.webRequest);

            } catch (Exception e) {
                Debug.LogError(e);
                wr.Dispose();
            }

            return null;
        }
    }
}