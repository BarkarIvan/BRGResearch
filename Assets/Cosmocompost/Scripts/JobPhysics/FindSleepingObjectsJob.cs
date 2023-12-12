using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct FindSleepingObjectsJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeQueue<int>.ParallelWriter SleepQueue;

    [ReadOnly]
    public NativeArray<int> Sleeping;


    public void Execute(int index)
    {
        if (Sleeping[index] > 0)
        {
            SleepQueue.Enqueue(index);
        }
    }
}