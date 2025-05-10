Shader "Custom/GrassTriangle"
{
    Properties
    {
        _LowerGreen("Lower Green", Color) = (0.0, 0.5, 0.0, 1.0)
        _UpperGreen("Upper Green", Color) = (0.0, 1.0, 0.0, 1.0)
        _HighlightColor("Highlight Color", Color) = (1.0, 1.0, 1.0, 1.0)
        // _WindColor("Wind Color", Color) = (1.0, 1.0, 1.0, 1.0)

        _NoiseTexture("Noise Texture", 2D) = "white" {}
        _NoiseScale("Noise Scale", Float) = 1.0

        _WindSpeed ("Wind Speed + Wind Factor", Vector) = (0.5, 0.5, 0, 0)
    }
    SubShader
    {
        Tags {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma exclude_renderers d3d11

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

            // #include "UnityCG.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct GrassPoint{
                float3 position;
                float height;
                float4 rotation;
            };

            // Pass from the csharp scrip
            StructuredBuffer<GrassPoint> grassPointBuffer;
            float4 unity_CameraRotation;

            float unity_Time;

            // Pass from the material editor
            float4 _LowerGreen;
            float4 _UpperGreen;
            float4 _HighlightColor;
            // float4 _WindColor;

            float4 _WindSpeed;

            sampler2D _NoiseTexture;
            float _NoiseScale;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint vertexID : SV_VertexID;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float height : TEXCOORD1;
                // UNITY_FOG_COORDS(2)
                float4 shadowCoord : TEXCOORD3;
            };

            float4 qmul(float4 q1, float4 q2)
            {
                return float4(
                    q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz),
                    q1.w * q2.w - dot(q1.xyz, q2.xyz)
                );
            }
            float3 rotate_vector(float3 v, float4 r)
            {
                float4 r_c = r * float4(-1, -1, -1, 1);
                return qmul(r, qmul(float4(v, 0), r_c)).xyz;
            }

            float4 QuaternionFromYawPitch(float yaw, float pitch)
            {
                float cy = cos(yaw * 0.5);
                float sy = sin(yaw * 0.5);
                float cp = cos(pitch * 0.5);
                float sp = sin(pitch * 0.5);

                // No roll, so cr = 1, sr = 0
                float4 q;
                q.x = sp * cy;  // pitch
                q.y = cp * sy;  // yaw
                q.z = -sp * sy; // combination
                q.w = cp * cy;  // overall scalar

                return q;
            }

            v2f vert (appdata v)
            {
                float3 basePosition = grassPointBuffer[v.instanceID].position;

                float2 offsets[3] = {
                    float2(0.1f, 0),
                    float2(-0.1f, 0),
                    float2(0.0, 1),
                };
                

                float3 offset = float3(offsets[v.vertexID].x,
                                       offsets[v.vertexID].y * grassPointBuffer[v.instanceID].height,
                                       0);


                v2f o;
                o.uv = float2(basePosition.x, basePosition.z) * _NoiseScale;

                float3 offsetWorld = rotate_vector(offset, unity_CameraRotation);
                // float4 combind = float4(offset.y, (float)v.vertexID, unity_Time * _WindSpeed.x + basePosition.x, unity_Time * _WindSpeed.y + basePosition.y);
                if (v.vertexID == 2)
                {
                    float4 uv = float4(unity_Time * _WindSpeed.x + basePosition.x, unity_Time * _WindSpeed.y + basePosition.y, 0, 0);
                    float noiseValue = tex2Dlod(_NoiseTexture, uv).x;
                    float angle = noiseValue * _WindSpeed.z;
                    float4 quat = float4(0,  sin(angle * 0.5), 0, cos(angle * 0.5));

                    // float4 quat = QuaternionFromYawPitch(noiseValue * _WindSpeed.z, noiseValue * _WindSpeed.w);

                    offsetWorld = rotate_vector(offsetWorld, quat);
                    offsetWorld = rotate_vector(offsetWorld, grassPointBuffer[v.instanceID].rotation);
                }

                
                o.vertex = TransformObjectToHClip(basePosition + offsetWorld);
                // UNITY_TRANSFER_FOG(o, o.vertex);
                // o.combind = combind;
                o.height = offset.y;

                VertexPositionInputs positions = GetVertexPositionInputs(basePosition + offsetWorld);
                float4 shadowCoordinates = GetShadowCoord(positions);

                o.shadowCoord = shadowCoordinates;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // float noiseValue = _NoiseTexture.Sample(sampler_NoiseTexture, i.uv).r;
                float noiseValue = tex2D(_NoiseTexture, i.uv).r;
                float4 greenColor = lerp(_LowerGreen, _UpperGreen, i.height / 2);
                float4 highlightColor = lerp(greenColor, _HighlightColor, noiseValue);

// #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
//                 UNITY_APPLY_FOG(i.fogCoord, highlightColor);
// #endif

                half shadowAmount = MainLightRealtimeShadow(i.shadowCoord);
                return highlightColor * shadowAmount;
                // if (i.combind.y > 1.7 && i.combind.y < 2.3)
                // {
                //     float2 uv = float2(i.combind.z, i.combind.w);
                //     noiseValue = tex2D(_NoiseTexture, uv).r * 0.5;
                //     return lerp(highlightColor, _WindColor, noiseValue);
                //     // return _WindColor;
                // }
                // else
                // {
                //     return highlightColor;
                // }
            }
            ENDHLSL
        }
    }
}
