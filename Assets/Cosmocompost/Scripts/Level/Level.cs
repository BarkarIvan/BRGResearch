using UnityEngine;


namespace Cosmocompost.Levels
{
    public class Level : MonoBehaviour
    {
      
        //add level config
        
        public WorldData _worldData;
         
      //  public string LevelName;
        
        private Vector3 _worldCenter;
        
        private void OnValidate() => _worldCenter = _worldData.WorldPosition + _worldData.WorldSize / 2;

        private void Start()
        {
            _worldCenter = _worldData.WorldPosition + _worldData.WorldSize / 2;
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube( _worldCenter, _worldData.WorldSize );
            Gizmos.color = Color.black;
        }
    }
}
