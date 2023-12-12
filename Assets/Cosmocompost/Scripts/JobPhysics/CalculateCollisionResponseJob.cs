
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile]
public struct CalculateCollisionResponseJob : IJobParallelFor
{

    [ ReadOnly ]
    public NativeArray< RaycastHit > Hits;

    [NativeDisableContainerSafetyRestriction]
    public NativeArray< Vector3 > Velocities;

    [ NativeDisableContainerSafetyRestriction ]
    public NativeArray< Quaternion > Rotation;
    
    [ NativeDisableContainerSafetyRestriction ]
    public NativeArray< Quaternion > TargetRotations;
    [ NativeDisableContainerSafetyRestriction ]
    public NativeArray< Vector3 > TargetPositions;
    
    [ NativeDisableContainerSafetyRestriction ]
    public NativeArray< Vector3 > Positions;
    
    
   
    public NativeArray< int > Sleeping;
    
    
    public void Execute( int index )
    {
        //если нормаль  не равно 0 - была коллизия
        if ( Hits[ index ].normal != Vector3.zero )
        {
            //если велосити мала - усыпляем
            if ( Velocities[ index ].magnitude <= 1f )
            {
                Velocities[index] = Vector3.zero;
                Sleeping[ index ] = 1; // ++ for timer
            }
            else
            {
                Vector3 movementDir = Velocities[ index ].normalized;
                Vector3 point = Hits[ index ].point;
                Vector3 collisionVector = point - Positions[ index ];
                Vector3 collisionDir = collisionVector.normalized;
                Vector3 mm = Vector3.Cross( movementDir, collisionDir );
                 // Vector3 rotationDirection = Vector3.Cross( Hits[ index ].normal, Vector3.up );
                 //  Vector3 s = Vector3.Cross( collisionVector, Hits[ index ].normal ).normalized;
                 float d = Vector3.Dot( collisionDir, movementDir );
                float angle = 2f * Velocities[index].magnitude * Mathf.Sign( d );
                Quaternion rotationChange = Quaternion.AngleAxis( angle, mm );
                TargetRotations[ index ] = rotationChange * Rotation[ index ];
                TargetPositions[ index ] = Positions[ index ] - ( rotationChange * collisionVector ) + collisionVector;
                
                // простой баунс
                Velocities[ index ] += Vector3.Reflect( Velocities[ index ], Hits[ index ].normal );
                Velocities[ index ] += mm;
                var NormalAndUpAngle = Vector3.Angle( Hits[ index ].normal, Velocities[index] );
                var lerp = NormalAndUpAngle / 180f;
                Velocities[ index ] *= Mathf.Lerp( 0.2f, 0.5f, lerp );
            }
        }
        else
        {
            Sleeping[ index ] = 0; // 15 as timer
            if ( Rotation[ index ] != TargetRotations[ index ] )
            {
                float lerpBase = 0.03f; 
                float lerpFactor = Mathf.Clamp(lerpBase + Velocities[index].magnitude * 0.01f, lerpBase, 0.5f);
                Rotation[ index ] = Quaternion.Slerp( Rotation[index], TargetRotations[index], lerpFactor);
                Positions[ index ] = Vector3.Lerp( Positions[ index ], TargetPositions[ index ], lerpFactor );
               
            }
        }
    }
}
