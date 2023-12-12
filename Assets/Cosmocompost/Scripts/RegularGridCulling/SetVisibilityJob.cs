using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct SetVisibilityJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<bool> voxelVisibility;

    [ReadOnly]
    public NativeArray<int> InstanceToVoxel;

    [NativeDisableParallelForRestriction] 
    public NativeArray<bool> positionVisibility;

    public void Execute(int index)
    {
        int voxelIndex = InstanceToVoxel[index];
        positionVisibility[index] = voxelVisibility[voxelIndex];

    }
}