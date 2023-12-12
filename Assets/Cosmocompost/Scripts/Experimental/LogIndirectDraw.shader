Shader "Hidden/LogIndirectDraw"
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
            Name "UnlitIndirect"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            
            Cull Off
                
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"

            
          
            struct Attributes
            {
                float4 positionOS : POSITION;
                half3 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                half3 color : COLOR;
                float4 positionCS : SV_POSITION;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            half4 _MainTex_ST;
            half4 _Color;


            StructuredBuffer<float3> positionBuffer;
            //StructuredBuffer<float3> vertexBuffer;
            //StructuredBuffer<uint> indexBuffer;


            Varyings vert(Attributes IN, uint svVertexID: SV_VertexID, uint svInstanceID : SV_InstanceID)
            {
                InitIndirectDrawArgs(0);
                Varyings o;
                IndirectDrawIndexedArgs args;
                uint cmdID = GetCommandID(0);
                GetIndirectDrawArgs(args, unity_IndirectDrawArgs, cmdID);
                uint vtxId = GetIndirectVertexID_Base(args, svVertexID);//svVertexID + args.startIndex;
                uint instID = GetIndirectInstanceID_Base(args, svInstanceID);//svInstanceID + args.startInstance;
                //uint index = indexBuffer[vtxId];
                //float3 vtxPosition = vertexBuffer[index].xyz;
                float3 instncePosition = positionBuffer[instID].xyz;
                o.positionCS = TransformObjectToHClip(IN.positionOS + instncePosition);
                o.color = IN.color;
                o.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return o;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                col *= _Color;
                return col;
            }
            ENDHLSL
        }
    }
}