Shader "BeresnevGames/Particles_Lit_Alpha"
{
    Properties
    {
        _BaseMap ("Albedo", 2D) = "white"{}
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness( "Smoothness", Range(0,1)) = 0.0

    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline" "Queue"="Geometry"
        }

        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex UniversalForwardVertex
            #pragma fragment UniversalForwardFragment

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Surface.hlsl"
            #include "Lighting.hlsl"
            #include "CustomBRDF.hlsl"


            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
         
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseMap_ST;
                half _Metallic;
                half _Smoothness;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                half3 normalOS : NORMAL;
                half4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 addUv : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                half3 normalWS : NORMAL;
                half3 viewDirWS : TEXCOORD4;
                float4 shadowCoord : TEXCOORD5;
                half4 color : COLOR;
            };


            Varyings UniversalForwardVertex(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionWS.xyz = positionInputs.positionWS;
                OUT.positionCS = positionInputs.positionCS;
                half3 viewDirWS = _WorldSpaceCameraPos - positionInputs.positionWS;
               
                OUT.viewDirWS = viewDirWS;
                OUT.normalWS = normalInputs.normalWS;


                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.color = IN.color;
                OUT.shadowCoord = GetShadowCoord(positionInputs);

                return OUT;
            }

            half4 UniversalForwardFragment(Varyings IN): SV_Target
            {
                half4 result;
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                albedo *= IN.color;

                Surface surface;
                surface.metallic = _Metallic;
                surface.smoothness = _Smoothness;
                surface.normal = normalize(IN.normalWS);
                surface.color = albedo.rgb;
                surface.alpha = albedo.a;
                
                surface.viewDir = normalize(IN.viewDirWS);
                
                BRDF brdf = GetBRDF(surface);
                //light
                Light light = GetMainLight(IN.shadowCoord);
                half3 lightColor = GetDiffuseLighting(light, surface);
                lightColor *= DirectBRDF(surface, brdf, light);
                half3 indirectDiffuse = SampleSH(surface.normal);
                half3 go = EnvironmentBRDF(surface, brdf, indirectDiffuse, lightColor);

               //reflectionProbe
                half3 envirReflection = GetReflectionProbe(surface);
                envirReflection *= surface.metallic + MIN_REFLECTIVITY;
                result.rgb = go + envirReflection;
                result.a = surface.alpha;

                //FOG
               // #if (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
               // result.rgb = CalculateFog(result, IN.positionWS);
               // #endif

                return result;
            }
            ENDHLSL
        }
    }

}