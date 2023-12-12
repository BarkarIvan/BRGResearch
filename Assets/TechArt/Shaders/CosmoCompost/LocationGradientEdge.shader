Shader "Cosmocompost/LocationEdge"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Step1 ("Step 1", Range(0,1)) = 0
        _Step2 ("Step 2", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        
        Pass
        {
            Name "SimpleUnlit"
            
            Blend DstColor Zero
            ZWrite Off
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            struct Attributes
            {
                float4 positionOS : POSITION;
                half3 color : COLOR;
            };

            struct Varyings
            {
                half3 color : COLOR;
                float4 positionCS : SV_POSITION;
            };
            
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            half _Step1;
            half _Step2;
            CBUFFER_END


            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.color = IN.color;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 col;
                col.rgb = _Color;// * (smoothstep(_Step1,_Step2,1.0 - IN.color));
                col.a  = 1.0;
                return col;
            }
            ENDHLSL
        }
    }
}