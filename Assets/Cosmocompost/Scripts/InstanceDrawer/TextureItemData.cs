using Unity.Mathematics;
using UnityEngine.Serialization;

public struct TextureItemData
{
    [FormerlySerializedAs("positionWS")] public float3 Position;
    [FormerlySerializedAs("GradationNum")] public int MeshNum;
    public bool IsUsed;
}
