using BVH;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class RegularGridCuller
{
    private int3 _gridSize;

    private NativeArray<TextureItemData> _textureItemData;
    private NativeArray<bool> _positionVisibility;

    private NativeArray<bool> _voxelVisibility;
    private NativeArray<int> _instanceToVoxel;

    private Camera _camera;

    private int _numVoxels;
    private float3 _voxelSize;
    private float3 _gridOrigin;

    public RegularGridCuller(int3 gridSize, int maxCount, NativeArray<TextureItemData> textureItemData, NativeArray<bool> positionVisibility, Camera camera, WorldData worldData)
    {
        _gridSize = gridSize;
        _textureItemData = textureItemData;
        _positionVisibility = positionVisibility;
        _camera = camera;

        _numVoxels = gridSize.x * gridSize.y * gridSize.z;
        _voxelSize = worldData.CalculateVoxelSize(gridSize);
        _gridOrigin = new float3(-worldData.WorldSize.x / 2, 0, -worldData.WorldSize.z / 2);

        _voxelVisibility = new NativeArray<bool>(_numVoxels, Allocator.Persistent);
        _instanceToVoxel = new NativeArray<int>(maxCount, Allocator.Persistent); //texture size
    }

    public void CullPositionWithRegularGrid()
    {
        NativeArray<AABB3D> bounds = new NativeArray<AABB3D>(_numVoxels, Allocator.TempJob);
        GenerateVoxelBoundsJob generateVoxel = new GenerateVoxelBoundsJob()
        {
            gridSize = _gridSize,
            voxelSize = _voxelSize,
            gridOrigin = _gridOrigin,
            voxelBounds = bounds
        };
        JobHandle generateVoxelHandle = generateVoxel.Schedule(_numVoxels, 32);

        //populate positions to voxels//
        PopulateVoxelGridJob populateVoxel = new()
        {
            ItemData = _textureItemData,
            gridOrigin = _gridOrigin,
            voxelSize = _voxelSize,
            InstanceToVoxel = _instanceToVoxel,
            gridSize = _gridSize
        };
        JobHandle populateVoxelHandle = populateVoxel.Schedule(_textureItemData.Length, 32, generateVoxelHandle);


        //cull positions
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_camera);
        NativeArray<float4> frustumPlanes = new NativeArray<float4>(planes.Length, Allocator.TempJob);
        for (int p = 0; p < 6; p++)
        {
            frustumPlanes[p] = new float4(planes[p].normal, planes[p].distance);
        }

        CullVoxelJob cullVoxels = new()
        {
            FrustumPlanes = frustumPlanes,
            voxelBounds = bounds,
            voxelVisibility = _voxelVisibility
        };

        JobHandle cullHandle = cullVoxels.Schedule(_numVoxels, 32, populateVoxelHandle);

        SetVisibilityJob collectVisibleJob = new()
        {
            voxelVisibility = _voxelVisibility,
            InstanceToVoxel = _instanceToVoxel,
            positionVisibility = _positionVisibility
        };

        JobHandle collectVisibleHandle = collectVisibleJob.Schedule(_textureItemData.Length, 32, cullHandle);
        collectVisibleHandle.Complete();
        bounds.Dispose();
        frustumPlanes.Dispose();

    }

    public void Dispose()
    {
        _voxelVisibility.Dispose();
        _instanceToVoxel.Dispose();
    }
}