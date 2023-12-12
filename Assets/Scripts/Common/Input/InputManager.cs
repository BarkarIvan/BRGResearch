using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cosmocompost.Inputs
{
    public class InputManager : MonoBehaviour
    {
        public enum InputMode
        {
            Disable,
            CameraMovement,
            ObjectInteraction,
            Drag,
        }

        public InputActionAsset InputActionAsset;
        public InputMode CurrentMode;
        public event Action<InputAction.CallbackContext> OnDragDeltaPerformRecieved;
        public event Action<InputAction.CallbackContext> OnDragDeltaStartedRecieved;
        public event Action<InputAction.CallbackContext> OnDragPositionPerformRecieved;

        private InputAction _deltaDragAction;
        private InputAction _positionDragAction;

        private float _dragThreshold = 0.2f;
        private Vector2 _startScreenPosition;
        private Vector2 _currentScreenPosition;

        public void SetMode(InputMode mode)
        {
            CurrentMode = mode;
        }

        private void Awake()
        {
            _deltaDragAction = InputActionAsset.FindAction("DeltaDragAction");
            _positionDragAction = InputActionAsset.FindAction("PositionDragAction");
            if (_deltaDragAction != null)
            {
                _deltaDragAction.performed += OnDeltaDragActionPerformedHandler;
                _deltaDragAction.started += OnDeltaDragActionStartedHandler;
                _deltaDragAction.Enable();
            }

            if (_positionDragAction != null)
            {
                _positionDragAction.performed += OnPositionDragActionPerformedHandler;
                _positionDragAction.started += OnPositionDragActionStartedHandler;
                _positionDragAction.canceled += OnPositionDragActionanCancelldHandler;
                _positionDragAction.Enable();
            }
        }

        private void OnDeltaDragActionStartedHandler(InputAction.CallbackContext context)
        {
            OnDragDeltaStartedRecieved?.Invoke(context);
        }

        private void OnPositionDragActionanCancelldHandler(InputAction.CallbackContext obj)
        {
            _currentScreenPosition = obj.ReadValue<Vector2>();
        }

        private void OnPositionDragActionStartedHandler(InputAction.CallbackContext obj)
        {
            _startScreenPosition = obj.ReadValue<Vector2>();
        }


        private void Start()
        {
            CurrentMode = InputMode.Disable;
        }

        private void OnDestroy()
        {
            if (_deltaDragAction != null)
            {
                _deltaDragAction.performed -= OnDeltaDragActionPerformedHandler;
                _deltaDragAction.started -= OnDeltaDragActionStartedHandler;

                _deltaDragAction.Disable();
            }
            
            if (_positionDragAction != null)
            {
                _positionDragAction.performed -= OnPositionDragActionPerformedHandler;
                _positionDragAction.started -= OnPositionDragActionStartedHandler;
                _positionDragAction.canceled -= OnPositionDragActionanCancelldHandler;
                _positionDragAction.Disable();
            }
        }


        private void OnDeltaDragActionPerformedHandler(InputAction.CallbackContext context)
        {
           
            OnDragDeltaPerformRecieved?.Invoke(context);
        }
        
        private void OnPositionDragActionPerformedHandler(InputAction.CallbackContext context)
        {
            _currentScreenPosition = context.ReadValue<Vector2>();
            if ((_currentScreenPosition - _startScreenPosition).magnitude < _dragThreshold) return;
            OnDragPositionPerformRecieved?.Invoke(context);

        }


    }
}