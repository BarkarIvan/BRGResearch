#ifndef CUSTOM_SURFACE_INCLUDED
#define CUSTOM_SURFACE_INCLUDED

struct Surface
{
    half3 normal;
    float3 viewDir;
    half3 color;
    half alpha;
    half metallic;
    half smoothness;
};

#endif