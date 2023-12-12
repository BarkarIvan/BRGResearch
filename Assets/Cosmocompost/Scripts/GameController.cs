using Common.UI;
using Cosmocompost.Inputs;
using Cosmocompost.Levels;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Inject] private LevelController _levelController;
    [Inject] private InputManager _inputManager;

    [Inject]
    private UIController _uiController;

    private int _currentLevelIndex;

   


    private void Start()
    {
        DIContainer.InjectProperties(this);
        _uiController.Init();
        _inputManager.SetMode(InputManager.InputMode.CameraMovement);
    }

    private void OnDestroy()
    {
        _levelController.DisposeCurrentLevel();
        
    }
    
    


}
