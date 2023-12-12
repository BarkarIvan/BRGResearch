using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile]
public struct PrepareBoxCastCommandJob : IJobParallelFor
{
    public float DeltaTime;

    [ ReadOnly ] 
    public NativeArray< Vector3 > Velocities;

    [ WriteOnly ]  [NativeDisableContainerSafetyRestriction]
    public NativeArray< BoxcastCommand > Raycasts;

    [ReadOnly]
    public NativeArray< Vector3 > Positions;

    [ ReadOnly ]
    public Vector3 HalfExtents;

    [ ReadOnly ]
    public NativeArray< Quaternion > Rotations;



    public void Execute( int index )
    {
        float distance =( Velocities[ index ].normalized * DeltaTime ).magnitude;
        QueryParameters p = new QueryParameters( -5, false, QueryTriggerInteraction.Ignore, true );
        Raycasts[ index ] = new BoxcastCommand(Positions[index], HalfExtents, Rotations[index],Velocities[index], p , distance);

    }
}
