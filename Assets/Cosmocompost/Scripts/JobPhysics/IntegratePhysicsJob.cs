using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[StructLayout(LayoutKind.Sequential)]
[BurstCompile]
public struct IntegratePhysicsJob : IJobParallelFor
{
    public float DeltaTime;

    [ReadOnly]
    public NativeArray<float3> Velocities;

    [ReadOnly] 
    public NativeArray<RaycastHit> Hits;
  
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<int> Sleeping;

    [NativeDisableParallelForRestriction]
    public NativeArray<float3> Positions;

    [NativeDisableParallelForRestriction]
    public NativeArray<quaternion> Rotations;

    [NativeDisableParallelForRestriction]
    public NativeArray<quaternion> RotationsIncrement;


    public void Execute(int index)
    {
        if (Sleeping[index] > 0)
        {
           return;
        }
        
        if (Hits[index].normal != Vector3.zero)
        {
            Positions[index] = Hits[index].point + new Vector3(0, 0.1f, 0);
        }
        else
        {
            Positions[index] += (Velocities[index] * DeltaTime);
            Rotations[index] = math.mul(Rotations[index], RotationsIncrement[index]);
        }

        if (Positions[index].y < -300)
        {
            Sleeping[index] = 1;
        }
    }
}