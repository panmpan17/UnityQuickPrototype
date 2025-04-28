Shader "Custom/GrassTriangle"
{
    Properties
    {
        _LowerGreen("Lower Green", Color) = (0.0, 0.5, 0.0, 1.0)
        _UpperGreen("Upper Green", Color) = (0.0, 1.0, 0.0, 1.0)
        _NoiseTexture("Noise Texture", 2D) = "white" {}
        _NoiseScale("Noise Scale", Float) = 1.0

        _WindSpeed ("Wind Speed + Wind Factor", Vector) = (0.5, 0.5, 0, 0)
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct GrassPoint{
                float3 position;
                float height;
            };

            // Pass from the csharp scrip
            StructuredBuffer<GrassPoint> grassPointBuffer;
            float3 unity_CameraUp;
            float3 unity_CameraRight;

            float unity_Time;

            // Pass from the material editor
            float4 _LowerGreen;
            float4 _UpperGreen;

            float4 _WindSpeed;

            sampler2D _NoiseTexture;
            // SamplerState sampler_NoiseTexture;
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
            };

            v2f vert (appdata v)
            {
                float3 basePosition = grassPointBuffer[v.instanceID].position;

                float2 offsets[3] = {
                    float2(0.1f, 0),
                    float2(-0.1f, 0),
                    float2(0.0, 1),
                };
                

                float x = offsets[v.vertexID].x;
                float y = offsets[v.vertexID].y * grassPointBuffer[v.instanceID].height;

                float3 offsetWorld = x * unity_CameraRight;
                offsetWorld += y * unity_CameraUp;


                v2f o;
                o.uv = float2(basePosition.x, basePosition.z) * _NoiseScale;

                if (v.vertexID == 2)
                {
                    float4 uv = float4(unity_Time * _WindSpeed.x + o.uv.x, unity_Time * _WindSpeed.y + o.uv.y, 0, 0);
                    float noiseValue = tex2Dlod(_NoiseTexture, uv).x;
                    offsetWorld.x += noiseValue * _WindSpeed.z;
                    offsetWorld.z += noiseValue * _WindSpeed.w;

                    // offsetWorld.x += sin(unity_Time * _WindSpeed.x) * _WindSpeed.z;
                    // offsetWorld.z += sin(unity_Time * _WindSpeed.y) * _WindSpeed.w;
                }

                o.vertex = TransformWorldToHClip(basePosition + offsetWorld);
                o.height = y;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // float noiseValue = _NoiseTexture.Sample(sampler_NoiseTexture, i.uv).r;
                float noiseValue = tex2D(_NoiseTexture, i.uv).r;
                float4 greenColor = lerp(_LowerGreen, _UpperGreen, i.height / 2);
                return lerp(greenColor, float4(1, 1, 0, 1), noiseValue); // white
            }
            ENDHLSL
        }
    }
}
