using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Genesis {

    public interface IDepthSampler {
        public float SampleBilinear(Vector2 uv);
    }

    /// <summary>
    /// Allows bilinear sampling from a texture containing
    /// a one component, 32-bit float channel like <see cref="GraphicsFormat.R32_SFloat"/>.
    /// </summary>
    public class DepthSamplerFloat : DepthSampler<float> {
        public DepthSamplerFloat(Texture2D depthTexture) : base(depthTexture) {
            DepthData = depthTexture.GetPixelData<float>(0);
        }

        public override float SampleBilinear(Vector2 uv) {
            float i = Mathf.Lerp(0f, Width - 1, uv.x);
            int i0 = Mathf.FloorToInt(i);
            int i1 = i0 < Width - 1 ? i0 + 1 : i0;

            float j = Mathf.Lerp(0f, Height - 1, uv.y);
            int j0 = Mathf.FloorToInt(j);
            int j1 = j0 < Height - 1 ? j0 + 1 : j0;

            float q11 = DepthData[(j0 * Width) + i0];
            float q21 = DepthData[(j0 * Width) + i1];
            float q12 = DepthData[(j1 * Width) + i0];
            float q22 = DepthData[(j1 * Width) + i1];

            float dx = i - i0;
            float dy = j - j0;

            float v1 = q11 + dx * (q21 - q11);
            float v2 = q12 + dx * (q22 - q12);

            return v1 + dy * (v2 - v1);
        }
    }

    /// <summary>
    /// Allows bilinear sampling from a one-component 8-bit texture 
    /// like <see cref="GraphicsFormat.R8_UNorm"/>.
    /// </summary>
    public class DepthSamplerByte : DepthSampler<byte> {
        public DepthSamplerByte(Texture2D depthTexture) : base(depthTexture) {
            DepthData = depthTexture.GetPixelData<byte>(0);
        }

        public override float SampleBilinear(Vector2 uv) {
            float i = Mathf.Lerp(0f, Width - 1, uv.x);
            int i0 = Mathf.FloorToInt(i);
            int i1 = i0 < Width - 1 ? i0 + 1 : i0;

            float j = Mathf.Lerp(0f, Height - 1, uv.y);
            int j0 = Mathf.FloorToInt(j);
            int j1 = j0 < Height - 1 ? j0 + 1 : j0;

            float q11 = DepthData[(j0 * Width) + i0] / 255.0f;
            float q21 = DepthData[(j0 * Width) + i1] / 255.0f;
            float q12 = DepthData[(j1 * Width) + i0] / 255.0f;
            float q22 = DepthData[(j1 * Width) + i1] / 255.0f;

            float dx = i - i0;
            float dy = j - j0;

            float v1 = q11 + dx * (q21 - q11);
            float v2 = q12 + dx * (q22 - q12);

            return v1 + dy * (v2 - v1);
        }
    }

    /// <summary>
    /// Allows bilinear sampling from a texture while treating it as a grayscale
    /// image using the red channel as the depth value.
    /// </summary>
    public class DepthSamplerRGB24 : DepthSampler<Color24> {
        public DepthSamplerRGB24(Texture2D depthTexture) : base(depthTexture) {
            DepthData = depthTexture.GetPixelData<Color24>(0);
        }

        public override float SampleBilinear(Vector2 uv) {
            float i = Mathf.Lerp(0f, Width - 1, uv.x);
            int i0 = Mathf.FloorToInt(i);
            int i1 = i0 < Width - 1 ? i0 + 1 : i0;

            float j = Mathf.Lerp(0f, Height - 1, uv.y);
            int j0 = Mathf.FloorToInt(j);
            int j1 = j0 < Height - 1 ? j0 + 1 : j0;

            float q11 = DepthData[(j0 * Width) + i0].r / 255.0f;
            float q21 = DepthData[(j0 * Width) + i1].r / 255.0f;
            float q12 = DepthData[(j1 * Width) + i0].r / 255.0f;
            float q22 = DepthData[(j1 * Width) + i1].r / 255.0f;

            float dx = i - i0;
            float dy = j - j0;

            float v1 = q11 + dx * (q21 - q11);
            float v2 = q12 + dx * (q22 - q12);

            return v1 + dy * (v2 - v1);
        }
    }

    public static class DepthSampler {

        public static IDepthSampler Get(Texture2D depthTexture) {
            switch (depthTexture.graphicsFormat) {
                case GraphicsFormat.R32_SFloat:
                    return new DepthSamplerFloat(depthTexture);
                case GraphicsFormat.R8_UNorm:
                    return new DepthSamplerByte(depthTexture);
                case GraphicsFormat.R8G8B8_UNorm:
                    return new DepthSamplerRGB24(depthTexture);
                default:
                    throw new NotSupportedException($"Sampling depth from texture format {depthTexture.graphicsFormat} is not supported.");
            }
        }

        public static bool IsFormatSupported(GraphicsFormat graphicsFormat) {
            switch (graphicsFormat) {
                case GraphicsFormat.R32_SFloat:
                case GraphicsFormat.R8_UNorm:
                case GraphicsFormat.R8G8B8_UNorm:
                    return true;
                default:
                    return false;
            }
        }
    }

    public abstract class DepthSampler<T> : IDepthSampler where T : struct {

        private Texture2D DepthTexture { get; set; }
        protected NativeArray<T> DepthData { get; set; }
        protected int Width { get; private set; }
        protected int Height { get; private set; }

        protected DepthSampler(Texture2D depthTexture) {
            DepthTexture = depthTexture;
            Width = DepthTexture.width;
            Height = DepthTexture.height;
        }

        public abstract float SampleBilinear(Vector2 uv);

    }

    public struct Color24 {
        public byte r;
        public byte g;
        public byte b;
    }
}