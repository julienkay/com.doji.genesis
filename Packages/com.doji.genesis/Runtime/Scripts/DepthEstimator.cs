using System;
using System.Linq;
using Unity.Sentis;
using Unity.Collections;
using UnityEngine;

namespace Genesis {
    public class DepthEstimator {
        public static Model MiDaSv2 {
            get {
                if (_MiDaSv2Asset == null) {
                    _MiDaSv2Asset = Resources.Load<ModelAsset>("ONNX/MiDaS_model-small");
                }
                if (_MiDaSv2Asset == null) {
                    Debug.LogError("MiDaS v2 ONNX model not found.");
                } else {
                    _MiDaSv2 = ModelLoader.Load(_MiDaSv2Asset);
                }
                return _MiDaSv2;
            } 
        }
        private static Model _MiDaSv2;
        private static ModelAsset _MiDaSv2Asset;

        public static Model MiDaSv31 {
            get {
                if (_MiDaSv31Asset == null) {
                    _MiDaSv31Asset = Resources.Load<ModelAsset>("ONNX/dpt_beit_large_512");
                }
                if (_MiDaSv31Asset == null) {
                    Debug.LogError("MiDaS v3.1 ONNX model not found.");
                } else {
                    _MiDaSv31 = ModelLoader.Load(_MiDaSv31Asset);
                }
                return _MiDaSv31;
            }
        }
        private static Model _MiDaSv31;
        private static ModelAsset _MiDaSv31Asset;

        private IWorker _worker;
        ITensorAllocator allocator;
        Ops ops;
        public RenderTexture _input, _output;
        private int _width, _height;

        public float MinDepth { get; private set; }
        public float MaxDepth { get; private set; }

        private Material _rotateMat = new Material(Shader.Find("Hidden/BlitRotateMirror"));

        public DepthEstimator() {
            InitializeNetwork();
        }
        private void InitializeNetwork() {
            Model model = MiDaSv31;
            _worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, model);
            _width = model.inputs[0].shape[2].value;
            _height = model.inputs[0].shape[3].value;
        }

        public RenderTexture GenerateDepth(Texture2D inputTexture) {
            if (inputTexture == null) {
                throw new ArgumentNullException(nameof(inputTexture));
            }

            if (MiDaSv2 == null) {
                throw new ArgumentNullException(nameof(MiDaSv2));
            }
            AllocateObjects();

            // Fast resize
            Graphics.Blit(inputTexture, _input);

            RunModel(_input);

            // Newer versions of Barracuda mess up the X and Y directions.
            // So we rotate the output here, so we don't have to mess with
            // swapping the UV in the shader and all other processing steps.
            //RotateAndMirrorOutput();

            return _output;
        }
        
        private void RotateAndMirrorOutput() {
            RenderTexture rotated = RenderTexture.GetTemporary(_output.descriptor);

            Graphics.Blit(_output, rotated, _rotateMat);
            _output.Release();
            _output = rotated;
        }


        /// <summary>
        /// Do some post processing of the depth texture to
        /// - fix depth discontinuities / seams
        /// - fix distortions at the poles
        /// - normalize depth values
        /// </summary>
        public Texture2D PostProcessDepth() {
            RenderTexture depth = _output;
            Texture2D depthTexture = new Texture2D(depth.width, depth.height, TextureFormat.RFloat, mipChain: false, linear: true);
            depthTexture.wrapModeU = TextureWrapMode.Repeat;
            depthTexture.wrapModeV = TextureWrapMode.Clamp;
            RenderTexture.active = depth;
            depthTexture.ReadPixels(new Rect(0, 0, depth.width, depth.height), 0, 0);
            RenderTexture.active = null;

            var depthData = depthTexture.GetPixelData<float>(0);

            NormalizeDepth(depthData);

            depthTexture.Apply(false);
            return depthTexture;
        }

        /// <summary>
        /// Makes sure all values of the depth texture are in the [0, 1] range
        /// and converts from MiDaS' reversed depth to depth.
        /// </summary>
        private void NormalizeDepth(NativeArray<float> depthData) {
            for (int i = 0; i < depthData.Length; i++) {
                float depth = depthData[i];
                float normalizedDepth = Mathf.Clamp01((depth - MinDepth) / (MaxDepth - MinDepth));
                depthData[i] = normalizedDepth;
            }
        }

        /// <summary>
        /// Allocates the necessary <see cref="RenderTexture"/> objects.
        /// </summary>
        private void AllocateObjects() {
            // Check for accidental memory leaks
            if (_input != null) {
                _input.Release();
            }

            if (_output != null) {
                _output.Release();
            }

            // Declare texture resources
            _input = RenderTexture.GetTemporary(_width, _height, 0, RenderTextureFormat.ARGB32);
            _output = RenderTexture.GetTemporary(_width, _height, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        }

        /// <summary>
        /// Releases all unmanaged objects
        /// </summary>
        private void DeallocateObjects() {
            if (_input != null) {
                _input.Release();
            }
            _input = null;

            if (_output != null) {
                _output.Release();
            }
            _output = null;
        }

        private void RunModel(Texture source) {
            using (var tensor = TextureConverter.ToTensor(source, source.width, source.height, 3)) {
                _worker.Execute(tensor);
            }

            Tensor output = _worker.PeekOutput();

            int height = output.shape[1];
            int width = output.shape[2];


            _output = TextureConverter.ToTexture(output.ShallowReshape(new TensorShape(1, 1, height, width)) as TensorFloat, width, height);

            Ops ops = WorkerFactory.CreateOps(BackendType.GPUCompute, allocator);
            TensorFloat minT = ops.ReduceMin(output as TensorFloat, null, false);
            TensorFloat maxT = ops.ReduceMax(output as TensorFloat, null, false);
            minT.MakeReadable();
            maxT.MakeReadable();
            MinDepth = minT[0];
            MaxDepth = maxT[0];
        }

        public void Dispose() {
            ops?.Dispose();
            allocator?.Dispose();
            _worker?.Dispose();
            DeallocateObjects();
        }
    }
}