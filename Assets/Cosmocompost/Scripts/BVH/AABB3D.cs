using System;
using Unity.Mathematics;

namespace BVH
{
    [ Serializable ]
    public struct AABB3D
    {
        public float3 LowerBound;
        public float3 UpperBound;

        public float3 Center => LowerBound + ( UpperBound - LowerBound ) * .5f;

        public AABB3D( float3 lowerBound, float3 upperBound )
        {
            LowerBound = lowerBound;
            UpperBound = upperBound;
        }

        public AABB3D Union( AABB3D other ) =>
            new()
            {
                LowerBound = math.min( other.LowerBound, LowerBound ),
                UpperBound = math.max( other.UpperBound, UpperBound )
            };

        public void Expand( float amount )
        {
            LowerBound -= amount;
            UpperBound += amount;
        }

        public float Area()
        {
            float3 d = UpperBound - LowerBound;

            return 2.0f * ( d.x * d.y + d.y * d.z + d.z * d.x );
        }
    }
}

