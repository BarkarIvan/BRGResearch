Shader "BeresnevGames/FullScreenGrid_BloomSoftAdd"
{
    Properties {}

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline" "Queue"="Geometry"
        }

        Pass
        {

            ZTest Always
            Cull Off
            Blend One One

            HLSLPROGRAM
            #pragma vertex BackgroundVertex
            #pragma fragment BackgroundFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                half3 color :COLOR;
            };

            TEXTURE2D(_BloomTexture);
            SAMPLER(sampler_BloomTexture);
            
            Varyings BackgroundVertex(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = float4((IN.positionOS.xy + 0.5) * 2.0 - 1.0, UNITY_NEAR_CLIP_VALUE, 1.0);
                half2 center = IN.uv;
                //if (_ProjectionParams.x < 0) center.y = 1 - center.y;
                half2 uv = (IN.positionOS.xy + 0.5);
                half3 color = SAMPLE_TEXTURE2D_LOD(_BloomTexture, sampler_BloomTexture, center, 0).rgb;
                half c = color.r + color.g + color.b;
                half threshold = step(0.0001, c);
                OUT.positionCS.xyz *= threshold;
                if (_ProjectionParams.x < 0) uv.y = 1 - uv.y;

                OUT.uv = uv;
                return OUT;
            }

            half4 BackgroundFragment(Varyings IN) : SV_Target
            {
                half4 result = SAMPLE_TEXTURE2D(_BloomTexture, sampler_BloomTexture, IN.uv);
                return result;
            }
            ENDHLSL
        }
    }
}