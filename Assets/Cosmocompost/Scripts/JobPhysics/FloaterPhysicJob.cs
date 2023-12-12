using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct FloaterPhysicJob : IJobParallelFor
{
    public float DeltaTime;
    public float Waterlevel;
    public float BounceDamp;

    public Vector3 Gravity;
  
    [NativeDisableContainerSafetyRestriction]
    public NativeArray< Vector3 > Velocities;
    [ReadOnly]
    public NativeArray< int > Sleeping;
    [ReadOnly]
    public NativeArray< Vector3 > Positions;

    [ ReadOnly ]
    public NativeArray< float > Drag;
    
    
    public void Execute( int index )
    {
       // if ( Sleeping[ index ] > 0 )
      //  {
           // Velocities[ index ] = new Vector3( 0, 0, 0 );
          //  return;
       // }
       
       //водный дра пусть тут пока
       
       
       
        if ( Positions[ index ].y < Waterlevel )
        {
            float f = 1f - ( ( Positions[ index ].y - Waterlevel ) );
            float velocityY = Velocities[ index ].y;
            Velocities[ index ] -= (Gravity * ( f - velocityY * BounceDamp)) * DeltaTime;
        }
        
        Velocities[ index ] *= 1.0f - Drag[index] * DeltaTime;
        
    }
}
