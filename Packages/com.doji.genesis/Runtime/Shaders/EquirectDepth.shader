Shader "Genesis/EquirectDepth" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Depth("Depth", 2D) = "white" {}
        _MinDepth("Min Depth", Range(0.0, 100.0)) = 2.0
        _MaxDepth("Max Depth", Range(10.0, 10000.0)) = 100.0
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

            float _MinDepth;
            float _MaxDepth;

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

                float2 uv = v.uv.yx; // flip needed because of how Barracuda outputs data
                uv.x = 1 - uv.x;     // invert x because we use an outward-facing sphere with 'Cull Front'
                
                float depth = tex2Dlod(_Depth, float4(uv, 0, 0));

                // assume normalized depth, map to a given min, max range
                depth = (depth * _MaxDepth) + _MinDepth;

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

                i.uv.x = 1 - i.uv.x;
                col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
