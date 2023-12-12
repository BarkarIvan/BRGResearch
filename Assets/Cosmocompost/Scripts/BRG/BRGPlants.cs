using System.Collections.Generic;
using Unity.Jobs;

namespace Cosmocompost.BRG.Plants
{
    using BrgContainer.Runtime;
    using InstanceDrawing;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.Rendering;


    public class BRGPlants : MonoBehaviour
    {
        [Inject] private InstanceDrawer _instanceDrawer;

        public Mesh[] PlantsMesh;
        public Material PlantsMaterial;

        private int _itemCount;
        private int _currentCount;

        private float _gradation;
        private BatchRendererGroupContainer m_BrgContainer;
        
        private List<BatchHandle> m_BatchHandlesList = new List<BatchHandle>();
        int m_BaseColorPropertyID = Shader.PropertyToID("_BaseColor");
        int m_CustomDataPropertyID = Shader.PropertyToID("_CustomData");
       

        public void Init(int maxCount)
        {
            var bounds = new Bounds(Vector3.zero, new Vector3(1000.0f, 1000.0f, 1000.0f));
            m_BrgContainer = new BatchRendererGroupContainer(bounds);

            var materialProperties = new NativeArray<MaterialProperty>(2, Allocator.Temp)
            {
                [0] = MaterialProperty.Create<Color>(m_BaseColorPropertyID),
                [1] = MaterialProperty.Create<float4>(m_CustomDataPropertyID)
            };

            var batchDescription = new BatchDescription(maxCount, materialProperties, Allocator.Persistent);
            materialProperties.Dispose();

            var rendererDescription = new RendererDescription
            {
                MotionMode = MotionVectorGenerationMode.Camera,
                ReceiveShadows = true,
                Layer = 0,
                RenderingLayerMask = 1,
                ShadowCastingMode = ShadowCastingMode.On,
                StaticShadowCaster = false
            };

            foreach (var mesh in PlantsMesh)
            {
                var handle = m_BrgContainer.AddBatch(ref batchDescription, mesh, 0, PlantsMaterial,
                    ref rendererDescription);
                var dataBuffer = handle.AsInstanceDataBuffer();

                _itemCount = maxCount;
                for (int i = 0; i < _itemCount; i++)
                {
                    //sample from ramp
                    dataBuffer.SetTRS(i, float3.zero, quaternion.identity, new float3(1,1,1));
                    dataBuffer.SetColor(i, m_BaseColorPropertyID, new float4(1, 1, 1, 1));
                    dataBuffer.SetVector(i, m_CustomDataPropertyID, new float4(1, 1, 1, 1));
                }

                m_BatchHandlesList.Add(handle);
            }
        }

        [BurstCompile]
        public void UpdateBRGPlants(NativeParallelMultiHashMap<int, float3> mGradationPositionsHashMap)
        {
            if (mGradationPositionsHashMap.Count() == 0) return;

            for (var b = 0; b < m_BatchHandlesList.Count; b++)
            {
                var dataBuffer = m_BatchHandlesList[b].AsInstanceDataBuffer();
                var values = mGradationPositionsHashMap.GetValuesForKey(b);
                UpdatePlantInstancesJob updatePlants = new UpdatePlantInstancesJob()
                {
                    InstanceDataBuffer = dataBuffer,
                    PositionData = values
                };
                JobHandle updatePlantsJobHandle = updatePlants.ScheduleByRef();
                updatePlantsJobHandle.Complete();
                var count = mGradationPositionsHashMap.CountValuesForKey(b);
                Upload(b, count);
            }
        }

        [BurstCompile]
        private void Upload(int BatchNum, int count)
        {
            m_BatchHandlesList[BatchNum].Upload(count);
        }

        private void OnDestroy()
        {
            m_BrgContainer.Dispose();
        }
    }
}