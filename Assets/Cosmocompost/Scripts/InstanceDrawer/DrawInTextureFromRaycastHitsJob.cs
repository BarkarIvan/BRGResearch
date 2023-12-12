using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
[StructLayout(LayoutKind.Sequential)]
public struct DrawInTextureFromRaycastHitsJob : IJobParallelFor
{
    [ReadOnly] 
    public NativeArray<RaycastHit> Hits;

    [NativeDisableContainerSafetyRestriction]
    public NativeArray<half4> Pixels;

    [ReadOnly] 
    public bool IsErase;
    [ReadOnly] 
    public half Amount;

    [ReadOnly] 
    public WorldData WorldData;
    [ReadOnly] 
    public int2 TextureWidthHeight;


    public void Execute(int index)
    {
        if (Hits[index].normal == Vector3.zero) return;

        Vector3 worldCenter = WorldData.WorldPosition + WorldData.WorldSize / 2;
        Vector3 positionWS = Hits[index].point;
        Vector3 offsetPosition = positionWS - (worldCenter - WorldData.WorldSize * 0.5f);
        Vector3 relativePosition = new(offsetPosition.x / WorldData.WorldSize.x,
            offsetPosition.y / WorldData.WorldSize.y, offsetPosition.z / WorldData.WorldSize.z);
        int2 pixelCoord = new((int)(relativePosition.x * TextureWidthHeight.x),
            (int)(relativePosition.z * TextureWidthHeight.y));
        if (pixelCoord.x < 0 || pixelCoord.x >= TextureWidthHeight.x || pixelCoord.y < 0 ||
            pixelCoord.y >= TextureWidthHeight.y) return;
        int pixelIndex = pixelCoord.y * TextureWidthHeight.x + pixelCoord.x;

        //DRAW
        half4 originalColor = Pixels[pixelIndex];
        half amount = IsErase ? (half)0 : Amount;
        Pixels[pixelIndex] = new half4(amount, originalColor.y, originalColor.z, (half)1);
    }
}