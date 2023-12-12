using Cosmocompost.Inputs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cosmocompost.Cameras
{
    public class CameraController : MonoBehaviour
    {
        [Inject]
        private InputManager _inputManager;

        private Camera _mainCamera;
        private Plane _groundPlane;
        private WorldData _worldData;

        private float _horizontalOffset;
        private Vector4 _cameraAreaMinMax;


        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Start()
        {
            DIContainer.InjectProperties(this);
            _inputManager.OnDragDeltaPerformRecieved += OnInputManagerPerformed;
            _groundPlane = new Plane(Vector3.up, Vector3.zero);
        }

        private void SetCameraAreaAndOffset()
        {
            //OFFSET
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);

            Ray ray = _mainCamera.ScreenPointToRay(screenCenter);
            float e;
            if (_groundPlane.Raycast(ray, out e))
            {
                Vector3 worldStartPoint = ray.GetPoint(e);
                //another raycast?
                Vector3 camProject = Vector3.ProjectOnPlane(_mainCamera.transform.position, _groundPlane.normal) + Vector3.Dot(transform.position, _groundPlane.normal) * _groundPlane.normal;
                _horizontalOffset = (worldStartPoint - camProject).magnitude;
            }

            //AREA
            Vector3 worlCenter = Vector3.zero;

            float minX = worlCenter.x - (_worldData.WorldSize.x / 2);
            float maxX = worlCenter.x + (_worldData.WorldSize.x / 2);
            float minZ = worlCenter.z - (_worldData.WorldSize.z / 2) - _horizontalOffset;
            float maxZ = worlCenter.z + (_worldData.WorldSize.z / 2) - _horizontalOffset;

            _cameraAreaMinMax = new Vector4(minX, minZ, maxX, maxZ);
        }

        private void OnInputManagerPerformed(InputAction.CallbackContext context)
        {
            switch (_inputManager.CurrentMode)
            {
                case InputManager.InputMode.CameraMovement:
                    Vector2 deltaScreen = context.ReadValue<Vector2>();
                    Vector3 deltaWorld = ScreenToWorldMovement(deltaScreen);
                    Vector3 newPosition = _mainCamera.transform.position - deltaWorld;
                    _mainCamera.transform.position = ClampCameraPosition(newPosition);

                    break;
            }
        }


        private Vector3 ClampCameraPosition(Vector3 newPosition)
        {
            float clampedX = Mathf.Clamp(newPosition.x, _cameraAreaMinMax.x, _cameraAreaMinMax.z);
            float clampedZ = Mathf.Clamp(newPosition.z, _cameraAreaMinMax.y, _cameraAreaMinMax.w);

            return new Vector3(clampedX, newPosition.y, clampedZ);
        }


        private Vector3 ScreenToWorldMovement(Vector2 deltaScreen)
        {
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);

            Ray startRay = _mainCamera.ScreenPointToRay(screenCenter);
            Ray endRay = _mainCamera.ScreenPointToRay(screenCenter + (Vector3)deltaScreen);

            float enterStart = 0.0f;
            float enterEnd = 0.0f;

            if (_groundPlane.Raycast(startRay, out enterStart) && _groundPlane.Raycast(endRay, out enterEnd))
            {
                Vector3 worldStartPoint = startRay.GetPoint(enterStart);
                Vector3 worldEndPoint = endRay.GetPoint(enterEnd);
                return (worldEndPoint - worldStartPoint);
            }

            return Vector3.zero;
        }

        public void SetWorld(WorldData levelWorldData)
        {
            _worldData = levelWorldData;
            SetCameraAreaAndOffset();
        }
    }
}