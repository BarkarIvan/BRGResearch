using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Project
{
    public class BloomRenderFeature : ScriptableRendererFeature
    {
        private RenderObjectsPass rendePass;
        private KawaseBlurPass kawaseBlurPass;
        private DrawFullScreenGridPass fullScreenGridPass;

        private Material _fullScreenMaterial;
        public RenderPassEvent PassEvent = RenderPassEvent.AfterRenderingTransparents;
        public KawaseBlurSettings blurSettings = new KawaseBlurSettings();
         public FullScreenGridSettings gridSettings = new FullScreenGridSettings();
        public LayerMask BloomLayer;

        private RenderTargetHandle _renderTexture;
        private RenderTargetHandle _depthTexture;
        private RenderTargetHandle _bluredTexture;

       

        public override void Create()
        {
            _fullScreenMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("BeresnevGames/FullScreenGrid_BloomSoftAdd"));//gridSettings.GridFinalMaterial;
            _renderTexture.Init("RenderObjectsPassDestination");
            _bluredTexture.Init("BlurTexture");
            rendePass = new RenderObjectsPass(_renderTexture, BloomLayer, blurSettings.DownSample);
            kawaseBlurPass = new KawaseBlurPass(blurSettings.DownSample, blurSettings.PassesCount,
                _renderTexture.Identifier(), _bluredTexture,blurSettings.BlurOffset);
            fullScreenGridPass = new DrawFullScreenGridPass(gridSettings.GridResolution, _fullScreenMaterial, _bluredTexture.Identifier());
            rendePass.renderPassEvent = PassEvent;
            kawaseBlurPass.renderPassEvent = PassEvent;
            fullScreenGridPass.renderPassEvent = PassEvent;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Game)
            {
                renderer.EnqueuePass(rendePass);
                renderer.EnqueuePass(kawaseBlurPass);
                renderer.EnqueuePass(fullScreenGridPass);
           }
        }
    }
}