using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace BVH
{
    public struct Leaf
    {
        public float3 position;
        public float3 size;
    }

    public struct Node
    {
        public AABB3D box;
        public int parentIndex;

        public bool isLeaf;

        //лист
        public Leaf leaf;

        //internal
        public int child1;
        public int child2;
    }


    public unsafe struct BVH
    {
        public NativeList< Node > nodes;

        public BVH( Allocator allocator ) => nodes = new NativeList< Node >( allocator );

        public void Dispose() => nodes.Dispose();

        public void Build( NativeList< Leaf > leaves )
        {
            nodes.Clear();
            BuildRecursive( leaves, 0, leaves.Length );
        }

        private int BuildRecursive( NativeList< Leaf > leaves, int start, int end )
        {
            Node node = new();

            AABB3D box = new( new float3( float.MaxValue, float.MaxValue, float.MaxValue ), new float3( float.MinValue, float.MinValue, float.MinValue ) );
            for ( int i = start; i < end; i++ )
                box = box.Union( new AABB3D( leaves[ i ].position, leaves[ i ].position + leaves[ i ].size ) );

            node.box = box;

            int count = end - start;

            if ( count == 1 )
            {
                node.child1 = -1;
                node.child2 = -1;
                node.isLeaf = true;
                node.leaf = leaves[ start ];
                nodes.Add(node);
                return nodes.Length - 1;
            }
            else
            {
                SortLeavesByX( leaves, start, end );
                int mid = start + count / 2;
                int leftChildIndex = BuildRecursive( leaves, start, mid );
                int rightChildIndex = BuildRecursive( leaves, mid, end );
                
                node.child1 = leftChildIndex;
                node.child2 = rightChildIndex;
                nodes.Add(node);
                int currentNodeIndex = nodes.Length - 1;
                
                
                Node* nodePtr = (Node*)nodes.GetUnsafePtr();
                nodePtr[leftChildIndex].parentIndex = currentNodeIndex;
                nodePtr[rightChildIndex].parentIndex = currentNodeIndex;
                return currentNodeIndex;
            }
        }

        private void SortLeavesByX( NativeList< Leaf > leaves, int start, int end )
        {
            for ( int i = start + 1; i < end; i++ )
            {
                Leaf key = leaves[ i ];
                int j = i - 1;

                while ( j >= start && leaves[ j ].position.x > key.position.x )
                {
                    leaves[ j + 1 ] = leaves[ j ];
                    j = j - 1;
                }
                leaves[ j + 1 ] = key;
            }
        }
    }
}
