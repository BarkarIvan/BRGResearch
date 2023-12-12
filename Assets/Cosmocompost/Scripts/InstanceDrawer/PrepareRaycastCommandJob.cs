using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Cosmocompost.TextureProcessing.Jobs
{
    [BurstCompile]
    public struct PrepareTextureProcessorRaycastCommandsJob : IJobParallelFor
    {
        public int TextureWidth;
        public int TextureHeight;
        public Vector3 WorldSize;
        public Vector3 WorldCenter;

        [WriteOnly, NativeDisableContainerSafetyRestriction]
        public NativeArray<RaycastCommand> RaycastCommands;

        public void Execute(int index)
        {
            int x = index % TextureWidth;
            int y = index / TextureHeight;
            Vector3 positionWS = new(x * WorldSize.x / TextureWidth + WorldCenter.x - WorldSize.x * 0.5f,
                WorldSize.y,
                y * WorldSize.z / TextureHeight + WorldCenter.z - WorldSize.z * 0.5f);

            QueryParameters queryParameters = new()
            {
                hitBackfaces = true,
                hitMultipleFaces = true,
                hitTriggers = QueryTriggerInteraction.Ignore,
                layerMask = -5
            };

            RaycastCommand command = new(positionWS, Vector3.down, queryParameters, WorldSize.y);
            RaycastCommands[index] = command;
        }
    }
}