using System;
using Cosmocompost.Inputs;
using Cosmocompost.InstanceDrawing;
using Cosmocompost.Levels;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

namespace Common.UI
{
    public class UIController : MonoBehaviour
    {
        [Inject] private LevelController _levelController;
        [Inject] private InstanceDrawer _instanceDrawer;
        [Inject] private InputManager _inputManager;
        [Inject] private MeteoriteEvent _meteoriteEvent;

        private Camera UICamera;
        public GameObject UIPrefab;

        private UIPlanetMenuScreen _uiPlanetMenuScreen;
        private UIGameScreen _uiGameScreen;
        private UIScreen[] _allScreens;
        

        public void Init()
        {
            DIContainer.InjectProperties(this);
            GameObject uiGameObject = Instantiate(UIPrefab);
            UICamera = uiGameObject.GetComponentInChildren<Camera>();
            StackCamera(Camera.main);

            _allScreens = uiGameObject.GetComponentsInChildren<UIScreen>(true);
            foreach (UIScreen screen in _allScreens)
            {
                screen.gameObject.SetActive(true);
                screen.Init();
            }

            _uiPlanetMenuScreen = uiGameObject.GetComponentInChildren<UIPlanetMenuScreen>();
            _uiGameScreen = uiGameObject.GetComponentInChildren<UIGameScreen>();
            _uiPlanetMenuScreen.Init();
            _uiPlanetMenuScreen.Open();

            Subscribe();
        }

        private void StackCamera(Camera targetCamera)
        {
            var camData = targetCamera.GetUniversalAdditionalCameraData();
            camData.cameraStack.Add(UICamera);
        }


        private void Subscribe()
        {
            for (int i = 0; i < _uiPlanetMenuScreen.LevelButtons.Length; i++)
            {
                _uiPlanetMenuScreen.LevelButtons[i].LevelButtonClicked += OnLevelButtonClickHandler;
            }

            _uiGameScreen._drawSoilModeButton.Clicked.AddListener(OnDrawSoilButtonClickHandler);
            _uiGameScreen._drawPlantsModeButton.Clicked.AddListener(OnDrawPlantsButtonClickHandler);
            _uiGameScreen._drawPlants2ModeButton.Clicked.AddListener(OnDrawPlants2ButtonClickHandler);
            _uiGameScreen._PlayMeteoriteEventButton.Clicked.AddListener(OnPlayMeteoriteEventButtonClick);
        }

        private void OnPlayMeteoriteEventButtonClick()
        {
            var x = 100f * Random.Range(-1f, 1f);
            var z = 100f * Random.Range(-1f, 1f);
            _meteoriteEvent.Play(new float3(x, 90f, z), Random.Range(50f,100f), Random.Range(50, 100) );
            
        }


        private void OnDrawButtonClickHandler(InstanceDrawer.DrawMode newDrawMode, Action setDrawMode)
        {
            if(_instanceDrawer.CurrentMode == newDrawMode) 
            {
                _instanceDrawer.CurrentMode = InstanceDrawer.DrawMode.Off;
                _inputManager.CurrentMode = InputManager.InputMode.CameraMovement;
                _uiGameScreen.SetInactiveDrawingMode();
            }
            else 
            {
                _inputManager.CurrentMode = InputManager.InputMode.Drag;
                _instanceDrawer.ToggleDrawMode(newDrawMode);
                setDrawMode();
            }
        }
    
        private void OnDrawSoilButtonClickHandler()
        {
            OnDrawButtonClickHandler(InstanceDrawer.DrawMode.DrawSoilMode, _uiGameScreen.SetDrawSoilMode);
        }

        private void OnDrawPlantsButtonClickHandler()
        {
            OnDrawButtonClickHandler(InstanceDrawer.DrawMode.DrawPlantsMode, _uiGameScreen.SetDrawPlantsMode);
        }

        private void OnDrawPlants2ButtonClickHandler()
        {
            OnDrawButtonClickHandler(InstanceDrawer.DrawMode.DrawPlants2Mode, _uiGameScreen.SetDrawPlants2Mode);
        }

        private void OnLevelButtonClickHandler(int index)
        {
            _uiPlanetMenuScreen.Close();
            _levelController.LoadLevelByIndex(index);
            _uiGameScreen.Open();
        }

        public void SetMapImage(Texture2D image)
        {
            _uiGameScreen.SetMap(image);
        }
    }
}