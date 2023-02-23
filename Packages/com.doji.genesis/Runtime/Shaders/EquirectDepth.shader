Shader "Genesis/EquirectDepth" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Depth("Depth", 2D) = "white" {}
        _Scale("Depth Multiplier", float) = 1
        _Max("Max Depth Cutoff", float) = 1000
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _Depth;

            float _Min;
            float _Max;
            float _Scale;

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

                // mirror x because we render on the inside of a sphere
                v.uv.x = 1 - v.uv.x;
                
                float depth = tex2Dlod(_Depth, float4(v.uv, 0, 0));
                depth = _Scale / depth;
                depth = clamp(depth, 0, _Max * _Scale);

                // Vertex displacement (assumes rendering on a unit sphere with radius 1)
                o.vertex = UnityObjectToClipPos(v.vertex * depth);

                // clamp to far clip plane (assumes reversed-Z)
                if (o.vertex.z < 1.0e-3f) {
                    o.vertex.z = 1.0e-3f;
                }

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float4 col;
                col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
