Shader "Custom/InteractiveRippleShader"
{
    Properties
    {
        // _MainTex ("Texture", 2D) = "white" {}
        // _Color ("Color", Color) = (1, 1, 1, 1)
        // [ShowAsVector2] _RippleCenter ("Ripple Center", Vector) = (0.5, 0.5, 0, 0)
        // _RippleSpeed ("Ripple Speed", Range(0, 10)) = 1
        // _RippleScale ("Ripple Scale", Range(0, 10)) = 1
        // _RippleDensity ("Ripple Density", Range(0, 100)) = 1
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
                float3 velocity;
            };

            StructuredBuffer<RipplePoint> rippleBuffer;

            // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "UnityCG.cginc"

            CBUFFER_START(UnityPerMaterial)

            CBUFFER_END

            // struct Attributes
            // {
            //     float4 positionOS   : POSITION;
            //     uint instanceID : SV_InstanceID;
            //     UNITY_VERTEX_INPUT_INSTANCE_ID
            // };

            // struct Varyings
            // {
            //     float4 positionHCS  : SV_POSITION;
            //     float4 color : COLOR;
            //     float size: PSIZE;
            // };

            struct appdata
            {
                float4 vertex : POSITION;
                // uint instanceID : SV_InstanceID;
                float2 uv : TEXCOORD0;
                // UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                // float height : TEXCOORD1;
            };

            // sampler2D _MainTex;
            // float4 _Color;
            // float4 _MainTex_ST;
            // float4 _RippleCenter;
            // float _RippleSpeed;
            // float _RippleScale;
            // float _RippleDensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;//TRANSFORM_TEX(v.uv, _MainTex);
                // o.height = _point.height;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                int x = round(i.uv.y * 128.0);
                RipplePoint _point = rippleBuffer[clamp(x, 0, 127)];
                return float4(0, _point.velocity.x, 0, 1);
            }
            ENDHLSL
        }
    }
}
