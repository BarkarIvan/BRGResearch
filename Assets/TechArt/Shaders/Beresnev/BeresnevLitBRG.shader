Shader "BeresnevGames/Lit_BRG"
{
    Properties
    {
        _BaseMap ("Albedo", 2D) = "white"{}
        _BaseColor ("Color", Color) = (1,1,1,1)

        _AdditionalMap ("Additional Map", 2D) = "white"{} //r - smoothness, g - metallic, b - normal.x, a - normal.y
        [Toggle(_NORMALMAP)] _UsingNormalMap("Using Normal Map", Float) = 0
        _NormalMapScale("Normal Map Scale", Range(1,3)) = 1

        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness( "Smoothness", Range(0,1)) = 0.0

        _Brightness("Brightness", Range(0,2)) = 1

        [Toggle(_USEALPHACLIP)] _UseAlphaClip ("Use Alpha Clip", Float) = 0
        _AlphaClip ("ClipAlha", Range(0,1)) = 0

        [HideInInspector] _CustomData("CustomData", Vector) = (1,1,1,1)

        [Space(40)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Int) = 2
        [Enum(UnityEngine.Rendering.BlendMode)] _Blend1 ("Blend mode", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _Blend2 ("Blend mode", Float) = 0
        [Enum(Off,0,On,1)] _ZWrite ("ZWrite", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline" "Queue"="Geometry"
        }

        Cull [_Cull]
        Blend [_Blend1] [_Blend2]
        ZWrite [_ZWrite]

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex UniversalForwardVertex
            #pragma fragment UniversalForwardFragment


            #pragma multi_compile_fog

            // #pragma prefer_hlslcc gles
            //#pragma exclude_renderers d3d11_9x

            #pragma shader_feature_local _NORMALMAP // ПРОТЕСТИТЬ
            #pragma shader_feature_local _ADDITIONALMAP
            #pragma shader_feature _USEALPHACLIP
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_instancing

            #pragma multi_compile _ DOTS_INSTANCING_ON
            #if UNITY_PLATFORM_ANDROID || UNITY_PLATFORM_WEBGL || UNITY_PLATFORM_UWP
             #pragma target 3.5 DOTS_INSTANCING_ON
            #else
            #pragma target 4.5 DOTS_INSTANCING_ON
            #endif

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Surface.hlsl"
            #include "Lighting.hlsl"
            #include "CustomBRDF.hlsl"


            //to inputs include

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_AdditionalMap);
            SAMPLER(sampler_AdditionalMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _CustomData;
                half4 _BaseMap_ST;
                half4 _AdditionalMap_ST;
                half _Brightness;
                half _Metallic;
                half _Smoothness;
                half _AlphaClip;
              half _NormalMapScale;
            CBUFFER_END

          


            #if defined(UNITY_DOTS_INSTANCING_ENABLED)
            UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
               UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
               UNITY_DOTS_INSTANCED_PROP(float4, _CustomData)
            UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
             
            #define _BaseColor UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseColor);
            #define _CustomData UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _CustomData);

            #endif

            struct Attributes
            {
                float3 positionOS : POSITION;
                half3 normalOS : NORMAL;
                half4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 addUv : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                #if defined (_ADDITIONALMAP) && !defined(_NORMALMAP)
                half3 normalWS : NORMAL;
                half3 viewDirWS : TEXCOORD4;
                #elif defined(_ADDITIONALMAP) && defined(_NORMALMAP)
                half4 normalWS : NORMAL; //xyz, w - viewDir.x
                half4 tangentWS : TEXCOORD3; ////xyz, w - viewDir.y
                half4 bitangentWS : TEXCOORD4; //xyz, w - viewDir.z
                #else
                half3 normalWS : NORMAL;
                half3 viewDirWS : TEXCOORD4;
                #endif
                float4 shadowCoord : TEXCOORD5;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            Varyings UniversalForwardVertex(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionWS.xyz = positionInputs.positionWS;
                OUT.positionCS = positionInputs.positionCS;
                half3 viewDirWS = normalize(_WorldSpaceCameraPos - positionInputs.positionWS);
                #ifdef _NORMALMAP
                OUT.normalWS = half4(normalInputs.normalWS, viewDirWS.x);
                half sign = IN.tangentOS.w;
                half3 tangentWS = TransformObjectToWorldDir(IN.tangentOS.xyz);
                half3 bitangentWS = cross(normalInputs.normalWS.xyz, normalInputs.tangentWS.xyz) * sign;
                OUT.tangentWS = half4(tangentWS, viewDirWS.y);
                OUT.bitangentWS = half4(bitangentWS, viewDirWS.z);
                #else
                OUT.viewDirWS = viewDirWS;
                OUT.normalWS = normalInputs.normalWS;
                #endif

                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.addUv = TRANSFORM_TEX(IN.uv, _AdditionalMap);
                OUT.color = IN.color * _BaseColor;
                OUT.shadowCoord = GetShadowCoord(positionInputs);

                return OUT;
            }

            half4 UniversalForwardFragment(Varyings IN): SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                half4 result;
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 additionalMaps = 1;
                albedo *= IN.color;
                albedo *= _Brightness;

                Surface surface;
                surface.metallic = _Metallic;
                surface.smoothness = _Smoothness;
                surface.normal = normalize(IN.normalWS);
                surface.color = albedo.rgb;
                surface.alpha = albedo.a;

                #if defined _ADDITIONALMAP
                additionalMaps = SAMPLE_TEXTURE2D(_AdditionalMap, sampler_AdditionalMap, IN.addUv);
                half smoothnessMask = additionalMaps.b;
                half metallicMask = additionalMaps.a;
                surface.metallic = _Metallic * metallicMask;
                surface.smoothness = _Smoothness * smoothnessMask;

                #if defined (_NORMALMAP)
                half x = additionalMaps.r;
                half y = additionalMaps.g;
                half z = sqrt(max(0, 1 - (x * x) - (y * y)));
                half3 normalTS = normalize(half3(x, y, z)) * 2.0 - 1.0;
                normalTS.xy *= _NormalMapScale;
                IN.tangentWS = normalize(IN.tangentWS);
                IN.normalWS = normalize(IN.normalWS);
                IN.bitangentWS = normalize(IN.bitangentWS);
                half3x3 tangentToWorld = half3x3(IN.tangentWS.xyz, IN.bitangentWS.xyz, IN.normalWS.xyz);
                surface.normal = normalize(mul(normalTS, tangentToWorld));
                surface.viewDir = normalize(half3(IN.normalWS.w, IN.tangentWS.w, IN.bitangentWS.w));
                #else
                surface.viewDir = normalize(IN.viewDirWS);
                #endif

                #else
                surface.viewDir = normalize(IN.viewDirWS);
                #endif

                #if defined (_USEALPHACLIP)
                clip(surface.alpha - _AlphaClip);
                #endif

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
                envirReflection *= surface.color;
                result.rgb = go + envirReflection;
                result.a = surface.alpha;

                //FOG
                #if (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
                result.rgb = CalculateFog(result, IN.positionWS);
                #endif

                return result;
            }
            ENDHLSL
        }

        //to shadowcaster hlsl

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode"="ShadowCaster"
            }

            ColorMask 0
            ZTest LEqual

            HLSLPROGRAM
            // #pragma prefer_hlslcc gles
            //   #pragma exclude_renderers d3d11_9x
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing

            #pragma multi_compile _ DOTS_INSTANCING_ON

            #if UNITY_PLATFORM_ANDROID || UNITY_PLATFORM_WEBGL || UNITY_PLATFORM_UWP
        #pragma target 3.5 DOTS_INSTANCING_ON
            #else
            #pragma target 4.5 DOTS_INSTANCING_ON
            #endif


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            CBUFFER_START(UnityPerMaterial)
                float3 _LightDirection;
            CBUFFER_END

            #if defined(UNITY_DOTS_INSTANCING_ENABLED)
            UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
               UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
               UNITY_DOTS_INSTANCED_PROP(float4, _CustomData)
            UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
             
            #define _BaseColor UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseColor);
            #define _CustomData UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _CustomData);

            #endif

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float4 positionCS = TransformWorldToHClip(positionWS);
                #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                return positionCS;
            }

            Varyings ShadowPassVertex(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                OUT.positionCS = GetShadowPositionHClip(IN);
                return OUT;
            }

            half4 ShadowPassFragment(Varyings IN) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                return 0;
            }
            ENDHLSL
        }
    }

    CustomEditor "BeresnevLitShaderEditor"

}