using System;
using System.Threading;
using BrgContainer.Runtime;
using Cosmocompost.InstanceDrawing;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Task = System.Threading.Tasks.Task;

//METEORITE EVENT
public class MeteoriteEvent : MonoBehaviour
{
    [Inject] private InstanceDrawer _instanceDrawer;
    
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    [SerializeField] private float3 _center;
    [SerializeField] private float _radius;

    private JobHandle _eraseByHitsFence;

    private float3 _gravity = new float3(0, -9, 0);

    private NativeArray<float3> _velocities;
    private NativeArray<float3> _positions;
    private NativeArray<quaternion> _rotations;
    private NativeArray<quaternion> _rotationsIncrements;
    private NativeArray<half3> _scales;
    private NativeArray<int> _sleepingTimer;
    private int m_itemCount;
    private int _currentCount;
    private bool _isPlaying;
    private NativeQueue<int> _sleepingQueue;
    private JobHandle _updateBRGItemsDependency;
    private CancellationTokenSource _cts;


    private BatchRendererGroupContainer m_BrgContainer;
    private BatchHandle m_BatchHandle;
    private int m_BaseColorPropertyID = Shader.PropertyToID("_BaseColor");
    private int m_CustomDataPropertyID = Shader.PropertyToID("_CustomData");
    
    
    public void Init(WorldData worldData)
    {
        DIContainer.InjectProperties(this);
        m_itemCount = 1000;

        var bounds = new Bounds(Vector3.zero, new Vector3(1000.0f, 1000.0f, 1000.0f));
        m_BrgContainer = new BatchRendererGroupContainer(bounds);

        var materialProperties = new NativeArray<MaterialProperty>(2, Allocator.Temp)
        {
            [0] = MaterialProperty.Create<Color>(m_BaseColorPropertyID),
            [1] = MaterialProperty.Create<float4>(m_CustomDataPropertyID)
        };

        var batchDescriptor = new BatchDescription(m_itemCount, materialProperties, Allocator.Persistent);
        materialProperties.Dispose();

        var rendererDescriptor = new RendererDescription
        {
            MotionMode = MotionVectorGenerationMode.Camera,
            ReceiveShadows = true,
            Layer = 0,
            RenderingLayerMask = 1,
            ShadowCastingMode = ShadowCastingMode.On,
            StaticShadowCaster = false
        };

        m_BatchHandle = m_BrgContainer.AddBatch(ref batchDescriptor, _mesh, 0, _material, ref rendererDescriptor);
        var dataBuffer = m_BatchHandle.AsInstanceDataBuffer();
    
        for (int i = 0; i< m_itemCount; i++)
        {
            dataBuffer.SetColor(i, m_BaseColorPropertyID, new float4(0.9f,0.9f,0.9f,0.9f));
            dataBuffer.SetVector(i, m_CustomDataPropertyID, new float4(1,1,1,1));
        }
        
        _velocities = new NativeArray<float3>(m_itemCount, Allocator.Persistent);
        _positions = new NativeArray<float3>(m_itemCount, Allocator.Persistent);
        _rotations = new NativeArray<quaternion>(m_itemCount, Allocator.Persistent);
        _sleepingTimer = new NativeArray<int>(m_itemCount, Allocator.Persistent);
        _rotationsIncrements = new NativeArray<quaternion>(m_itemCount, Allocator.Persistent);
        _scales = new NativeArray<half3>(m_itemCount, Allocator.Persistent);
        _sleepingQueue = new NativeQueue<int>(Allocator.Persistent);

        //Play(new float3(-100, 150, 0), 50, 200);
    }

    public void SetCenterAndRadius(Vector3 centerPosition, float radius)
    {
        _center = centerPosition;
        _radius = radius;
    }

   
    public async void Play(float3 position, float radius, int count)
    {
        if (_isPlaying)
        {
            _cts.Cancel();
        }
        
        _cts = new CancellationTokenSource();

        SetCenterAndRadius(position, radius);
        RespawnAsync(count);

        _isPlaying = true;
        int num = 0;

        try
        {
            while (num < count)
            {
                Respawn(num);
                _currentCount++;
                _currentCount %= m_itemCount;
                await Task.Delay(100, _cts.Token);
                num++;

                _cts.Token.ThrowIfCancellationRequested();
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("catch");
        }
    }

    private void Stop()
    {
        _isPlaying = false;
    }

    private async void RespawnAsync(int index)
    {
        _isPlaying = true;
        Respawn(index);
        _currentCount++;
        _currentCount %= m_itemCount;
        await Task.Delay(100);
    }


    private void Respawn(int index)
    {
        //ну и зачем джоба
        CreateMeteoritesEventData createMeteoritesJob = new CreateMeteoritesEventData()
        {
            Index = index,
            DeltaTime = Time.deltaTime,
            Velocities = _velocities,
            Scales = _scales,
            CenterPosition = _center,
            MainVelocity = new float3(5, -5, 0),
            Positions = _positions,
            Radius = _radius,
            Rotations = _rotations,
            RotationIncrements = _rotationsIncrements,
            SleepingTimer = _sleepingTimer,
            SleepingQueue = _sleepingQueue,
            SpeedMinMax = new float2(5, 8)
        };

        JobHandle createMeteoritesJobHandle = createMeteoritesJob.ScheduleByRef();
        createMeteoritesJobHandle.Complete();
    }


    private void Update()
    {
        if (!_isPlaying) return;

        var dt = Time.deltaTime;

        // GRAVITY
        var gravityJob = new GravityJob()
        {
            DeltaTime = dt,
            Gravity = _gravity,
            Velocities = _velocities,
            Sleeping = _sleepingTimer
        };

        JobHandle gravityDependency = gravityJob.ScheduleByRef(_currentCount, 32);

        //PHYSICS
        var rayCastCommands = new NativeArray<RaycastCommand>(_currentCount, Allocator.TempJob);
        var raycastHits = new NativeArray<RaycastHit>(_currentCount, Allocator.TempJob);

        var prepareRaycasts = new PrepareRaycastCommandJob()
        {
            DeltaTime = dt,
            Raycasts = rayCastCommands,
            Velosities = _velocities,
            Positions = _positions,
        };

        var prepareRaycastsDependency = prepareRaycasts.ScheduleByRef(_currentCount, 32, gravityDependency);

        var raycastDependency =
            RaycastCommand.ScheduleBatch(rayCastCommands, raycastHits, 32, prepareRaycastsDependency);

        //INTEGRATE
        var integratePhysicsJob = new IntegratePhysicsJob()
        {
            DeltaTime = dt,
            Hits = raycastHits,
            Positions = _positions,
            Rotations = _rotations,
            RotationsIncrement = _rotationsIncrements,
            Velocities = _velocities,
            Sleeping = _sleepingTimer
        };

        JobHandle integratePhysicDependency = integratePhysicsJob.ScheduleByRef(_currentCount, 32, raycastDependency);

        /// COLLISION RESPONSE
        var collisionResponseBounceJob = new CalculateCollisionResponseBounceJob()
        {
            Hits = raycastHits,
            RotationIncrements = _rotationsIncrements,
            Velocities = _velocities,
            Sleeping = _sleepingTimer
        };

        JobHandle collisionResponseDependency =
            collisionResponseBounceJob.ScheduleByRef(_currentCount, 32, integratePhysicDependency);

        collisionResponseDependency.Complete();


        //UPDATE OBJECTS
        var dataBuffer = m_BatchHandle.AsInstanceDataBuffer();
        UpdateInstanceTRSJob updateMeteoriteInstance = new UpdateInstanceTRSJob()
        {   
            InstanceDataBuffer = dataBuffer,
            Positions = _positions,
            Rotations = _rotations,
            Scale = _scales
        };

        JobHandle updateItemsDataDependency = updateMeteoriteInstance.ScheduleByRef(_currentCount, 32, integratePhysicDependency);
        updateItemsDataDependency.Complete();


        FindSleepingObjectsJob findSleepingObjectJobJob = new FindSleepingObjectsJob()
        {
            SleepQueue = _sleepingQueue.AsParallelWriter(),
            Sleeping = _sleepingTimer
        };

        JobHandle findSleepingObjectsJobHandle =
            findSleepingObjectJobJob.ScheduleByRef(_currentCount, 8, updateItemsDataDependency);
        findSleepingObjectsJobHandle.Complete();

        if (_sleepingQueue.Count == _currentCount)
        {
            Stop();
        }

        _sleepingQueue.Clear();
        _instanceDrawer.EraseByRaycastHitsArray(raycastHits);

        raycastHits.Dispose();
        rayCastCommands.Dispose();
    }

    private void LateUpdate()
    {
        if (!_isPlaying) return;
        UploadData();
    }

    [BurstCompile]
    private void UploadData()
    {
        m_BatchHandle.Upload(_currentCount);
    }

    public void Dispose()
    {
        _cts.Cancel();
        
        m_BrgContainer.Dispose();
        
        if (_velocities.IsCreated)
        {
            _velocities.Dispose();
        }

        if (_positions.IsCreated)
        {
            _positions.Dispose();
        }

        if (_sleepingTimer.IsCreated)
        {
            _sleepingTimer.Dispose();
        }

        if (_rotations.IsCreated)
        {
            _rotations.Dispose();
        }

        if (_rotationsIncrements.IsCreated)
        {
            _rotationsIncrements.Dispose();
        }
        

        if (_scales.IsCreated)
        {
            _scales.Dispose();
        }

        if (_sleepingQueue.IsCreated)
        {
            _sleepingQueue.Dispose();
        }
    }
}