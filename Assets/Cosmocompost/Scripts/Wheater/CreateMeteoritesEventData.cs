using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;


public struct CreateMeteoritesEventData : IJob
{

    public float DeltaTime;
    public int Index;
    public float3 CenterPosition;
    public float Radius;
    public float3 MainVelocity;
    public float2 SpeedMinMax;
    
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<float3> Velocities;

    [NativeDisableContainerSafetyRestriction]
    public NativeArray<half3> Scales;

    [NativeDisableContainerSafetyRestriction]
    public NativeArray<float3> Positions;

    [NativeDisableContainerSafetyRestriction]
    public NativeArray<quaternion> Rotations;

    [NativeDisableContainerSafetyRestriction]
    public NativeArray<quaternion> RotationIncrements;

    [NativeDisableContainerSafetyRestriction]
    public NativeArray<int> SleepingTimer;

    [NativeDisableContainerSafetyRestriction]
    public NativeQueue<int> SleepingQueue;


    public void Execute()
    {
        Positions[Index] = GetRandomPosition(Index+1);
        Velocities[Index] = GetVelocity(Index+1);
        SleepingTimer[Index] = GetSpeepingTimer(Index+1);
        Rotations[Index] = GetRotation(Index+1);
        RotationIncrements[Index] = GetRotationIncrement(Index+1);
        Scales[Index] = GetScale(Index + 1);

        if (!SleepingQueue.IsEmpty()) SleepingQueue.Dequeue();
    }

    private half3 GetScale(int index)
    {
        var random = new Random((uint)(index * 40000));
        return (half3)random.NextFloat(1, 4);
    }

    private quaternion GetRotationIncrement(int index)
    {
        var random = new Random((uint)(index * 40000));
        float3 euler = new float3(
            random.NextFloat(-10,10) * DeltaTime,
            random.NextFloat(-10,10)* DeltaTime,
            random.NextFloat(-10,10)* DeltaTime
        );
        return quaternion.Euler(euler);
    }


    private quaternion GetRotation(int index)
    {
        var random = new Random((uint)(index * 40000));

        float3 euler = new float3(
            random.NextFloat(0,360),
            random.NextFloat(0, 360),
            random.NextFloat(0,360)
        );
        return quaternion.Euler(euler);
    }

    private int GetSpeepingTimer(int index)
    {
       return 0;
    }
    
    private float3 GetVelocity(int index)
    {
        var random = new Random((uint)(index * 15000));
        float speed = random.NextFloat(SpeedMinMax.x, SpeedMinMax.y);
        return MainVelocity * speed;
    }
    
    
    private float3 GetRandomPosition(int index)
    {
        var random = new Random((uint)(index * 10000));
        float angle = random.NextFloat(0, math.PI * 2);
        float distance = random.NextFloat(0, Radius);

        float x = CenterPosition.x + distance * math.cos(angle);
        float y = CenterPosition.y;//+ random.NextFloat(0, 10);
        float z = CenterPosition.z + distance * math.sin(angle);

        return new float3(x, y, z);
        
    }
}


