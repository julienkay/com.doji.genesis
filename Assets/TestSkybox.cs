using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetForger {


    public class TestSkybox : MonoBehaviour {

        private void Start() {
            AssetForge.Instance.GetSkyboxImage()
        }

        // Update is called once per frame
        void Update() {

        }
    }
}