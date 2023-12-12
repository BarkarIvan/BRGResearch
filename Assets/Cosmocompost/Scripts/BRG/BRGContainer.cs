using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;


public unsafe class BRGContainer
{
    private bool UseConstantBuffer => BatchRendererGroup.BufferTarget == BatchBufferTarget.ConstantBuffer;
    private bool m_castShadows;

    
    private int m_maxInstances; 
    private int m_instanceCount;
    private int m_alignedGPUWindowSize; 
    private int m_maxInstancePerWindow; 
    private int m_windowCount; 
    private int m_totalGpuBufferSize; 
    private NativeArray<float4> m_sysmemBuffer; 
    public bool IsInitialized;
    private int m_instanceSize; 
    private BatchID[] m_batchIDs; 
    private BatchMaterialID m_materialID;
    private BatchMeshID m_meshID;
    private BatchRendererGroup m_BatchRendererGroup; // TODO EXTRACT TO ANOTHER CLASS
    private GraphicsBuffer m_GPUPersistentInstanceData; //TODO EXTRACT


    private int m_subMeshCount;
    public bool Init(Mesh mesh, Material mat, int submeshCount, int maxInstances, int instanceSize, bool castShadows)
    {
        m_BatchRendererGroup = new BatchRendererGroup(this.OnPerformCulling, IntPtr.Zero);

        m_instanceSize = instanceSize;
        m_instanceCount = 0;
        m_maxInstances = maxInstances;
        m_castShadows = castShadows;
        m_subMeshCount = submeshCount;

        if (UseConstantBuffer)
        {
            m_alignedGPUWindowSize = BatchRendererGroup.GetConstantBufferMaxWindowSize();
            m_maxInstancePerWindow = m_alignedGPUWindowSize / instanceSize;
            m_windowCount = (m_maxInstances + m_maxInstancePerWindow - 1) / m_maxInstancePerWindow;
            m_totalGpuBufferSize = m_windowCount * m_alignedGPUWindowSize;
            m_GPUPersistentInstanceData = new GraphicsBuffer(GraphicsBuffer.Target.Constant, m_totalGpuBufferSize / 16, 16);
        }
        else
        {
            m_alignedGPUWindowSize = (m_maxInstances * instanceSize + 15) & (-16);
            m_maxInstancePerWindow = maxInstances;
            m_windowCount = 1;
            m_totalGpuBufferSize = m_windowCount * m_alignedGPUWindowSize;
            m_GPUPersistentInstanceData = new GraphicsBuffer(GraphicsBuffer.Target.Raw, m_totalGpuBufferSize / 4, 4);
        }
        
        var batchMetadata = new NativeArray<MetadataValue>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        // Batch metadata buffer
        int objectToWorldID = Shader.PropertyToID("unity_ObjectToWorld");
        int worldToObjectID = Shader.PropertyToID("unity_WorldToObject");
        int colorID = Shader.PropertyToID("_BaseColor");
        int customDataID = Shader.PropertyToID("_CustomData");

        m_sysmemBuffer = new NativeArray<float4>(m_totalGpuBufferSize / 16, Allocator.Persistent, NativeArrayOptions.ClearMemory);

        int numBatch;
        if (UseConstantBuffer)
        {
            numBatch = m_windowCount;
        }
        else
        {
            numBatch = 1;
        }
        m_batchIDs = new BatchID[numBatch];
        for (int b = 0; b < numBatch; b++)
        {
            batchMetadata[0] = CreateMetadataValue(objectToWorldID, 0, true);       // matrices
            batchMetadata[1] = CreateMetadataValue(worldToObjectID, m_maxInstancePerWindow * 3 * 16, true); // inverse matrices
            batchMetadata[2] = CreateMetadataValue(colorID, m_maxInstancePerWindow * 3 * 2 * 16, true); // colors
            batchMetadata[3] = CreateMetadataValue(customDataID, (m_maxInstancePerWindow) * ((3 * 2 + 1) * 16), true); //custom data float4
            
            int offset;
         
            offset = b * m_alignedGPUWindowSize;
         
            m_batchIDs[b] = m_BatchRendererGroup.AddBatch(batchMetadata, m_GPUPersistentInstanceData.bufferHandle, (uint)offset, UseConstantBuffer ? (uint)m_alignedGPUWindowSize : 0);
        }

        batchMetadata.Dispose();

        UnityEngine.Bounds bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(1048576.0f, 1048576.0f, 1048576.0f));
        m_BatchRendererGroup.SetGlobalBounds(bounds);

        if (mesh) m_meshID = m_BatchRendererGroup.RegisterMesh(mesh);
        if (mat) m_materialID = m_BatchRendererGroup.RegisterMaterial(mat);

        IsInitialized = true;
        return true;
    }

    [BurstCompile]
    public bool UploadGpuData(int instanceCount)
    {
        if ((uint)instanceCount > (uint)m_maxInstances)
            return false;

        m_instanceCount = instanceCount;
        int completeWindows = m_instanceCount / m_maxInstancePerWindow;

        if (completeWindows > 0)
        {
            int sizeInFloat4 = (completeWindows * m_alignedGPUWindowSize) / 16;
            m_GPUPersistentInstanceData.SetData(m_sysmemBuffer, 0, 0, sizeInFloat4);
        }

        int lastBatchId = completeWindows;
        int itemInLastBatch = (m_instanceCount) - m_maxInstancePerWindow * completeWindows;

        if (itemInLastBatch > 0)
        {
            int windowOffsetInFloat4 = (lastBatchId * m_alignedGPUWindowSize) / 16;
            int offsetMat1 = windowOffsetInFloat4 + m_maxInstancePerWindow * 0;
            int offsetMat2 = windowOffsetInFloat4 + m_maxInstancePerWindow * 3;
            int offsetColor = windowOffsetInFloat4 + m_maxInstancePerWindow * 3 * 2;
            int offsetCustomData = windowOffsetInFloat4 + (m_maxInstancePerWindow) * ((3 * 2) + 1);
            m_GPUPersistentInstanceData.SetData(m_sysmemBuffer, offsetMat1, offsetMat1, itemInLastBatch * 3);     
            m_GPUPersistentInstanceData.SetData(m_sysmemBuffer, offsetMat2, offsetMat2, itemInLastBatch * 3);  
            m_GPUPersistentInstanceData.SetData(m_sysmemBuffer, offsetColor, offsetColor, itemInLastBatch * 1);   
            m_GPUPersistentInstanceData.SetData(m_sysmemBuffer, offsetCustomData, offsetCustomData,itemInLastBatch * 1);
        }
        return true;
    }

    public void ShutDown()
    {
        if (IsInitialized)
        {
            for (uint b = 0; b < m_windowCount; b++)
                m_BatchRendererGroup.RemoveBatch(m_batchIDs[b]);

            m_BatchRendererGroup.UnregisterMaterial(m_materialID);
            m_BatchRendererGroup.UnregisterMesh(m_meshID);
            m_BatchRendererGroup.Dispose();
            m_GPUPersistentInstanceData.Dispose();
            m_sysmemBuffer.Dispose();
        }
    }

    public NativeArray<float4> GetSysMemBuffer(out int totalSize, out int alignedWindowSize)
    {
        totalSize = m_totalGpuBufferSize;
        alignedWindowSize = m_alignedGPUWindowSize;
        return m_sysmemBuffer;
    }

    static MetadataValue CreateMetadataValue(int nameID, int gpuOffset, bool isPerInstance)
    {
        const uint kIsPerInstanceBit = 0x80000000;
        return new MetadataValue
        {
            NameID = nameID,
            Value = (uint)gpuOffset | (isPerInstance ? (kIsPerInstanceBit) : 0),
        };
    }

    private static T* Malloc<T>(uint count) where T : unmanaged
    {
        return (T*)UnsafeUtility.Malloc(
            UnsafeUtility.SizeOf<T>() * count,
            UnsafeUtility.AlignOf<T>(),
            Allocator.TempJob);
    }

    [BurstCompile] ///TODO EXTRACT TO DIFFERENT CLASS
    public JobHandle OnPerformCulling(BatchRendererGroup rendererGroup, BatchCullingContext cullingContext, BatchCullingOutput cullingOutput, IntPtr userContext)
    {
        if (IsInitialized)
        {
            BatchCullingOutputDrawCommands drawCommands = new BatchCullingOutputDrawCommands();

            int drawCommandCount = (m_instanceCount + m_maxInstancePerWindow - 1) / m_maxInstancePerWindow;
            drawCommandCount *= m_subMeshCount;
            int maxInstancePerDrawCommand = m_maxInstancePerWindow;
            
            drawCommands.drawCommandCount = drawCommandCount;

            drawCommands.drawRangeCount = 1;
            drawCommands.drawRanges = Malloc<BatchDrawRange>(1);
            drawCommands.drawRanges[0] = new BatchDrawRange
            {
                drawCommandsBegin = 0,
                drawCommandsCount = (uint)drawCommandCount,
                filterSettings = new BatchFilterSettings
                {
                    renderingLayerMask = 1,
                    layer = 0,
                    motionMode = MotionVectorGenerationMode.Camera,
                    shadowCastingMode = m_castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off,
                    receiveShadows = true,
                    staticShadowCaster = false,
                    allDepthSorted = false
                }
            };

            //TODO Culling here
            if (drawCommands.drawCommandCount > 0)
            {
                int visibilityArraySize = maxInstancePerDrawCommand;
                if (m_instanceCount < visibilityArraySize)
                    visibilityArraySize = m_instanceCount;

                drawCommands.visibleInstances = Malloc<int>((uint)visibilityArraySize);

                for (int i = 0; i < visibilityArraySize; i++)
                    drawCommands.visibleInstances[i] = i;

                drawCommands.drawCommands = Malloc<BatchDrawCommand>((uint)drawCommandCount);

                int offset = 0;
                int left = m_instanceCount;
                for (int b = 0; b < drawCommandCount; b++)
                {
                    int inBatchCount;

                    if (!UseConstantBuffer)
                    {
                        inBatchCount = left > m_instanceCount/m_subMeshCount ? m_instanceCount/m_subMeshCount : left;
                    }
                    else
                    {
                        inBatchCount = left > maxInstancePerDrawCommand ? maxInstancePerDrawCommand:left;
                    }
                    
                    drawCommands.drawCommands[b] = new BatchDrawCommand
                    {
                        visibleOffset = (uint)offset,    
                        visibleCount = (uint)inBatchCount,
                        batchID = UseConstantBuffer? m_batchIDs[b % m_windowCount] : m_batchIDs[0],
                        materialID = m_materialID,
                        meshID = m_meshID,
                        submeshIndex = UseConstantBuffer ? (ushort)(b % m_windowCount % m_subMeshCount) : (ushort)(b % m_subMeshCount),
                        splitVisibilityMask = 0xff,
                        flags = BatchDrawCommandFlags.None,
                        sortingPosition = 0
                    };
                    left -= inBatchCount;
                    if (!UseConstantBuffer)
                    {
                        offset += (m_instanceCount / m_subMeshCount);
                    }
                    else
                    {
                        offset = 0;
                    }
                   
                }
            }

            cullingOutput.drawCommands[0] = drawCommands;
            drawCommands.instanceSortingPositions = null;
            drawCommands.instanceSortingPositionFloatCount = 0;
        }

        return new JobHandle();
    }
}