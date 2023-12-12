
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[StructLayout(LayoutKind.Sequential)]
[BurstCompile(OptimizeFor = OptimizeFor.Performance, FloatMode = FloatMode.Fast, CompileSynchronously = true, FloatPrecision = FloatPrecision.Low, DisableSafetyChecks = true)]
public struct CalculateCollisionResponseBounceJob : IJobParallelFor
{

    [ ReadOnly ]
    public NativeArray< RaycastHit > Hits;
    

    [NativeDisableParallelForRestriction]
    public NativeArray< float3 > Velocities;

    [NativeDisableParallelForRestriction]
    public NativeArray<quaternion> RotationIncrements;
    
    [NativeDisableParallelForRestriction]
    public NativeArray< int > Sleeping;
    
    
    public void Execute( int index )
    {
        if (Hits[index].normal != Vector3.zero)
        {
            if (math.length(Velocities[index]) <= 2f)
            {
                Velocities[index] = Vector3.zero;
                Sleeping[index] = 1;
            }
            else
            {
                Velocities[index] = math.reflect(Velocities[index], Hits[index].normal);
                
                var angle = Vector3.Angle(Hits[index].normal, Vector3.up);
                angle /= 180f;
                var factor = math.lerp(0.2f, 1f, angle);
                
                Velocities[index] *= factor;
                
                quaternion originalIncrement = RotationIncrements[index];

               RotationIncrements[index] = math.slerp(quaternion.identity, originalIncrement, factor);

            }
        }
    }

    private float Angle2(float3 a, float3 b)
    {
        float3 abm = a * math.length(b);
        float3 bam = b * math.length(a);
        float angle = 2 * math.atan2(math.length(abm - bam), math.length(abm + bam));
        return math.degrees(angle);

    }
}
