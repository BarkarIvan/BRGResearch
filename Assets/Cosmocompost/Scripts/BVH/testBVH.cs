using BVH;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

public class BVHTest : MonoBehaviour
{
    public BVH.BVH bvh;
    public NativeList< Leaf > leaves;
    private readonly int count = 6;

    private NativeList< float3 > culled;
    private NativeArray< Plane > planes;

    private Camera _mainCamera;
    private NativeList< int > _checkedNodes;

    private void Start()
    {
        _mainCamera = Camera.main;
        
        leaves = new NativeList< Leaf >( count, Allocator.Persistent );
        Random random = new();

        for ( int i = 0; i < count; i++ )
        {
            float3 position = new( ( float )random.NextDouble() * 100, ( float )random.NextDouble() * 100, ( float )random.NextDouble() * 100 );
            float3 size = new( ( float )random.NextDouble() * 10, ( float )random.NextDouble() * 10, ( float )random.NextDouble() * 10 );
            leaves.Add( new Leaf { position = position, size = size } );
        }

        bvh = new BVH.BVH( Allocator.Persistent );
        bvh.Build( leaves );
      //  for(int i = 0; i< bvh.nodes.Length; i ++)
          //  Debug.Log( "idx - " + i + "|c1-" + bvh.nodes[i].child1 + "|c2-" + bvh.nodes[i].child2 + "|l-" + bvh.nodes[i].isLeaf + "|PRNT-" + bvh.nodes[i].parentIndex + "|");
    }

    private void Update()
    {
       
        Plane[] planesArray = GeometryUtility.CalculateFrustumPlanes( _mainCamera );
        planes = new NativeArray< Plane >( planesArray.Length, Allocator.Temp );
        for ( int i = 0; i < planesArray.Length; i++ )
            planes[ i ] = planesArray[ i ];

        if ( !culled.IsCreated )
            culled = new NativeList< float3 >( Allocator.Persistent );
        if(!_checkedNodes.IsCreated)
            _checkedNodes = new NativeList< int >( Allocator.Persistent );

       
        FrustumCuller frustumCuller = new( planes );
        _checkedNodes.Clear();
        frustumCuller.CullByFrustumDebug( bvh , ref culled, ref _checkedNodes);
        frustumCuller.Dispose();
    }

    private void OnDrawGizmos()
    {
        if ( bvh.nodes.IsCreated )
            //  for ( int i = 0; i < bvh.nodes.Length; i++ )
           // {
               DrawNodeGizmo( bvh.nodes.Length-1, 0 );
            //  Node node = bvh.nodes[ i ];
              //  float3 size = node.box.UpperBound - node.box.LowerBound;
              //  Gizmos.color = node.isLeaf ? Color.red : Color.yellow;
              // Gizmos.DrawWireCube( node.box.Center, size );
            //}
        

              if ( culled.IsCreated && Application.isPlaying )
            foreach ( float3 pos in culled )
            {
                Gizmos.color = Color.black;
                Gizmos.DrawSphere( pos, 5.0f );
            }
    }

    private void DrawNodeGizmo( int nodeIndex, float depth )
    {
        Node node = bvh.nodes[ nodeIndex ];
        float3 size = node.box.UpperBound - node.box.LowerBound;

        if ( _checkedNodes.Contains( nodeIndex ) )
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
                //float shadeFactor = Mathf.Log(bvh.nodes.Length + 1) * 0.2f;
          // float shade = 1f - shadeFactor * depth;
           // Gizmos.color = new Color(shade, shade, shade, 1f);
        }
        Gizmos.DrawWireCube( node.box.Center, size );

        if ( node.isLeaf )
            return;
        DrawNodeGizmo( node.child1, depth + 1 );
        DrawNodeGizmo( node.child2, depth + 1 );
    }

    private void OnDestroy()
    {
        bvh.Dispose();
        leaves.Dispose();
        culled.Dispose();
        _checkedNodes.Dispose();
    }
}
