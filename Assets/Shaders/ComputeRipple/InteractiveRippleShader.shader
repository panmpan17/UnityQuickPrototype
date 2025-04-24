Shader "Custom/InteractiveRippleShader"
{
    Properties
    {
        _BufferSize ("Buffer Size", Range(1, 128)) = 128
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            // CGPROGRAM
            // Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members height)
            #pragma exclude_renderers d3d11

            #pragma vertex vert
            #pragma fragment frag

            struct RipplePoint{
                float height;
                float velocity;
            };

            StructuredBuffer<RipplePoint> rippleBuffer;
            int _BufferSize;

            #include "UnityCG.cginc"

            CBUFFER_START(UnityPerMaterial)

            CBUFFER_END

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                int x = clamp(round(i.uv.x * _BufferSize), 0, _BufferSize - 1);
                int y = clamp(round(i.uv.y * _BufferSize), 0, _BufferSize - 1);
                RipplePoint _point = rippleBuffer[x + y * _BufferSize];
                return float4(0, _point.height, 0, 1);
                // return float4(x, y, 0, 1);
            }
            ENDHLSL
        }
    }
}
