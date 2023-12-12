Shader "BeresnevGames/SimpleUnlitInstanced"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" 
        }


        Pass
        {
            Name "Unlit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            struct Attributes
            {
                float4 positionOS : POSITION;
                half3 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                half3 color : COLOR;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            UNITY_INSTANCING_BUFFER_START(Props)
                half4 _Color;
            UNITY_INSTANCING_BUFFER_END(Props)

            #define Color    UNITY_ACCESS_INSTANCED_PROP(Props, _Color)


            Varyings vert(Attributes IN)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, o);
                o.color = IN.color;
                o.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                o.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return o;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                col *= Color;
                return col;
            }
            ENDHLSL
        }
    }
}