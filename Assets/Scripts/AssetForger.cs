using System;
using System.Collections;
using UnityEngine;
using static AssetForger.Utilities.WebRequestUtility;
using static AssetForger.Utilities.UrlUtility;
using Newtonsoft.Json;
using static Doji.TwitchStreaming.WebRequestAsyncUtility;
using System.Threading.Tasks;

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
        private static readonly string GET_SKYBOX_URL = BASE_URL + "getSkyboxImage";

        //https://assetforger.blockadelabs.com/api/skybox/generateSkyboxImage
        //    https://assetforger.blockadelabs.com/api/skybox/getSkyboxImage?id=8fc0d4bc7608324e30fbab6c452ef742

        public void GenerateSkybox(SkyboxPrompt prompt, Action<SkyboxGenerationResponse> OnGamesReceived, Action<string> OnError = null) {
            string promptJSON = JsonConvert.SerializeObject(prompt.CreatePromptString());
            Debug.Log(promptJSON);
            StartCoroutine(HttpRequest(GENERATE_SKYBOX_URL, HTTPVerb.Post, OnGamesReceived, OnError, promptJSON));
        }
        public void GetSkyboxImage(string id, Action<SkyboxImageResponse> OnGamesReceived, Action<string> OnError = null) {
            string url = GET_SKYBOX_URL + "?";
            AppendQuerytoUrl(ref url, "id", id);
            StartCoroutine(HttpRequest(url, HTTPVerb.Post, OnGamesReceived, OnError));
        }

        /// <summary>
        /// Starts a UnityWebrequest configured for HTTP GET.
        /// </summary>
        /// <typeparam name="T">the type of data that is expected as a response</typeparam>
        /// <param name="url">the url to request the data from</param>
        /// <param name="onResponseReceived">the action to execute when the data has arrived</param>
        /// <returns></returns>
        private IEnumerator HttpRequest<T>(string url, HTTPVerb verb, Action<T> onResponseReceived, Action<string> onError, string postData = "", params Tuple<string, string>[] requestHeaders) {
            WebRequest<T> webRequest = new WebRequest<T>(this, url, verb, postData, requestHeaders);
            yield return webRequest.Coroutine;

            if (webRequest.Error != null) {
                onError.Invoke(webRequest.Error);
            } else {
                onResponseReceived.Invoke(webRequest.Result);
            }
        }
        private async Task<T> HttpRequestAsync<T>(string url, HTTPVerb verb, string postData = null, params Tuple<string, string>[] requestHeaders) {
            T result = await WebRequestAsync<T>.SendWebRequestAsync(url, verb, postData, requestHeaders);
            return result;
        }
    }
}