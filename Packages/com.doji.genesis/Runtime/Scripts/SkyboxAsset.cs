using System;
using UnityEngine;

namespace Genesis {

    public class SkyboxAsset : ScriptableObject {

        public Texture2D TextureAsset { get; set; }

        public string Id { get; set; }

        public string Title { get; set; }

        public Uri FileUrl { get; set; }

        public Uri ThumbUrl { get; set; }

        /// <summary>
        /// The version of the genesis package that this asset was originally created with.
        /// </summary>
        public Version Version { get; set; }

    }
}