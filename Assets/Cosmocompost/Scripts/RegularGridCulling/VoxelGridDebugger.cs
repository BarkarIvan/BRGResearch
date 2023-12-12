using System.Collections;
using System.Collections.Generic;
using BVH;
using Unity.Collections;
using UnityEngine;

public class VoxelGridDebugger : MonoBehaviour
{
   private NativeArray<AABB3D> voxelBounds;
   private List<AABB3D> voxelToDraw = new List<AABB3D>();
   private List<AABB3D> culledVoxels = new List<AABB3D>();



   private void CopyDataFromJob()
   {
      foreach (AABB3D bound in voxelBounds)
      {
         
      }
   }
   
}
