using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Genesis.Utilities;
using static Genesis.Utilities.UrlUtility;
using static Genesis.Utilities.WebRequestAsyncUtility;
using UnityEngine.Networking;

namespace Genesis {

    /// <summary>
    /// An interface to Skybox Lab from Blockade Labs.
    /// </summary>
    public class AssetForge : MonoBehaviour {

        public static AssetForge Instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<AssetForge>();
#if UNITY_EDITOR
                    if (_instance == null) {
                        GameObject g = new GameObject();
                        g.name = nameof(AssetForge);
                        _instance = g.AddComponent<AssetForge>();
                    }
#endif
                }
                return _instance;
            }
            set { _instance = value; }
        }
        private static AssetForge _instance;

        private static readonly string BASE_URL = "https://assetforger.blockadelabs.com/api/skybox/";

        private static readonly string GENERATE_SKYBOX_URL = BASE_URL + "generateSkyboxImage";
        private static readonly string GET_SKYBOX_URL      = BASE_URL + "getSkyboxImage";

        /// <summary>
        /// Generates a skybox for the given prompt and returns the downloaded
        /// texture object.
        /// </summary>
        public async Task<Texture2D> GenerateSkybox(SkyboxPrompt prompt) {
            // kick off generation
            SkyboxGenerationResponse gen = await RequestSkybox(prompt);
            if (gen == null) {
                return null;
            }

            // wait for result
            SkyboxImageResponse img = await WaitForSkyboxGeneration(gen.Id);
            if (img == null) {
                return null;
            }

            // download the associated image
            Texture2D skybox = await DownloadImage(img.FileUrl);
            return skybox;
        }

        /// <summary>
        /// Generates the skybox, and returns with the metadata, once generation has finished.
        /// </summary>
        internal async Task<SkyboxGenerationResponse> RequestSkybox(SkyboxPrompt prompt) {
            // send generation POST request
            var gen = await GenerateSkyboxImage(prompt);

            if (gen.Error != null) {
                return null;
            }

            return gen.Value;
        }

        /// <summary>
        /// Returns once the skybox for the given id has been generated
        /// </summary>
        /// <returns></returns>
        internal async Task<SkyboxImageResponse> WaitForSkyboxGeneration(string id) {
            // poll until we get a HTTP 200 for the generated skybox id
            bool finishedGeneration;
            Result<SkyboxImageResponse> img;
            int retry = 10;
            do {
                await Task.Delay(5000);
                img = await GetSkyboxImage(id);

                // as long as we get a 304 (Not Modified) response, the generation is still in progress
                if (img.ReponseCode == 304 ||
                    img.Value.Status == "processing") {
                    finishedGeneration = false;

                    //timeout
                    retry--;
                    if (retry <= 0) {
                        Debug.LogError("Skybox generation did not return a result in time.");
                        break;
                    }
                } else {
                    finishedGeneration = true;
                }

            } while (!finishedGeneration);

            // anything other than "complete" is considered an error
            if (img.Error != null || img.Value.Status != "complete") {
                Debug.LogError($"({img.ReponseCode}) - Skybox generation not successful.");
                return null;
            } else {
                return img.Value;
            }
        }

        internal async Task<Texture2D> GetSkyboxById(string id) {
            var img = await GetSkyboxImage(id);
            if (img.Error != null) {
                return null;
            }

            Uri url = img.Value.FileUrl;
            Texture2D skybox = await DownloadImage(url);
            return skybox;
        }

        /// <summary>
        /// Sends a HTTP POST request to generate a skybox to the API.
        /// </summary>
        private async Task<Result<SkyboxGenerationResponse>> GenerateSkyboxImage(SkyboxPrompt prompt) {
            string promptJSON = JsonConvert.SerializeObject(prompt, new SkyboxPrompt.PromptConverter());
            return await HttpRequestAsync<SkyboxGenerationResponse>(GENERATE_SKYBOX_URL, HTTPVerb.POST, promptJSON);
        }

        /// <summary>
        /// Sends a HTTP GET request to retrieve a skybox by ID.
        /// </summary>
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
            try {
                using (UnityWebRequest wr = UnityWebRequestTexture.GetTexture(url, false)) {
                    var asyncOp = wr.SendWebRequest();
                    while (!asyncOp.webRequest.isDone) {
                        await Task.Yield();
                    }
                    return DownloadHandlerTexture.GetContent(asyncOp.webRequest);
                }
            } catch (Exception e) {
                Debug.LogError(e);
            }

            return null;
        }
    }
}