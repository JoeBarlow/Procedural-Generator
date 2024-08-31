Shader "Custom/WaterShader"
{
    Properties
    {
        _SurfaceColour("Surface Colour", Color) = (1, 1, 1, 1)
        _WaveColour("Wave Colour", Color) = (1, 1, 1, 1)
        _VoronoiScale ("Voronoi Scale", Float) = 1.0
        _NoiseClamp("Noise Clamp", Float) = 1.0
        _ScrollSpeed("Scroll Speed", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv : TEXCOORD0;
            };            

            CBUFFER_START(UnityPerMaterial)
                float4 _SurfaceColour;
                float4 _WaveColour;
                float _VoronoiScale;
                float _NoiseClamp;
                float _ScrollSpeed;
            CBUFFER_END


             // Function to generate 2D Voronoi noise (from chatGPT)
            float voronoi(float2 pos)
            {
                float2 p = pos * _VoronoiScale;
                float minDist = 1.0;
                float2 minPoint;
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 testPoint = floor(p) + float2(x, y);
                        float2 randomOffset = frac(sin(dot(testPoint, float2(12.9898, 78.233))) * 43758.5453);
                        float2 candidate = testPoint + randomOffset;
                        float dist = distance(p, candidate) * _NoiseClamp;
                        if (dist < minDist)
                        {
                            minDist = dist;
                            minPoint = candidate;
                        }
                    }
                }
                return minDist;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                float2 waveOffset = float2(1.0, 1.0) * _ScrollSpeed;
                OUT.uv = IN.uv + (waveOffset * _Time.y);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float noiseValue = voronoi(uv);
                float4 finalColour = lerp(_SurfaceColour, _WaveColour, noiseValue);
                return finalColour;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode" = "DepthNormals"}


            HLSLPROGRAM

            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"

            ENDHLSL
        }
    }
}