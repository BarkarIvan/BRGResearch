

using System.Runtime.InteropServices;
using BrgContainer.Runtime;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

[StructLayout(LayoutKind.Sequential)]
[BurstCompile]//(OptimizeFor = OptimizeFor.Performance, FloatMode = FloatMode.Fast, CompileSynchronously = true, FloatPrecision = FloatPrecision.Low, DisableSafetyChecks = true)]
public struct UpdateInstanceTRSJob : IJobParallelFor
{
    [ReadOnly] 
    public NativeArray<float3> Positions;
    
    [ReadOnly]
    public NativeArray<half3> Scale;
    
    [ReadOnly] 
    public NativeArray<quaternion> Rotations;
    
    [WriteOnly, NativeDisableParallelForRestriction]
    public BatchInstanceDataBuffer InstanceDataBuffer;
    
    public void Execute(int index)
    {
        var pos = Positions[index];
        var rotation = Rotations[index];
        var scale = Scale[index];
        
        InstanceDataBuffer.SetTRS(index, pos, rotation, scale);
    }
}
