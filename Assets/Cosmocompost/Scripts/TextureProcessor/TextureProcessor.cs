using System;
using System.IO;
using System.Runtime.InteropServices;
using Cosmocompost.TextureProcessing.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Cosmocompost.TextureProcessing
{
    public class TextureProcessor
    {
       // [NonSerialized] public NativeList<TextureItemData> _outputData;

       
        private AsyncGPUReadbackRequest? _prevRequest;
        private AsyncGPUReadbackRequest? _getImageBytesRequest;
        private JobHandle _textureProcessingJobHandle;

        private WorldData _worldData;
        private Vector3 _worldCenter;

        private string _filePath;

        private int _numGradation;
        private NativeArray<int> m_gradiationCounters;
        private NativeParallelMultiHashMap<int, float3> m_gradationPositionsHashMap;

        public TextureProcessor(WorldData worldData, string filePath,
             NativeParallelMultiHashMap<int, float3> hashMap, int numGradation)
        {
            _worldData = worldData;
            _worldCenter = _worldData.WorldPosition + _worldData.WorldSize / 2;
            _numGradation = numGradation;
            _filePath = filePath;
            m_gradiationCounters = new NativeArray<int>(numGradation, Allocator.Persistent);
            m_gradationPositionsHashMap = hashMap;
        }


        public void LoadDataFromFile(string path, Texture2D texture)
        {
            byte[] bytes = File.ReadAllBytes(_filePath);
            int numOfhalfs = bytes.Length / (4 * 2);

            NativeArray<half4> loadedData = new NativeArray<half4>(numOfhalfs, Allocator.Temp);

            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
           
            try
            {
                unsafe
                {
                    IntPtr bytePointer = handle.AddrOfPinnedObject();
                    UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafePtr(loadedData), bytePointer.ToPointer(),
                        bytes.Length);
                }
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free();
            }

            texture.SetPixelData(loadedData, 0);
            texture.Apply();

            loadedData.Dispose();
        }

        #region DrawheightMapInTexture

        public void DrawHeightMapInTexture(Texture2D texture)
        {
            NativeArray<RaycastCommand> RaycastCommands = new(texture.width * texture.height, Allocator.TempJob);
            NativeArray<RaycastHit> raycastHits = new(texture.width * texture.height, Allocator.TempJob);
            PrepareTextureProcessorRaycastCommandsJob prepareTextureProcessorRaycastJob = new()
            {
                RaycastCommands = RaycastCommands,
                TextureWidth = texture.width,
                TextureHeight = texture.height,
                WorldCenter = _worldCenter,
                WorldSize = _worldData.WorldSize
            };

            JobHandle prepareRaycastHandle =
                prepareTextureProcessorRaycastJob.ScheduleByRef(texture.width * texture.height, 16);
            JobHandle raycastHandler =
                RaycastCommand.ScheduleBatch(RaycastCommands, raycastHits, 64, prepareRaycastHandle);

            HeightToColorJob heightToColor = new()
            {
                colors = texture.GetPixelData<half4>(0),
                WorldHeight = _worldData.WorldSize.y,
                RaycastHits = raycastHits
            };

            JobHandle heightToColorHandler = heightToColor.ScheduleByRef(texture.width * texture.height, 16, raycastHandler);
            
            heightToColorHandler.Complete();

            texture.Apply();

            RaycastCommands.Dispose();
            raycastHits.Dispose();
        }

        #endregion

        #region DRAWING

        public void DrawingWithRaycastBrush(Ray paintRay, Texture2D texture, int brushSize, bool isSoil, half amount,
            bool isErase)
        {
            if (Physics.Raycast(paintRay, out RaycastHit hit, Mathf.Infinity))
            {
                Vector3 positionWS = hit.point;
                Vector3 offsetPosition = positionWS - (_worldCenter - _worldData.WorldSize * 0.5f);
                Vector3 relativePosition = new(offsetPosition.x / _worldData.WorldSize.x,
                    offsetPosition.y / _worldData.WorldSize.y, offsetPosition.z / _worldData.WorldSize.z);
                int2 centerPixelCoord = new((int)(relativePosition.x * texture.width),
                    (int)(relativePosition.z * texture.height));

                DrawInTextureJobParallel DrawJob = new DrawInTextureJobParallel
                {
                    BrushSize = brushSize,
                    CenterPixelCoord = centerPixelCoord,
                    IsErase = isErase,
                    TextureSize = new Vector2Int(texture.width, texture.height),
                    Pixels = texture.GetPixelData<half4>(0),
                    IsSoil = isSoil,
                    Normal = hit.normal,
                    Amount = amount
                };

                int totalPixels = (2 * brushSize + 1) * (2 * brushSize + 1);
                JobHandle drawJobHandle = DrawJob.Schedule(totalPixels, 64);
                drawJobHandle.Complete();
                texture.Apply(false);
                
                TextureProcessingReadback(texture);
            }
        }

        public void DrawingByRaycastHits(Texture2D texture, NativeArray<RaycastHit> raycastHits, bool isErase,
            half amount)
        {
            DrawInTextureFromRaycastHitsJob drawInTextureFromRaycastHitsJob = new()
            {
                Hits = raycastHits,
                Pixels = texture.GetPixelData<half4>(0),
                IsErase = isErase,
                Amount = amount,
                WorldData = _worldData,
                TextureWidthHeight = new int2(texture.width, texture.height)
            };

            JobHandle handle = drawInTextureFromRaycastHitsJob.Schedule(raycastHits.Length, 32);
            handle.Complete();
            texture.Apply();
          
            TextureProcessingReadback(texture);
        }

        #endregion

        #region READ

        public void TextureProcessingReadback(Texture texture)
        {
            if (_prevRequest.HasValue && !_prevRequest.Value.done || !_textureProcessingJobHandle.IsCompleted)
            {
                return;
            }
            _prevRequest = AsyncGPUReadback.Request(texture, 0, texture.graphicsFormat, OnTextureReadbackComplete);
        }


        private void OnTextureReadbackComplete(AsyncGPUReadbackRequest request)
        {
            if (request.hasError)
            {
                Debug.LogError("не удалось прочесть ГПУ текстуру");
                return;
            }

            m_gradationPositionsHashMap.Clear();
           TextureProcessingJob job = new()
            {
                Pixels = request.GetData<half4>(),
                TextureSize = request.height,
                WorldSize = _worldData.WorldSize,
                WorldCenter = _worldCenter,
                RandomOffset = 0.002f,
                GradationNum = _numGradation,
                GradationCounters = m_gradiationCounters,
                HashMap = m_gradationPositionsHashMap.AsParallelWriter()
            };
            _textureProcessingJobHandle = job.ScheduleByRef();
            _textureProcessingJobHandle.Complete();
        }

        #endregion

        #region SAVE

        public void SaveTextureDataToFile(Texture2D texture)
        {
            _getImageBytesRequest = AsyncGPUReadback.Request( texture, 0,
                texture.graphicsFormat, OnSaveImageRequestComplete);
        }

        private void OnSaveImageRequestComplete(AsyncGPUReadbackRequest request)
        {
            if (request.hasError)
            {
                Debug.LogError("Failed to read GPU texture");
                return;
            }

            var requestData = request.GetData<byte>();
            byte[] bytesTSave = new byte[requestData.Length];
            Buffer.BlockCopy(requestData.ToArray(), 0, bytesTSave, 0, bytesTSave.Length);

            if (!Directory.Exists(Application.persistentDataPath + "/WorldTextures/"))
                Directory.CreateDirectory(Application.persistentDataPath + "/WorldTextures/");

            File.WriteAllBytes(_filePath, bytesTSave);
        }

        #endregion

        public void Dispose()
        {
            m_gradationPositionsHashMap.Dispose();
        }

    }
}