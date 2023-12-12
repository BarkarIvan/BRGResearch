Shader "BeresnevGames/KawaseBlur"
{
    Properties
    {
        _MainTex("MainTex",  2D) = "white"{}
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline" "Queue"="Geometry"
        }


        Pass
        {

            Name "Blur DownSample"
            HLSLPROGRAM
            #pragma vertex blurVertex
            #pragma fragment blurFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "KawaseBlurFunctions.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            sampler2D _MainTex;
            half2 _HalfPixel;
            half _BlurOffset;

            Varyings blurVertex(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 blurFragment(Varyings IN) : SV_Target
            {
                half4 result = downsample(IN.uv, _HalfPixel, _MainTex, _BlurOffset);
                return result; // col;
            }
            ENDHLSL
        }

        Pass
        {

            Name "Blur Upsample"

            HLSLPROGRAM
            #pragma vertex blurVertex
            #pragma fragment blurFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "KawaseBlurFunctions.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            sampler2D _MainTex;
            
            half2 _HalfPixel;
            half _BlurOffset;

            Varyings blurVertex(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 blurFragment(Varyings IN) : SV_Target
            {
                half4 result = upsample(IN.uv, _HalfPixel, _MainTex, _BlurOffset);
                return result;
            }
            ENDHLSL
        }
    }
}