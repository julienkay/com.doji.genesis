using System;
using System.Linq;
using Unity.Barracuda;
using UnityEngine;

namespace Genesis {
    public class DepthEstimator : IDisposable {
        public static NNModel MiDaSv2 {
            get {
                if (_MiDaSv2 == null) {
                    _MiDaSv2 = Resources.Load<NNModel>("ONNX/MiDaS_model-small");
                }
                return _MiDaSv2;
            } 
        }
        private static NNModel _MiDaSv2;

        private Model _model;
        private IWorker _engine;
        public RenderTexture _input, _output;
        private int _width, _height;

        private float _minDepth, _maxDepth;

        // Shader Properties
        private static readonly int _minShaderProp = Shader.PropertyToID("_Min");
        private static readonly int _maxShaderProp = Shader.PropertyToID("_Max");
        public DepthEstimator() {
            InitializeNetwork();
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

            //OnImageResized.Invoke(inputTexture.height / (float)inputTexture.width);
            //OnDepthSolved.Invoke(_output);

            //DeallocateObjects();

            return _output;
        }

        public void SetProperties(Material mat) {
            // In Barracuda 1.0.4 the output of MiDaS can be passed  directly to a texture as it is shaped correctly.
            // In later versions we have to swap X and Y axes, and also flip the X axis...
            // We need to inform the shader of this change.
#if BARRACUDA_1_0_5_OR_NEWER
            mat.SetInt("_SwapChannels", 1);
#else
            mat.SetInt("_SwapChannels", 0);
#endif

            mat.SetFloat(_minShaderProp, _minDepth);
            mat.SetFloat(_maxShaderProp, _maxDepth);
        }

        /// <summary>
        /// Loads the <see cref="NNModel"/> asset in memory and creates a Barracuda <see cref="IWorker"/>
        /// </summary>
        private void InitializeNetwork() {
            if (MiDaSv2 == null) {
                throw new Exception("Could not find NN model.");
            }

            // Load the model to memory
            _model = ModelLoader.Load(MiDaSv2);

            // Create a worker
            _engine = WorkerFactory.CreateWorker(_model, WorkerFactory.Device.GPU);

            // Get Tensor dimensionality ( texture width/height )
            // In Barracuda 1.0.4 the width and height are in channels 1 & 2.
            // In later versions in channels 5 & 6
#if BARRACUDA_1_0_5_OR_NEWER
            _width  = _model.inputs[0].shape[5];
            _height = _model.inputs[0].shape[6];
#else
            _width = _model.inputs[0].shape[1];
            _height = _model.inputs[0].shape[2];
#endif
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
            _output = RenderTexture.GetTemporary(_width, _height, 0, RenderTextureFormat.RFloat);
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

        /// <summary>
        /// Performs Inference on the Neural Network Model
        /// </summary>
        /// <param name="source"></param>
        private void RunModel(Texture source) {
            using (var tensor = new Tensor(source, 3)) {
                _engine.Execute(tensor);

                // In Barracuda 1.0.4 the output of MiDaS can be passed  directly to a texture as it is shaped correctly.
                // In later versions we have to reshape the tensor. Don't ask why...
#if BARRACUDA_1_0_5_OR_NEWER
                var to = _engine.PeekOutput().Reshape(new TensorShape(1, _width, _height, 1));
#else
                var to = _engine.PeekOutput();
#endif

                //TensorToRenderTexture(to, _output, fromChannel: 0);
                to.ToRenderTexture(_output, fromChannel: 0);

                var data = to.data.SharedAccess(out var o);
                _minDepth = ((float[])data).Min();
                _maxDepth = ((float[])data).Max();
                Debug.Log(_minDepth);
                Debug.Log(_maxDepth);

                to?.Dispose();
            }
        }

        public void Dispose() {
            _engine?.Dispose();
            _engine = null;
        }
    }
}