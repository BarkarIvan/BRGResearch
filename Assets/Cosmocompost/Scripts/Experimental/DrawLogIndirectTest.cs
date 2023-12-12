using UnityEngine;

public class DrawLogIndirectTest : MonoBehaviour
{
    public Material Material;
    public Mesh[] Meshes;
    private Mesh CombinedMesh;
    public ComputeShader ComputeShader;

    public int InstancesCount;
    
    private GraphicsBuffer _posBuffer_1;
    private GraphicsBuffer _posBuffer_2;
    private GraphicsBuffer _directionBuffer;
    private GraphicsBuffer _argsBuffer;
    private GraphicsBuffer.IndirectDrawIndexedArgs[] _cmdData;
    private int _commandCount;

    private Vector4[] _positions;
    private RenderParams _renderParams;

    private int _kernelHandle;
    private int _updateArgsKernelHandle;

    //попытки в MDI
    // private struct MeshMetaData
    //{
    //  public uint vertexOffset;
    // public uint indexOffset;
    // public uint indexCount;
    // }


    private Mesh CombineMesh( Mesh[] meshes )
    {
        CombineInstance[] combine = new CombineInstance[meshes.Length];

        for ( int i = 0; i < meshes.Length; i++ )
        {
            combine[ i ].mesh = meshes[ i ];
            combine[ i ].transform = Matrix4x4.identity;
        }
        Mesh combinedMesh = new();
        combinedMesh.CombineMeshes( combine, false );

        return combinedMesh;
    }

    private void Start()
    {
        CombinedMesh = CombineMesh( Meshes ); //но вообще это не особо имело смысла, всеравно два вызова.
        _commandCount = CombinedMesh.subMeshCount;

        _kernelHandle = ComputeShader.FindKernel( "CSPositionChange" );

        _argsBuffer = new GraphicsBuffer( GraphicsBuffer.Target.IndirectArguments, _commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size );

        _cmdData = new GraphicsBuffer.IndirectDrawIndexedArgs[_commandCount];
        for ( int i = 0; i < _commandCount; i++ )
        {
            _cmdData[ i ].indexCountPerInstance = CombinedMesh.GetIndexCount( i );
            _cmdData[ i ].baseVertexIndex = CombinedMesh.GetBaseVertex( i );
            _cmdData[ i ].startIndex = CombinedMesh.GetIndexStart( i );
            _cmdData[ i ].startInstance = ( uint )( InstancesCount / _commandCount * i );
            _cmdData[ i ].instanceCount = ( uint )( InstancesCount / _commandCount );
        }
        _argsBuffer.SetData( _cmdData );

        //dirs
        _directionBuffer = new GraphicsBuffer( GraphicsBuffer.Target.Structured, InstancesCount, sizeof( float ) * 4 );
        Vector4[] directions = new Vector4[InstancesCount];

        for ( int i = 0; i < InstancesCount; i++ )
            directions[ i ] = Random.onUnitSphere;
        _directionBuffer.SetData( directions );
        ComputeShader.SetBuffer( _kernelHandle, "directionBuffer", _directionBuffer );


        //positions
        _posBuffer_1 = new GraphicsBuffer( GraphicsBuffer.Target.Structured, InstancesCount, sizeof( float ) * 4 );
        _posBuffer_2 = new GraphicsBuffer( GraphicsBuffer.Target.Structured, InstancesCount, sizeof( float ) * 4 );

        _positions = new Vector4[InstancesCount];
        for ( int i = 0; i < InstancesCount; i++ )
            _positions[ i ] = Random.insideUnitSphere * 90.0f;
        _posBuffer_1.SetData( _positions );
        _posBuffer_2.SetData( _positions );

        //compute

        ComputeShader.SetBuffer( _kernelHandle, "positionBufferR", _posBuffer_1 );
        ComputeShader.SetBuffer( _kernelHandle, "positionBufferW", _posBuffer_2 );

        _renderParams = new RenderParams( Material );
        _renderParams.worldBounds = new Bounds( Vector3.zero, Vector3.one * 200 );
        _renderParams.matProps = new MaterialPropertyBlock();
        _renderParams.matProps.SetBuffer( "positionBuffer", _posBuffer_1 );
    }


    private void Update()
    {
        ComputeShader.Dispatch( _kernelHandle, Mathf.CeilToInt(InstancesCount/64f), 1, 1 );
        _renderParams.matProps.SetBuffer( "positionBuffer", _posBuffer_1 );

        Graphics.RenderMeshIndirect( _renderParams, CombinedMesh, _argsBuffer, _commandCount );

        //думал для оптимизона и конфликтов делить буферы для чтения и записи - но я бы позамерял 
        Swap( ref _posBuffer_1, ref _posBuffer_2 );

        ComputeShader.SetBuffer( _kernelHandle, "positionBufferR", _posBuffer_1 );
        ComputeShader.SetBuffer( _kernelHandle, "positionBufferW", _posBuffer_2 );
    }


    private void Swap< T >( ref T a, ref T b ) => ( a, b ) = ( b, a );


    private void OnDestroy()
    {
        _posBuffer_1?.Release();
        _posBuffer_2?.Release();
        _directionBuffer?.Release();
    }
}
