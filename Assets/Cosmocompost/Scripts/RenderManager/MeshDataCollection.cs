using UnityEngine;

[CreateAssetMenu(fileName = "MeshDataCollection", menuName = "ScriptableObjects/MeshDataCollection", order = 1)]
public class MeshDataCollection : ScriptableObject
{
    public Mesh[] Meshes;
}