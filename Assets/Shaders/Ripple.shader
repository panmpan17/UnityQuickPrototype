Shader "Unlit/TestShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        [ShowAsVector2] _RippleCenter ("Ripple Center", Vector) = (0.5, 0.5, 0, 0)
        _RippleSpeed ("Ripple Speed", Range(0, 10)) = 1
        _RippleScale ("Ripple Scale", Range(0, 10)) = 1
        _RippleDensity ("Ripple Density", Range(0, 100)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Color;
            float4 _MainTex_ST;
            float4 _RippleCenter;
            float _RippleSpeed;
            float _RippleScale;
            float _RippleDensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;//TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                float distToCenter = distance(i.uv, _RippleCenter.xy);
                float invertDistance = max(1 - (distToCenter * _RippleScale), 0);

                float rippleMoveAmount = _Time.y * _RippleSpeed;

                float rippleFloat = sin((invertDistance / (1 / _RippleDensity)) + rippleMoveAmount) * invertDistance;

                float2 uvOffsetCenter = float2(i.uv.x - 0.5, i.uv.y - 0.5);

                float2 uv = i.uv + (uvOffsetCenter * max(rippleFloat, 0));

                fixed4 col = tex2D(_MainTex, uv);
                return col;
            }
            ENDCG
        }
    }
}
