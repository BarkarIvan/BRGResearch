using System.Runtime.InteropServices;
using BrgContainer.Runtime;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

[StructLayout(LayoutKind.Sequential)]
[BurstCompile (Debug = true)]
public struct UpdatePlantInstancesJob : IJob
{
    [NativeDisableParallelForRestriction]
    public BatchInstanceDataBuffer InstanceDataBuffer;
 
    [ReadOnly]
    public NativeParallelMultiHashMap<int, float3>.Enumerator PositionData;

    public  void Execute()
    {
        int i = 0;
        while (PositionData.MoveNext())
        {
            float3 position = PositionData.Current;
            uint fixedSeed = 12345;
            uint uniqueSeed = fixedSeed + ((uint)math.abs(position.x * 10000)) +
                              ((uint)math.abs(position.z * 19000)) % 100089000;
            Random random = new Random(uniqueSeed);
            float rotationY = 360 * random.NextFloat();
            quaternion rotation = quaternion.Euler(0, rotationY, 0);
            float uniformScale = random.NextFloat(1f, 1.5f);

            InstanceDataBuffer.SetTRS(i, position, rotation, new float3(uniformScale, uniformScale, uniformScale));
            i++;
        }
    }
}