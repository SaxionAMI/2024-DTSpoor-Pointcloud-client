Shader "Custom/PointShader" {
    Properties{
        _PointSize("Point Size", float) = 0.05
        _Color("Point Color", Color) = (1,1,1,1) // Default color white
    }
        Subshader{
            Tags { "RenderType" = "Opaque" }
            Pass {
                CGPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_instancing
                #pragma multi_compile _ UNITY_STEREO_INSTANCING_ENABLED

                #include "UnityCG.cginc"

                struct appdata {
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f {
                    float4 pos : SV_POSITION;
                    half psize : PSIZE;
                    half4 col : COLOR;
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                float _PointSize;
                float4x4 _Transform;
                float3 _Offset;
                float3 _Scale;
                float4 _Color; // Added color property

                StructuredBuffer<float3> _Positions;

                v2f vert(uint vid : SV_VertexID, uint inst : SV_InstanceID, appdata v) {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_OUTPUT(v2f, o);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                    float3 pos = (_Positions[vid] + _Offset) * _Scale;

                    o.pos = UnityObjectToClipPos(mul(_Transform, float4(pos, 1)));
                    o.col = _Color; // Directly use the color property for all points
                    o.psize = _PointSize;

                    return o;
                }

                half4 frag(v2f i) : SV_Target {
                    return i.col;
                }

                ENDCG
            }
    }
}