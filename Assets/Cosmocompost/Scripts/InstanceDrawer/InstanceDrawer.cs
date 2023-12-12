using System.IO;
using Common.UI;
using Cosmocompost.BRG.Plants;
using Cosmocompost.Inputs;
using Cosmocompost.Levels;
using Cosmocompost.TextureProcessing;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;

namespace Cosmocompost.InstanceDrawing
{
    public class InstanceDrawer : MonoBehaviour
    {
        [Inject] private InputManager _inputManager;
        [Inject] private LevelController _levelController;
        [Inject] private BRGPlants _brgPlants;
        [Inject] private UIController _uiController;

        [SerializeField] private int _brushSizeInPixels = 5;
        [SerializeField] private int _worldTextureSize = 512;

        private Texture2D _textureToPaint;
        private Camera _mainCamera;
        private TextureProcessor _textureProcessor;
        private readonly GraphicsFormat _texturesGraphicFormat = GraphicsFormat.R16G16B16A16_SFloat;

        private NativeParallelMultiHashMap<int, float3> m_gradationPositionsHashMap;

        private half _gradationSize;
        [SerializeField] private float _currentGradation = 1f;

        public enum DrawMode
        {
            DrawSoilMode = 0,
            DrawPlantsMode = 1,
            DrawPlants2Mode = 2,
            Off = 8
        }

        public DrawMode CurrentMode = DrawMode.Off;

        //Shader global
        private static readonly int s_globalLevelDataTextureID = Shader.PropertyToID("_GlobalLevelDataTexture");
        private static readonly int s_locationMinID = Shader.PropertyToID("_LocationMin");
        private static readonly int s_locationMaxID = Shader.PropertyToID("_LocationMax");


        public void Init(WorldData worldData)
        {
            DIContainer.InjectProperties(this);
            _mainCamera = Camera.main;
            int maxCount = _worldTextureSize * _worldTextureSize;
            m_gradationPositionsHashMap = new NativeParallelMultiHashMap<int, float3>(maxCount, Allocator.Persistent);
            Subscribe();

            _brgPlants.Init(maxCount);

            _gradationSize = (half)(1f / _brgPlants.PlantsMesh.Length);
            InitTexture(worldData);
            InitShaderVariables(_textureToPaint, worldData);
        }


        private void InitShaderVariables(Texture2D texture, WorldData worldData)
        {
            Vector3 locatiobPos = worldData.WorldPosition;
            Vector3 locationSize = worldData.WorldSize;

            Vector3 halfSize = locationSize * 0.5f;
            Vector3 locationMin = locatiobPos - halfSize;
            Vector3 locationMax = locatiobPos + halfSize;

            Shader.SetGlobalVector(s_locationMinID, locationMin);
            Shader.SetGlobalVector(s_locationMaxID, locationMax);
            Shader.SetGlobalTexture(s_globalLevelDataTextureID, texture);
        }


        private void InitTexture(WorldData worldData)
        {
            _textureToPaint = new Texture2D(_worldTextureSize, _worldTextureSize, _texturesGraphicFormat,
                TextureCreationFlags.None);
            _textureToPaint.filterMode = FilterMode.Bilinear;
            string filePath = GetFilePath();
            _textureProcessor = new TextureProcessor(worldData, filePath,
                m_gradationPositionsHashMap,
                _brgPlants.PlantsMesh.Length);

            if (File.Exists(filePath))
            {
                _textureProcessor.LoadDataFromFile(filePath, _textureToPaint);
            }
            else
            {
                _textureProcessor.DrawHeightMapInTexture(_textureToPaint);
            }

           // m_gradationPositionsHashMap.Clear();
            _textureProcessor.TextureProcessingReadback(_textureToPaint);
            _brgPlants.UpdateBRGPlants(m_gradationPositionsHashMap);
            _uiController.SetMapImage(_textureToPaint);
        }

        private string GetFilePath()
        {
            string levelName = _levelController.GetLevelName();
            string path = Path.Combine(Application.persistentDataPath, "WorldTextures", $"{levelName}.bin");

            return path;
        }


        private void OnInputManagerDragPositionPerformPerformed(InputAction.CallbackContext context)
        {
            switch (_inputManager.CurrentMode)
            {
                case InputManager.InputMode.Drag:
                    Ray ray = _mainCamera.ScreenPointToRay(context.ReadValue<Vector2>());
                    _textureProcessor.DrawingWithRaycastBrush(ray, _textureToPaint, _brushSizeInPixels,
                        CurrentMode == DrawMode.DrawSoilMode, (half)(_currentGradation * _gradationSize), false);
                    _brgPlants.UpdateBRGPlants(m_gradationPositionsHashMap);
                    break;
            }
        }


        public void ToggleDrawMode(InstanceDrawer.DrawMode drawMode)
        {
            CurrentMode = drawMode;
            switch (drawMode)
            {
                case DrawMode.DrawPlantsMode:
                    _currentGradation = 1;
                    break;

                case DrawMode.DrawPlants2Mode:
                    _currentGradation = 2;
                    break;
            }
        }

        private void Subscribe()
        {
            _inputManager.OnDragPositionPerformRecieved += OnInputManagerDragPositionPerformPerformed;
        }

        private void Unsubscribe()
        {
            _inputManager.OnDragPositionPerformRecieved -= OnInputManagerDragPositionPerformPerformed;
        }

        public void Dispose()
        {
            Unsubscribe();
            Destroy(_textureToPaint);
            m_gradationPositionsHashMap.Dispose();
            _textureProcessor.Dispose();
          
            
        }


        public void EraseByRaycastHitsArray(NativeArray<RaycastHit> raycastHits)
        {
            _textureProcessor.DrawingByRaycastHits(_textureToPaint, raycastHits, true, (half)0f);
            _brgPlants.UpdateBRGPlants(m_gradationPositionsHashMap);
        }
    }
}