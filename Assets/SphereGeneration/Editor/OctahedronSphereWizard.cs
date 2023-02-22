using UnityEditor;
using UnityEngine;

public class OctahedronSphereWizard : ScriptableWizard {
	
	[MenuItem("Assets/Create/Octahedron Sphere")]
	private static void CreateWizard () {
		ScriptableWizard.DisplayWizard<OctahedronSphereWizard>("Create Octahedron Sphere");
	}
	
	public int level = 6;
	public float radius = 1f;
	
	private void OnWizardCreate () {
		string path = EditorUtility.SaveFilePanelInProject("Save Octahedron Sphere", "Octahedron Sphere", "asset", "Specify where to save the mesh.");
		if (path.Length > 0) {
			Mesh mesh = OctahedronSphereCreator.Create(level, radius);
			MeshUtility.Optimize(mesh);
			AssetDatabase.CreateAsset(mesh, path);
			Selection.activeObject = mesh;
		}
	}
}