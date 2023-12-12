using Cosmocompost.Levels;
using UnityEngine;

[CreateAssetMenu(fileName = "AllLevelsData", menuName = "ScriptableObjects/AllLevelsData", order = 1)]
public class AllLevelsData : ScriptableObject
{
    public Level[] LevelPrefabs;
}