using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Cosmocompost.TextureProcessing.Jobs
{
    [BurstCompile]
    public struct HeightToColorJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<RaycastHit> RaycastHits;
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<half4> colors;
        public float WorldHeight;

        public void Execute(int index)
        {
            RaycastHit hit = RaycastHits[index];
            half depth;
            if (hit.distance != 0)
                depth = (half)(hit.point.y / WorldHeight);
            else
                depth = (half)0;
            colors[index] = new half4(new half(0), depth, (half)0, (half)1);
        }
    }
}