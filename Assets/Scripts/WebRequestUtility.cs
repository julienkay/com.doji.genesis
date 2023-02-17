using System.Collections;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System;

namespace AssetForger.Utilities {
    public enum HTTPVerb { Get, Post };

    /// <summary>
    /// TODO: Parsing ConnectionError, ProtocolError & DataProcessingError must be improved.
    /// </summary>
    public static class WebRequestUtility {

        /// <summary>
        /// This class performs a WebRequest with the given URL. The response is expected to be a JSON serialized
        /// object of type <typeparamref name="T"/> and is automatically being deserialized and returned.
        /// </summary>
        /// <typeparam name="T">the type that this web request returns.</typeparam>
        public class WebRequest<T> {

            public Coroutine Coroutine { get; private set; }

            /// <summary>
            /// Contains the data that was retrieved by this WebRequest.
            /// </summary>
            public T Result;

            /// <summary>
            /// Contains information on the error in case the request failed.
            /// Null otherwise.
            /// </summary>
            public string Error;
            
            private IEnumerator _target;

            public WebRequest(MonoBehaviour owner, string url, HTTPVerb verb = HTTPVerb.Get, string payload = "", params Tuple<string, string>[] requestHeaders) {
                _target = CreateRequest(url, verb, payload, requestHeaders);
                Coroutine = owner.StartCoroutine(Run());
            }

            private IEnumerator Run() {
                while (_target.MoveNext()) {
                    if (_target.Current.GetType().Equals(typeof(UnityWebRequest))) {
                        UnityWebRequest webRequest = (UnityWebRequest)_target.Current;

                        switch (webRequest.result) {
                            case UnityWebRequest.Result.InProgress:
                                yield return _target.Current;
                                break;
                            case UnityWebRequest.Result.Success:
#if UNITY_EDITOR
                                Debug.Log(webRequest.url + "\n" + webRequest.downloadHandler.text);
#endif
                                Result = JsonConvert.DeserializeObject<T>(webRequest.downloadHandler.text);
                                break;
                            case UnityWebRequest.Result.ConnectionError:
                            case UnityWebRequest.Result.ProtocolError:
                            case UnityWebRequest.Result.DataProcessingError:
                                Debug.LogError($"{webRequest.error}\nURL: {webRequest.url}\nJSON: {webRequest.downloadHandler.text}");
                                Error = webRequest.downloadHandler.text;
                                break;
                            default:
                                break;
                        }

                        yield return Result;
                    } else {
                        yield return _target.Current; 
                    }
                }
            }
        }

        /// <summary>
        /// Simple WebRequest without JSON parsing. Returns plain text.
        /// </summary>
        public class WebRequestSimple {
            public Coroutine Coroutine { get; private set; }

            public string Result;

            /// <summary>
            /// Contains information on the error in case the request failed.
            /// Null otherwise.
            /// </summary>
            public string Error;

            private IEnumerator _target;

            public WebRequestSimple(MonoBehaviour owner, string url, HTTPVerb verb = HTTPVerb.Get, string payload = "", params Tuple<string, string>[] requestHeaders) {
                _target = CreateRequest(url, verb, payload, requestHeaders);
                Coroutine = owner.StartCoroutine(Run());
            }

            private IEnumerator Run() {
                while (_target.MoveNext()) {
                    if (_target.Current.GetType().Equals(typeof(UnityWebRequest))) {
                        UnityWebRequest webRequest = (UnityWebRequest)_target.Current;

                        switch (webRequest.result) {
                            case UnityWebRequest.Result.InProgress:
                                yield return _target.Current;
                                break;
                            case UnityWebRequest.Result.Success:
                                Result = webRequest.downloadHandler.text;
                                break;
                            case UnityWebRequest.Result.ConnectionError:
                            case UnityWebRequest.Result.ProtocolError:
                            case UnityWebRequest.Result.DataProcessingError:
                                Debug.LogError($"{webRequest.result}: {webRequest.error}\nURL: {webRequest.url}");
                                Error = webRequest.downloadHandler.text;
                                break;
                            default:
                                break;
                        }

                        yield return Result;
                    } else {
                        yield return _target.Current;
                    }
                }
            }
        }

        /// <summary>
        /// WebRequest for binary data.
        /// </summary>
        public class WebRequestBinary {
            public Coroutine Coroutine { get; private set; }

            /// <summary>
            /// Contains the downloaded byte array that was retrieved by this WebRequest.
            /// </summary>
            public byte[] Result;

            /// <summary>
            /// Contains information on the error in case the request failed.
            /// Null otherwise.
            /// </summary>
            public string Error;

            private IEnumerator _target;

            public WebRequestBinary(MonoBehaviour owner, string url, HTTPVerb verb = HTTPVerb.Get, string payload = "", params Tuple<string, string>[] requestHeaders) {
                _target = CreateRequest(url, verb, payload, requestHeaders);
                Coroutine = owner.StartCoroutine(Run());
            }

            private IEnumerator Run() {
                while (_target.MoveNext()) {
                    if (_target.Current.GetType().Equals(typeof(UnityWebRequest))) {
                        UnityWebRequest webRequest = (UnityWebRequest)_target.Current;

                        switch (webRequest.result) {
                            case UnityWebRequest.Result.InProgress:
                                yield return _target.Current;
                                break;
                            case UnityWebRequest.Result.Success:
                                Result = webRequest.downloadHandler.data;
                                break;
                            case UnityWebRequest.Result.ConnectionError:
                            case UnityWebRequest.Result.ProtocolError:
                            case UnityWebRequest.Result.DataProcessingError:
                                Debug.LogError($"{webRequest.result}: {webRequest.error}\nURL: {webRequest.url}");
                                Error = webRequest.downloadHandler.text;
                                break;
                            default:
                                break;
                        }

                        yield return Result;
                    } else {
                        yield return _target.Current;
                    }
                }
            }
        }

        private static IEnumerator CreateRequest(string url, HTTPVerb verb, string postData = "", params Tuple<string, string>[] requestHeaders) {
            UnityWebRequest webRequest;

            switch (verb) {
                case HTTPVerb.Get:
                    webRequest = UnityWebRequest.Get(url);
                    break;
                case HTTPVerb.Post:
                    webRequest = UnityWebRequest.Post(url, postData);
                    break;
                default:
                    Debug.LogError("Invalid HTTP request method.");
                    yield break;
            }

            // set optional headers
            if (requestHeaders != null) {
                foreach(var header in requestHeaders) {
                    webRequest.SetRequestHeader(header.Item1, header.Item2);
                }
            }

            yield return webRequest.SendWebRequest();
            yield return webRequest;
        }
    }
}