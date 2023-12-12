using BVH;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct CullVoxelJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<float4> FrustumPlanes;

    [ReadOnly]
    public NativeArray<AABB3D> voxelBounds;

    public NativeArray<bool> voxelVisibility;


    public void Execute(int index)
    {
        AABB3D aabb = voxelBounds[index];
        voxelVisibility[index] = TestPlaneAABB(aabb, FrustumPlanes);
    }
    
    private bool TestPlaneAABB(AABB3D aabb, NativeArray<float4> frustumPlanes)
    {
        for (int i = 0; i < 6; i++)
        {
            float4 plane = frustumPlanes[i];
            float3 center = aabb.Center;
            float3 extents = aabb.UpperBound - center;

            float distance = math.dot(plane.xyz, center) + plane.w;
            float radius = math.dot(extents, math.abs(plane.xyz));
            if (distance < -radius) 
                return false;
        }
        return true;
    }
}