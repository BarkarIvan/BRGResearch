using System;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct WorldData
{
    public Vector3 WorldPosition;
    public Vector3 WorldSize;
    
    
    public float3 CalculateVoxelSize(int3 gridSize)
    {
        return new float3(WorldSize.x / gridSize.x, WorldSize.y / gridSize.y, WorldSize.z / gridSize.z);
    }
}

