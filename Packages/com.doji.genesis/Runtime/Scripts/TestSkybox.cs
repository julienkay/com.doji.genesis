using UnityEngine;

namespace Genesis {

    public class TestSkybox : MonoBehaviour {

        public string _prompt;
        public PromptType _type;

        private DepthFromImage _depthFromImage;

        public Texture2D TestImage;

        public RenderTexture In;
        public RenderTexture Out;

        private void Start() {
            _depthFromImage = new DepthFromImage();

            SkyboxPrompt prompt = new SkyboxPrompt(_prompt, _type);
            //var skyBox = await AssetForge.Instance.GenerateSkybox(prompt);
            //var skyBox = await AssetForge.Instance.GetSkyboxById("8fc0d4bc7608324e30fbab6c452ef742");
            var renderer = GetComponent<MeshRenderer>();
            //renderer.sharedMaterial.mainTexture = skyBox;

            Texture depth = _depthFromImage.GenerateDepth(TestImage);
            In = _depthFromImage._input;
            renderer.sharedMaterial.SetTexture("_MainTex", TestImage);
            renderer.sharedMaterial.SetTexture("_Depth", depth);

            // "disable" frustum culling
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            meshFilter.sharedMesh.bounds = new Bounds(transform.position, Vector3.one * float.MaxValue);

            _depthFromImage.SetProperties(renderer.sharedMaterial);
        }

        // Update is called once per frame
        void Update() {

        }

        private void OnDestroy() {
            if (_depthFromImage != null) {
                _depthFromImage.Dispose();
            }
        }
    }
}