Shader "Custom/GrassTriangle"
{
    Properties
    {
        _Color1("Color1", Color) = (1, 1, 1, 1)
        _Color2("Color2", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma exclude_renderers d3d11

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fog

            // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "UnityCG.cginc"

            struct Cricket{
                float3 position;
                float3 velocity;
            };

            // Pass from the csharp scrip
            StructuredBuffer<Cricket> cricketBuffer;

            // Pass from the material editor
            float4 _Color1;
            float4 _Color2;

            struct appdata
            {
                uint instanceID : SV_InstanceID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float size: PSIZE;
                float colorIndex : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(cricketBuffer[v.instanceID].position);
                UNITY_TRANSFER_FOG(o, o.vertex);
                o.size = 5;
                o.colorIndex = float((v.instanceID % 20.f) / 20.f);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 color = lerp(_Color1, _Color2, i.colorIndex);
                UNITY_APPLY_FOG(i.fogCoord, color);
                return color;
            }
            ENDHLSL
        }
    }
}
