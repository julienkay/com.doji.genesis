using UnityEngine;

namespace Genesis {

    public class DepthSkybox : MonoBehaviour {

        private void Awake() {
            // "Disable" frustum culling because we extrude vertices in the shader.
            // The sphere object would be erroneously frustum culled otherwise
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            meshFilter.sharedMesh.bounds = new Bounds(transform.position, Vector3.one * 100000f);
        }
    }
}