using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RenderObjectsOverrideMaterialPass : ScriptableRenderPass
        {
            private RenderTargetHandle destination;
            private List<ShaderTagId> _shaderTagList = new List<ShaderTagId>() {new ShaderTagId("SRPDefaultUnlit")};
            private FilteringSettings _filteringSettings;
            private RenderStateBlock _renderStateBlock;
            private int _downSample;

            private Material overrideMaterial;

            public RenderObjectsOverrideMaterialPass(RenderTargetHandle dest, LayerMask layer, Material material, int downSample)
            {
                _downSample = downSample;
                overrideMaterial = material;
                destination = dest;
                _filteringSettings = new FilteringSettings(RenderQueueRange.all, layer);
                _renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                //downsample?
                var width = Mathf.Max(1, cameraTextureDescriptor.width >> _downSample);
                var height = Mathf.Max(1, cameraTextureDescriptor.height >> _downSample);
               RenderTextureDescriptor desc = new RenderTextureDescriptor(width, height, RenderTextureFormat.Depth, 32,0);

                cmd.GetTemporaryRT(destination.id, desc);
                ConfigureTarget(destination.Identifier());
                ConfigureClear(ClearFlag.All, Color.black);
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                base.FrameCleanup(cmd);
                cmd.ReleaseTemporaryRT(destination.id);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
                DrawingSettings drawingSettings = 
                    CreateDrawingSettings(_shaderTagList, ref renderingData, sortingCriteria);
                
                drawingSettings.overrideMaterial = overrideMaterial;
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref  _filteringSettings, ref _renderStateBlock);
                var cmd = CommandBufferPool.Get("SetBloomTextureDepth");
                cmd.SetGlobalTexture("_BloomTextureDepth",destination.Identifier(), RenderTextureSubElement.Depth);
                context.ExecuteCommandBuffer(cmd); 
                CommandBufferPool.Release(cmd);
               


               
               
            }

            
        }