Shader "Genesis/EquirectDepth" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Depth("Depth", 2D) = "white" {}
        _MinDistance("Min Distance", Range(0.0, 100.0)) = 0.0
        _MaxDistance("Max Distance", Range(10.0, 10000.0)) = 100.0
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
            
            // Minimum predicted inverse depth
            float _Min;

            // Maximum predicted inverse depth
            float _Max;

            float _MinDistance;
            float _MaxDistance;

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

                //noramlize to range [0, 1] and go from MiDaS' reversed depth to depth
                float normalizedDepth = 1.0 - saturate((depth - _Min) / (_Max - _Min));

                // Vertex displacement (assumes unit sphere with radius 1)
                depth = (normalizedDepth * _MaxDistance) + _MinDistance;
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
