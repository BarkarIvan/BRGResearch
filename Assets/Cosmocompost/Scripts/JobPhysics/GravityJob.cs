using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

[StructLayout(LayoutKind.Sequential)]
[BurstCompile(OptimizeFor = OptimizeFor.Performance, FloatMode = FloatMode.Fast, CompileSynchronously = true, FloatPrecision = FloatPrecision.Low, DisableSafetyChecks = true)]
public struct GravityJob : IJobParallelFor
{
    public float DeltaTime;

    public float3 Gravity;
    
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<float3> Velocities;
    
    [ReadOnly]
    public NativeArray< int > Sleeping;
    
    public void Execute( int index )
    {
        if (Sleeping[index] > 0) return;
        Velocities[index] += Gravity * DeltaTime;
    }
}
