using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace BVH
{
    public unsafe struct FrustumCuller :IDisposable
    {
        public NativeArray< Plane > planes;

        public FrustumCuller( NativeArray< Plane > inputPlanes )
        {
            planes = new NativeArray< Plane >( inputPlanes.Length, Allocator.Temp );
            for ( int i = 0; i < inputPlanes.Length; i++ )
            {
                planes[ i ] = inputPlanes[ i ];
            }
        }

        public bool Intersects( AABB3D box )
        {
            for ( int i = 0; i < planes.Length; i++ )
            {
                Plane plane = planes[ i ];
                float3 positiveVertex = box.LowerBound;
                if ( plane.normal.x >= 0 )
                    positiveVertex.x = box.UpperBound.x;
                if ( plane.normal.y >= 0 )
                    positiveVertex.y = box.UpperBound.y;
                if ( plane.normal.z >= 0 )
                    positiveVertex.z = box.UpperBound.z;

                if ( math.dot( plane.normal, positiveVertex ) + plane.distance < 0 )
                    return false;
            }

            return true;
        }


        public void CullByFrustum( BVH bvh, ref NativeList<float3> culled )
        {
            culled.Clear();
            CullByFrustumRecursive( bvh, bvh.nodes.Length - 1, ref culled );
        }

        private void CullByFrustumRecursive(BVH bvh, int nodeIndex, ref NativeList<float3> culledPositions)
        {
            Node node = bvh.nodes[nodeIndex];
            if (Intersects(node.box))
            {
                if (node.isLeaf)
                {
                    culledPositions.Add(node.leaf.position);
                }
                else
                {
                    if (node.child1 != -1) //заменить это говно
                    {
                        CullByFrustumRecursive(bvh, node.child1, ref culledPositions);
                    }
            
                    if (node.child2 != -1) 
                    {
                        CullByFrustumRecursive(bvh, node.child2, ref culledPositions);
                    }
                }
            }
        }
        
        public void CullByFrustumDebug( BVH bvh, ref NativeList<float3> culled, ref NativeList<int> checkedNodes )
        {
            culled.Clear();
            //checkedNodes.Clear();
            CullByFrustumRecursiveDebug( bvh, bvh.nodes.Length - 1, ref culled, ref checkedNodes );
        }
        
        private void CullByFrustumRecursiveDebug(BVH bvh, int nodeIndex, ref NativeList<float3> culledPositions, ref NativeList<int> checkedNodes )
        {
            Node node = bvh.nodes[nodeIndex];
            if (Intersects(node.box))
            {
                if (node.isLeaf)
                {
                    culledPositions.Add(node.leaf.position);
                }
                else
                {
                    if (node.child1 != -1) 
                    {
                        CullByFrustumRecursiveDebug(bvh, node.child1, ref culledPositions, ref checkedNodes);
                    }
            
                    if (node.child2 != -1) 
                    {
                        CullByFrustumRecursiveDebug(bvh, node.child2, ref culledPositions, ref checkedNodes);
                    }
                }
                checkedNodes.Add( nodeIndex );
            }
        }
        public void Dispose() => planes.Dispose();
    }
}
