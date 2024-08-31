Shader "Custom/TestShader"
{
    Properties
    {
        [Header(Surface Options)]
        [MainColor] _BaseColour("Colour", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseTexture("Image", 2D) = "white"{}
        _Smoothness("Smoothness", Float) = 0
        _Cull("__cull", Float) = 2.0
    }
    SubShader
    {
        Tags {"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}

        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            HLSLPROGRAM

            #define _SPECULAR_COLOR
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _SHADOWS_SOFT

            

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes{
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Interpolators{
                float4 positionCS : SV_POSITION;
                float3 positionWS: TEXCOORD1;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD2;
            };

            float4 _BaseColour;

            TEXTURE2D(_BaseTexture);
            SAMPLER(sampler_BaseTexture);
            float4 _BaseTexture_ST;

            float _Smoothness;

            Interpolators Vertex(Attributes IN){

                Interpolators OUT;

                VertexPositionInputs posnInputs = GetVertexPositionInputs(IN.positionOS);
                VertexNormalInputs normInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = posnInputs.positionCS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseTexture);
                OUT.normalWS = normInputs.normalWS;
                OUT.positionWS = posnInputs.positionWS;

                return OUT;
            }

            float4 Fragment(Interpolators IN) : SV_TARGET {

                float4 finalColour = SAMPLE_TEXTURE2D(_BaseTexture, sampler_BaseTexture, IN.uv);
                
                InputData inputData = (InputData)0;
                inputData.normalWS = normalize(IN.normalWS);
                inputData.positionWS = IN.positionWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = finalColour.rgb * _BaseColour.rgb;
                surfaceData.alpha = finalColour.a * _BaseColour.a;
                surfaceData.specular = 1;
                surfaceData.smoothness = _Smoothness;

                return UniversalFragmentBlinnPhong(inputData, surfaceData);
            };

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags {"Lightmode" = "ShadowCaster"}

            HLSLPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes{
                float3 positionOS : POSITION;
            };

            struct Interpolators{
                float4 positionCS : SV_POSITION;
            };

            Interpolators Vertex(Attributes IN)
            {
                Interpolators OUT;

                VertexPositionInputs posnInputs = GetVertexPositionInputs(IN.positionOS);

                OUT.positionCS = posnInputs.positionCS;

                return OUT;
            }

            float4 Fragment(Interpolators IN) : SV_TARGET{
                return 0;
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
