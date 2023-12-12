using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RenderObjectsPass : ScriptableRenderPass
{
    private RenderTargetHandle destination;
    private List<ShaderTagId> _shaderTagList = new List<ShaderTagId>() { new ShaderTagId("SRPDefaultUnlit") };
    private FilteringSettings _filteringSettings;
    private RenderStateBlock _renderStateBlock;
    private int _downSample;
    
    public RenderObjectsPass(RenderTargetHandle dest, LayerMask layer, int downSample)
    {
        _downSample = downSample;
        destination = dest;
        _filteringSettings = new FilteringSettings(RenderQueueRange.all, layer);
        _renderStateBlock = new RenderStateBlock(RenderStateMask.Depth);
        _renderStateBlock.depthState = new DepthState(true, CompareFunction.LessEqual);
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        var width = Mathf.Max(1, cameraTextureDescriptor.width >> _downSample);
        var height = Mathf.Max(1, cameraTextureDescriptor.height >> _downSample);
        RenderTextureDescriptor renderTextureDescriptor =
            new RenderTextureDescriptor(width, height, cameraTextureDescriptor.colorFormat, 24, 0);
        cmd.GetTemporaryRT(destination.id, renderTextureDescriptor); 
        ConfigureTarget(destination.Identifier());
        ConfigureClear(ClearFlag.All, Color.black);
    }


    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
        DrawingSettings drawingSettings =
            CreateDrawingSettings(_shaderTagList, ref renderingData, sortingCriteria);

        context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref _filteringSettings,
            ref _renderStateBlock);
        var cmd = CommandBufferPool.Get("SendDepthTexture");
        cmd.SetGlobalTexture("_BloomTextureDepth", destination.id, RenderTextureSubElement.Depth);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        base.FrameCleanup(cmd);
        cmd.ReleaseTemporaryRT(destination.id);
    }
}