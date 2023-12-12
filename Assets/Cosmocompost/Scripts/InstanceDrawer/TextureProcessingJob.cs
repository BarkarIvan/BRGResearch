using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Cosmocompost.TextureProcessing.Jobs
{
    [StructLayout(LayoutKind.Sequential)]
    [BurstCompile (OptimizeFor = OptimizeFor.Performance, FloatMode = FloatMode.Fast, CompileSynchronously = true, FloatPrecision = FloatPrecision.Low, DisableSafetyChecks = true)]
    public struct TextureProcessingJob : IJob
    {
        [ReadOnly]
        public NativeArray<half4> Pixels; 
        
        [WriteOnly, NativeDisableContainerSafetyRestriction]
        public NativeArray<int> GradationCounters;

        
        public int TextureSize;
        public float RandomOffset;
        public int GradationNum;
        public Vector3 WorldSize;
        public Vector3 WorldCenter;
        [WriteOnly, NativeDisableContainerSafetyRestriction]
        public NativeParallelMultiHashMap<int, float3>.ParallelWriter HashMap;


        public unsafe void Execute()
        {
            for (int i = 0; i < TextureSize*TextureSize; i++)
            {
                int x = i % TextureSize;
                int y = i / TextureSize;
            
                half4 pixelColor = Pixels[y * TextureSize + x];
                if (pixelColor.x > 0)
                {
                    var seed = math.hash(new int2(x, y));
                    var random = new Random(seed);
                    float2 randomOffset = new float2(random.NextFloat(-RandomOffset, RandomOffset),
                        random.NextFloat(-RandomOffset, RandomOffset));
                    float2 uv = new float2((float)x / TextureSize, (float)y / TextureSize);
                    uv += randomOffset;
                    int gradationNum = (int)math.round(pixelColor.x * (GradationNum - 1));
                    float3 position = UVToWorldPosition(uv, pixelColor.y);
                  
                    var itemData = new TextureItemData
                    {
                        Position = UVToWorldPosition(uv, pixelColor.y),
                        MeshNum = gradationNum
                    };

                    ref var gradationCounter =
                        ref UnsafeUtility.ArrayElementAsRef<int>(GradationCounters.GetUnsafePtr(), gradationNum);
                    Interlocked.Increment(ref gradationCounter);
                    HashMap.Add(gradationNum, position);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float3 UVToWorldPosition(float2 uv, float relativeHeight)
        {
            float pixelSizeX = 1f / TextureSize;
            float pixelSizeY = 1f / TextureSize;
            uv.x += pixelSizeX * 0.5f;
            uv.y += pixelSizeY * 0.5f;
            float3 worldPosition = new Vector3(uv.x * WorldSize.x + WorldCenter.x - 0.5f * WorldSize.x,
                relativeHeight * WorldSize.y,
                uv.y * WorldSize.z + WorldCenter.z - 0.5f * WorldSize.z);

            return worldPosition;
        }
    }
}