Shader "Custom/TerrainGradient"
{
    Properties
    {
        [Header(Gradient Heights)]
        _SandCutoff("Sand Cutoff", Float) = 0
        _GrassCutoff("Grass Cutoff", Float) = 0
        _StoneCutoff("Stone Cutoff", Float) = 0
        //_SnowCutoff("Snow Cutoff", Float) = 0

        [Header(Gradient Colours)]
        _SandColour("Sand Colour", Color) = (1, 1, 1, 1)
        _GrassColour("Grass Colour", Color) = (1, 1, 1, 1)
        _StoneColour("Stone Colour", Color) = (1, 1, 1, 1)
        _SnowColour("Snow Colour", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Name "ForwardLit"
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _SHADOWS_SOFT

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 gradientColour : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };


            CBUFFER_START(UnityPerMaterial)
                float _SandCutoff;
                float _GrassCutoff;
                float _StoneCutoff;
                //float _SnowCutoff;

                float4 _SandColour;
                float4 _GrassColour;
                float4 _StoneColour;
                float4 _SnowColour;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);

                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);

                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);

                //Adds height-dependent colour
                if (OUT.positionWS.y <= _SandCutoff)
                {
                    OUT.gradientColour = _SandColour;
                }
                else if (OUT.positionWS.y <= _GrassCutoff)
                {
                    OUT.gradientColour = _GrassColour;
                }
                else if (OUT.positionWS.y <= _StoneCutoff)
                {
                    OUT.gradientColour = _StoneColour;
                }
                else
                {
                    OUT.gradientColour = _SnowColour;
                }

                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                InputData inputData = (InputData)0;
                inputData.normalWS = normalize(IN.normalWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = IN.gradientColour.rgb;
                surfaceData.alpha = IN.gradientColour.a;

                float4 finalColour =  UniversalFragmentBlinnPhong(inputData, surfaceData);

                float luminance = 0.2126 * finalColour.r + 0.7152 * finalColour.g + 0.0722 * finalColour.b;
                
                if (luminance <= 0.5)
                {
                    finalColour = lerp(finalColour, IN.gradientColour, 0.1);
                }

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