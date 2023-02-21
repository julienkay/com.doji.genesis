using UnityEngine;

namespace Genesis {

    public class TestSkybox : MonoBehaviour {

        public string _prompt;
        public PromptType _type;

        private DepthEstimator _depthEstimator;

        public Texture2D TestImage;

        public RenderTexture In;
        public RenderTexture Out;

        private void Start() {
            _depthEstimator = new DepthEstimator();

            SkyboxPrompt prompt = new SkyboxPrompt(_prompt, _type);
            //var skyBox = await AssetForge.Instance.GenerateSkybox(prompt);
            //var skyBox = await AssetForge.Instance.GetSkyboxById("8fc0d4bc7608324e30fbab6c452ef742");
            var renderer = GetComponent<MeshRenderer>();
            //renderer.sharedMaterial.mainTexture = skyBox;

            Texture depth = _depthEstimator.GenerateDepth(TestImage);
            In = _depthEstimator._input;
            renderer.material.SetTexture("_MainTex", TestImage);
            renderer.material.SetTexture("_Depth", depth);

            // "disable" frustum culling
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            meshFilter.sharedMesh.bounds = new Bounds(transform.position, Vector3.one * float.MaxValue);

            _depthEstimator.SetProperties(renderer.sharedMaterial);
        }

        // Update is called once per frame
        void Update() {

        }

        private void OnDestroy() {
            if (_depthEstimator != null) {
                _depthEstimator.Dispose();
            }
        }
    }
}