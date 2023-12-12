using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


namespace Cosmocompost.TextureProcessing.Jobs
{
    [StructLayout(LayoutKind.Sequential)]
    [BurstCompile(OptimizeFor = OptimizeFor.Performance, FloatMode = FloatMode.Fast, CompileSynchronously = true, FloatPrecision = FloatPrecision.Low, DisableSafetyChecks = true)]

    public struct DrawInTextureJobParallel : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<half4> Pixels;
       
        public int2 CenterPixelCoord;
        public int BrushSize;
        public Vector2Int TextureSize;
        public bool IsSoil;
        public bool IsErase;
        public Vector3 Normal;
        public half Amount;

        public void Execute(int index)
        {
            int y = index / (2 * BrushSize + 1) - BrushSize;
            int x = index % (2 * BrushSize + 1) - BrushSize;

            if (x * x + y * y > BrushSize * BrushSize) return;
           
            var pixelCoord = new int2(CenterPixelCoord.x + x, CenterPixelCoord.y + y);
         
            if (pixelCoord.x < 0 || pixelCoord.x >= TextureSize.x || pixelCoord.y < 0 ||
                pixelCoord.y >= TextureSize.y) return;
          
            int pixelIndex = pixelCoord.y * TextureSize.x + pixelCoord.x;
            half4 originalColor = Pixels[pixelIndex];
            half amount = (IsErase ? (half)0 : Amount);
          
            if (IsSoil )
            {
                if (math.dot(Normal, new half3((half)0, (half)1.0, (half)0.0)) > 0.5f)
                {
                    Pixels[pixelIndex] = new half4(originalColor.x, originalColor.y, (half)1, (half)1);
                }
            }
            else //is plantss
            {
                if (originalColor.z > 0)
                {
                    Pixels[pixelIndex] = new half4(amount, originalColor.y, originalColor.z, (half)1);
                }
            }
        }
    }
}