#ifndef  CUSTOM_LIGHTING
#define  CUSTOM_LIGHTING

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

half3 CalculateFog(half4 color, float3 positionWS)
{
    float viewZ = -(mul(UNITY_MATRIX_V, float4(positionWS,1)).z);
    float nearToFarZ = max(viewZ - _ProjectionParams.y, 0);
    half fogFactor = ComputeFogFactorZ0ToFar(nearToFarZ);
    half intensity = ComputeFogIntensity(fogFactor);
    return lerp(color.rgb, unity_FogColor.rgb, (1 - intensity));
}

half3 GetDiffuseLighting(Light light, Surface surface)
{
    half NoL = saturate(dot(surface.normal, light.direction));
    return light.color * lerp(1, _SubtractiveShadowColor.xyz, 1.0 - NoL) * lerp(1, _SubtractiveShadowColor.xyz,1.0 - light.shadowAttenuation);
}

half3 GetReflectionProbe(Surface surface)
{
    half3 rV = reflect(-surface.viewDir, surface.normal);
    half4 probe = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, rV,
        (1-surface.smoothness) * UNITY_SPECCUBE_LOD_STEPS);
    half3 envirReflection = DecodeHDREnvironment(probe, unity_SpecCube0_HDR);
    return envirReflection;
}

#endif