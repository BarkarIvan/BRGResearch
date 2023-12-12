using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Project
{
    [Serializable]
    public class FullScreenGridSettings
    {
        public int GridResolution = 60;
    }
    
    public class DrawFullScreenGridPass : ScriptableRenderPass
    {
        private int _gridResolution;
        private Material _material;
        private Mesh _gridMesh;

        private RenderTargetIdentifier _source;
        
        
        public  DrawFullScreenGridPass(int gridResolution, Material material, RenderTargetIdentifier src)
        {
            _source = src;
            _gridResolution = gridResolution;
            _material = material;

        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("DrawFullScreenGrid");
            cmd.SetGlobalTexture("_BloomTexture", _source);
            if (_gridMesh == null)
            {
                _gridMesh = CreateMeshGrid(_gridResolution);
            }
            cmd.DrawMesh(_gridMesh, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one), _material, 0);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        
        
        private Mesh CreateMeshGrid(int gridResolution)
            {
                float aspect = (float) Screen.width / (float) Screen.height;
                int widthRectangles = Mathf.RoundToInt((gridResolution * aspect));
                int heightRectangles = gridResolution;

                Mesh grid = new Mesh();
                grid.name = "Grid";
                Vector3[] vertices = new Vector3[widthRectangles * heightRectangles * 4];
                int[] triangles = new int[widthRectangles * heightRectangles * 6];
                Vector2[] uv = new Vector2[vertices.Length];

                for (int y = 0, vi = 0, ti = 0; y < heightRectangles; y++)
                {
                    for (int x = 0; x < widthRectangles; x++, vi += 4, ti += 6)
                    {
                        // vertices
                        vertices[vi] = new Vector3((float) x / widthRectangles - 0.5f,
                            (float) y / heightRectangles - 0.5f, 0);
                        vertices[vi + 1] = new Vector3((float) (x + 1) / widthRectangles - 0.5f,
                            (float) y / heightRectangles - 0.5f, 0);
                        vertices[vi + 2] = new Vector3((float) (x + 1) / widthRectangles - 0.5f,
                            (float) (y + 1) / heightRectangles - 0.5f, 0);
                        vertices[vi + 3] = new Vector3((float) x / widthRectangles - 0.5f,
                            (float) (y + 1) / heightRectangles - 0.5f, 0);

                        // triangles
                        triangles[ti] = vi;
                        triangles[ti + 1] = vi + 1;
                        triangles[ti + 2] = vi + 2;
                        triangles[ti + 3] = vi;
                        triangles[ti + 4] = vi + 2;
                        triangles[ti + 5] = vi + 3;

                        // CUVs
                        float u = (x + 0.5f) / widthRectangles;
                        float v = (y + 0.5f) / heightRectangles;
                        uv[vi] = new Vector2(u, v);
                        uv[vi + 1] = new Vector2(u, v);
                        uv[vi + 2] = new Vector2(u, v);
                        uv[vi + 3] = new Vector2(u, v);
                    }
                }

                grid.vertices = vertices;
                grid.triangles = triangles;
                grid.uv = uv;
                return grid;
            }
    }
}
