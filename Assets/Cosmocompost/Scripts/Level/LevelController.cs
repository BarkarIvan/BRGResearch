using Cosmocompost.Cameras;
using Cosmocompost.InstanceDrawing;
using DG.Tweening;
using UnityEngine;

namespace Cosmocompost.Levels
{
    
    public class LevelController : MonoBehaviour
    {
        [Inject] private InstanceDrawer _instanceDrawer;

        [Inject]
        private CameraController _cameraController;

        [Inject]
        private MeteoriteEvent _meteoriteEvent;

        private Level _currentLevel;
        private Texture2D _currentLevelInstanceTexture;

        [SerializeField]
        private AllLevelsData _allLevelsData;
        
        private void Start()
        {
            DIContainer.InjectProperties(this);
        }
        

        public void LoadLevel(Level level)
        {
            Instantiate(level.gameObject,Vector3.zero, Quaternion.identity);
            _currentLevel = level;
            _instanceDrawer.Init(level._worldData);
            _meteoriteEvent.Init(level._worldData);// TEST
            _cameraController.SetWorld(level._worldData);
        }

        public void DisposeCurrentLevel()
        {
            _meteoriteEvent.Dispose();
            _instanceDrawer.Dispose();
            DestroyImmediate(_currentLevel);
            _currentLevel = null;
        }

        public string GetLevelName()
        {
            return _currentLevel.gameObject.name;
        }

        public void LoadLevelByIndex(int index)
        {
           LoadLevel(_allLevelsData.LevelPrefabs[index]);
        }
    }
}