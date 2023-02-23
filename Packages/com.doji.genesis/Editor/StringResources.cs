using UnityEngine;

namespace Genesis.Editor {
    /// <summary>
    /// String Resources
    /// </summary>
    internal static class StringResources {
        internal static readonly string DefaultWindowHeader = "Genesis Editor";
        internal static readonly string GeneratetButton = "Generate Skybox";
        internal static readonly string GenerateTooltip = "Generates a new skybox with the given prompt";
        internal static readonly string ExtractButton = "Extract Mesh";
        internal static readonly string ExtractTooltip = "Extracts a mesh from this depth skybox while taking into account the current material settings.";
        internal static readonly GUIContent GenerateButton = new GUIContent(GeneratetButton, GenerateTooltip);
        internal static readonly GUIContent ExtractMeshButton = new GUIContent(ExtractButton, ExtractTooltip);
    }
}