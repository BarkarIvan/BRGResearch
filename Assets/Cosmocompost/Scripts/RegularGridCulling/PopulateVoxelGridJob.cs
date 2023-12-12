using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Serialization;

[BurstCompile]
public struct PopulateVoxelGridJob : IJobParallelFor
{
    [ReadOnly][NativeDisableParallelForRestriction]
    public NativeArray<TextureItemData> ItemData;

    [WriteOnly]
    public NativeArray<int> InstanceToVoxel;

    public int3 gridSize;
    public float3 voxelSize;
    public float3 gridOrigin;

    private int FlattenVoxelIndex(int3 voxelIndex, int3 gridSize)
    {
        return voxelIndex.z * gridSize.y * gridSize.x + voxelIndex.y * gridSize.x + voxelIndex.x;
    }

    public void Execute(int index)
    {
        float3 position = ItemData[index].Position;
        int3 voxelIndex = (int3)math.floor((position - gridOrigin) / voxelSize);
        int linearIndex = FlattenVoxelIndex(voxelIndex, gridSize);
        InstanceToVoxel[index] = linearIndex;
    }
}