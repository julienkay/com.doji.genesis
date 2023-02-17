using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetForger {


    public class TestSkybox : MonoBehaviour {

        public string _prompt;
        public PromptType _type;

        private async void Start() {
            SkyboxPrompt prompt = new SkyboxPrompt(_prompt, _type);
            //var skyBox = await AssetForge.Instance.GenerateSkybox(prompt);
            var skyBox = await AssetForge.Instance.GetSkyboxById("8fc0d4bc7608324e30fbab6c452ef742");
            var renderer = GetComponent<MeshRenderer>();
            renderer.sharedMaterial.mainTexture = skyBox;
        }

        // Update is called once per frame
        void Update() {

        }
    }
}