Shader "BeresnevGames/Lit_VAT"
{
    Properties
    {
        _BaseMap ("Albedo", 2D) = "white"{}
        _Color ("Color", Color) = (1,1,1,1)

        _AdditionalMap ("Additional Map", 2D) = "white"{} //r - smoothness, g - metallic, b - normal.x, a - normal.y
        [Toggle(_NORMALMAP)] _UsingNormalMap("Using Normal Map", Float) = 0

        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness( "Smoothness", Range(0,1)) = 0.0

        _Brightness("Brightness", Range(0,2)) = 1

        [Space(40)]
         [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _Blend1 ("Blend mode", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _Blend2 ("Blend mode", Float) = 10
        [Enum(On, 1, Off,2)] _ZWrite ("ZWrite", Float) = 1
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

            #pragma shader_feature_local _NORMALMAP // ПРОТЕСТИТЬ
            #pragma shader_feature_local _ADDITIONALMAP
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_fog
            #pragma target 3.5

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Surface.hlsl"
            #include "CustomBRDF.hlsl"


            //to inputs include
            sampler2D _VAT;
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_AdditionalMap);
            SAMPLER(sampler_AdditionalMap);

            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            half4 _BaseMap_ST;
            half4 _AdditionalMap_ST;
            half _Brightness;
            half _Metallic;
            half _Smoothness;
            half _Frame;
            half3 _BoundBoxMin;
            half3 _BoundBoxMax;
            float4 _VAT_TexelSize;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                half3 normalOS : NORMAL;
                half4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                uint id : SV_VertexID;
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
            };


            float4 GetDataFromVAT(sampler2D VAT, half2 uv, half3 BoundBoxMin, half3 BoundBoxMax)
            {
                float4 relativePos = tex2Dlod(VAT, half4(uv, 0, 0));
                relativePos.xyz = relativePos.xyz * (BoundBoxMax - BoundBoxMin) + BoundBoxMin;
                return relativePos;
            }

            half3 RestoreNormalizedVectorZ(half2 v)
            {
                half z = sqrt(1 - v.x * v.x - v.y * v.y);
                return half3(v.x, v.y, z);
            }

            Varyings UniversalForwardVertex(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionWS.xyz = positionInputs.positionWS;

                //VAT
                float2 coords = float2(IN.id * 2.0, _Frame);
                float2 halfTexelSize = _VAT_TexelSize.xy * 0.5;
                float2 uv = float2(coords.x, coords.y) * _VAT_TexelSize.xy + halfTexelSize;
                float2 uv2 = uv +  float2(1.0 * _VAT_TexelSize.x,0);
                
                float4 firstVatData = GetDataFromVAT(_VAT, uv,_BoundBoxMin,_BoundBoxMax);
                half4 secondVatData = normalize(GetDataFromVAT(_VAT, uv2, _BoundBoxMin, _BoundBoxMax));
                
               
                normalInputs.normalWS = normalize(secondVatData);
                OUT.positionCS = TransformObjectToHClip(firstVatData);
                ///

                
                half3 viewDirWS = _WorldSpaceCameraPos - positionInputs.positionWS;
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
                OUT.color = _Color;
                OUT.shadowCoord = GetShadowCoord(positionInputs);

                return OUT;
            }

            half4 UniversalForwardFragment(Varyings IN): SV_Target
            {
                half4 result;
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 maskMap = 1;
                albedo *= _Color;
                albedo *= _Brightness;

                Surface surface;
                surface.metallic = _Metallic;
                surface.smoothness = _Smoothness;
                surface.normal = normalize(IN.normalWS);
                surface.color = albedo.rgb;
                surface.alpha = 1; //albedo.a;

                #if defined _ADDITIONALMAP
                maskMap = SAMPLE_TEXTURE2D(_AdditionalMap, sampler_AdditionalMap, IN.addUv);
                half smoothnessMask = maskMap.b;
                half metallicMask = maskMap.a;
                surface.metallic = _Metallic * metallicMask;
                surface.smoothness = _Smoothness * smoothnessMask;

                #if defined (_NORMALMAP)
                half x = maskMap.r;
                half y = maskMap.g;
                half z = sqrt(max(0, 1 - (x * x) - (y * y)));
                half3 normalTS = half3(x, y, z);
                half3x3 tangentToWorld = half3x3(IN.tangentWS.xyz, IN.bitangentWS.xyz, IN.normalWS.xyz);
                surface.normal = normalize(mul(normalTS, tangentToWorld));
                surface.viewDir = normalize(half3(IN.normalWS.w, IN.tangentWS.w, IN.bitangentWS.w));
                #else
                surface.viewDir = normalize(IN.viewDirWS);
               // half3 normalWS = normalize(IN.normalWS);
                #endif

                #else
                surface.viewDir = normalize(IN.viewDirWS);
                //half3 normalWS = normalize(IN.normalWS);
                #endif

                BRDF brdf = GetBRDF(surface);
                //light
                Light light = GetMainLight(IN.shadowCoord);
                half NoL = saturate(dot(surface.normal, light.direction));
                half3 lightColor = light.color * NoL * light.shadowAttenuation;
                lightColor *= DirectBRDF(surface, brdf, light);
                half3 indirectDiffuse = SampleSH(surface.normal);
                half3 go = EnvironmentBRDF(surface, brdf, indirectDiffuse, lightColor);

                half3 rV = reflect(-surface.viewDir, surface.normal);
                half4 probe = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, rV,
                                                     (1-surface.smoothness) * UNITY_SPECCUBE_LOD_STEPS);
                half3 envirReflection = DecodeHDREnvironment(probe, unity_SpecCube0_HDR);
                envirReflection *= surface.metallic + MIN_REFLECTIVITY;
                envirReflection *= _Color.rgb;
                result.rgb = go + envirReflection;
                result.a = surface.alpha;

                //FOG calc //TODO вынести
                half intensity = 1;
                #if (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
                float viewZ = -(mul(UNITY_MATRIX_V, float4(IN.positionWS,1)).z);
                float nearToFarZ = max(viewZ - _ProjectionParams.y, 0);
                half fogFactor = ComputeFogFactorZ0ToFar(nearToFarZ);
                intensity = ComputeFogIntensity(fogFactor);
                result.rgb = lerp(result.rgb, unity_FogColor, (1 - intensity));
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
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _VAT;
            CBUFFER_START(UnityPerMaterial)
            float3 _LightDirection;
             half _Frame;
            half3 _BoundBoxMin;
            half3 _BoundBoxMax;
            float4 _VAT_TexelSize;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                uint id : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

             float4 GetDataFromVAT(sampler2D VAT, half2 uv, half3 BoundBoxMin, half3 BoundBoxMax)
            {
                float4 relativePos = tex2Dlod(VAT, half4(uv, 0, 0));
                relativePos.xyz = relativePos.xyz * (BoundBoxMax - BoundBoxMin) + BoundBoxMin;
                return relativePos;
            }

            float4 GetShadowPositionHClip(Attributes IN)
            {
                //float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                  float2 coords = float2(IN.id * 2.0, _Frame);
                float2 halfTexelSize = _VAT_TexelSize.xy * 0.5;
                float2 uv = float2(coords.x, coords.y) * _VAT_TexelSize.xy + halfTexelSize;
                float2 uv2 = uv +  float2(1.0 * _VAT_TexelSize.x,0);
                
                float4 firstVatData = GetDataFromVAT(_VAT, uv,_BoundBoxMin,_BoundBoxMax);
                float4 positionCS = TransformObjectToHClip(firstVatData);
                #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                return positionCS;
            }

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = GetShadowPositionHClip(input);
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }

    CustomEditor "BeresnevLitShaderEditor"

}