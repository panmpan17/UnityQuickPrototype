Shader "Custom/Cricket"
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

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // Render State Commands
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // #include "UnityCG.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif

            struct Cricket{
                float3 position;
                float3 velocity;
            };

            // Pass from the csharp scrip
            StructuredBuffer<Cricket> cricketBuffer;

            struct appdata
            {
                uint instanceID : SV_InstanceID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                // float size: PSIZE;
                // float colorIndex : TEXCOORD1;
                // UNITY_FOG_COORDS(2)
            };

            // Shader Stages
            #pragma vertex vert
            #pragma fragment frag

            // Material Keywords
        //     #pragma shader_feature_local _ALPHATEST_ON
        //     #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // GPU Instancing
        //     #pragma multi_compile_instancing
        //     #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Unity defined keywords
            // #pragma multi_compile _ LOD_FADE_CROSSFADE

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            float3 _LightDirection;
            float3 _LightPosition;

            float4 GetShadowPositionHClip(float3 positionWS)
            {
                // float3 positionWS = TransformObjectToWorld(position);
                // float3 normalWS = TransformObjectToWorldNormal(float3(0, -1, 0));

            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, float3(0, -1, 0), lightDirectionWS));
                positionCS = ApplyShadowClamping(positionCS);
                return positionCS;
            }

            v2f vert (appdata v)
            {
                // UNITY_SETUP_INSTANCE_ID(v);

                v2f o;
                // o.vertex = float4(cricketBuffer[v.instanceID].position, 0);
                // o.vertex = TransformObjectToHClip(cricketBuffer[v.instanceID].position);
                o.vertex = GetShadowPositionHClip(TransformObjectToHClip(cricketBuffer[v.instanceID].position));
                o.vertex = ApplyShadowClamping(o.vertex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
// #if defined(LOD_FADE_CROSSFADE)
//                 LODFadeCrossFade(i.vertex);
// #endif

                return 0;
            }

            ENDHLSL
        }
    }
}
