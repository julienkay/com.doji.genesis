using UnityEngine;

namespace Genesis.Editor {
    public partial class GenesisEditorWindow {
        /// <summary>
        /// String Resources
        /// </summary>
        private static class SR {
            internal static readonly string DefaultWindowHeader = "Genesis Editor";
            internal static readonly string GeneratetButton     = "Generate Skybox";
            internal static readonly string GenerateTooltip     = "Generates a new skybox with the given prompt";
            internal static readonly GUIContent GenerateButton = new GUIContent(GeneratetButton, GenerateTooltip);
        }
    }
}