Shader "Custom/CheckerBoard"
{
    Properties
    { 
    }
 
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

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
                float4 positionOS   : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };            

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);

                return OUT;
            }
        
            half4 frag(Varyings IN) : SV_Target
            {
                float scale = 1;
                uint3 worldIntPos = uint3(abs(IN.positionWS.xyz * scale));
                bool white = (worldIntPos.x & 1) ^ (worldIntPos.y & 1) ^ (worldIntPos.z & 1);
                half4 colour = white ? half4(1, 1, 1, 1) : half4(0.5, 0.5, 0.5, 1);

                InputData inputData = (InputData)0;
                inputData.normalWS = normalize(IN.normalWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = colour.rgb;
                surfaceData.alpha = colour.a;

                float4 finalColour =  UniversalFragmentBlinnPhong(inputData, surfaceData);
                float luminance = 0.2126 * finalColour.r + 0.7152 * finalColour.g + 0.0722 * finalColour.b;
                
                if (luminance <= 0.5)
                {
                    finalColour = lerp(finalColour, colour, 0.1);
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
