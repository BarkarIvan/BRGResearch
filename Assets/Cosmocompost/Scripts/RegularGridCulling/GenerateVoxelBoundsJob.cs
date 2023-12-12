using BVH;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct GenerateVoxelBoundsJob : IJobParallelFor
{
    [ReadOnly]
    public int3 gridSize;

    [ReadOnly]
    public float3 voxelSize;

    [ReadOnly]
    public float3 gridOrigin;
    //[]
    public NativeArray<AABB3D> voxelBounds;
    
    
    public void Execute(int index)
    {
        int z = index / (gridSize.x * gridSize.y);
        int indexXY = index % (gridSize.x * gridSize.y);
        int y = indexXY / gridSize.x;
        int x = indexXY % gridSize.x;

        float3 lowerBound = gridOrigin + new float3(x, y, z) * voxelSize;
        float3 upperBound = lowerBound + voxelSize;

        voxelBounds[index] = new AABB3D(lowerBound, upperBound);
    }
}
