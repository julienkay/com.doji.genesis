Shader "AssetForger/EquirectDepth" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Depth("Depth", 2D) = "white" {}
        _Min("Min Depth", Range(0.0, 10000.0)) = 0.0
        _Max("Max Depth", Range(0.0, 10000.0)) = 1000.0
        _DepthMultiplier("Depth Exaggeration", Range(0.0, 1.0)) = 0.01
        _LogNorm("Log Normalization Factor", Range(0.0, 2.0)) = 1
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Front

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _Depth;
            
            // Minimum recorded depth
            float _Min;

            // Maximum recorded depth
            float _Max;

            // Depth Exaggeration
            float _DepthMultiplier;

            // Log-Normalization factor
            float _LogNorm;

            int _SwapChannels;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v) {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // Newer versions of Barracuda mess up the X and Y directions. Therefore the UV has to be swapped
                float2 uv = v.uv.yx;
                uv.x = 1 - uv.x;
                // Vertex displacement (assumes unit sphere with radius 1)
                float depth = clamp(_Max- tex2Dlod(_Depth, float4(uv, 0, 0)), 0, _Max);
                o.vertex = UnityObjectToClipPos(v.vertex * depth * _DepthMultiplier);

                // clamp to far clip plane (assumes reversed-Z)
                if (o.vertex.z < 1.0e-3f) {
                    o.vertex.z = 1.0e-3f;
                }

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float4 col;
                // The color texture is sampled normally, so we have to flip the coordinates back
                float2 uv = lerp(i.uv, float2(1 - i.uv.y, 1 - i.uv.x), _SwapChannels);

                col = tex2D(_MainTex, uv);
                return col;
            }
            ENDCG
        }
    }
}
