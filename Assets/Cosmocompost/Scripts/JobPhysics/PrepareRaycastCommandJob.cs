using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[StructLayout(LayoutKind.Sequential)]
[BurstCompile(OptimizeFor = OptimizeFor.Performance, FloatMode = FloatMode.Fast, CompileSynchronously = true,
    FloatPrecision = FloatPrecision.Low, DisableSafetyChecks = true)]
public struct PrepareRaycastCommandJob : IJobParallelFor
{
    public float DeltaTime;

    [NativeDisableParallelForRestriction] 
    public NativeArray<RaycastCommand> Raycasts;
    
    [ReadOnly] 
    public NativeArray<float3> Velosities;
   
    [ReadOnly] 
    public NativeArray<float3> Positions;


    public void Execute(int index)
    {
        var Params = new QueryParameters()
        {
            hitTriggers = QueryTriggerInteraction.Ignore,
            layerMask = -5
        };

        float distance = math.length(Velosities[index] * DeltaTime); // * Distance;
        Raycasts[index] = new RaycastCommand(Positions[index], math.normalizesafe(Velosities[index]), Params, distance);
    }
}