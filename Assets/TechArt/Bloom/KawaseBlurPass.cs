using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Project
{
    public class KawaseBlurPass : ScriptableRenderPass
    {
        private int tmpBlurTexId1 = Shader.PropertyToID("_BlurRT1");
        private int tmpBlurTexId2 = Shader.PropertyToID("_BlurRt2");
        private RenderTargetIdentifier tmpBlurRT1;
        private RenderTargetIdentifier tmpBlurRT2;
        private RenderTargetIdentifier _source;
        private RenderTargetHandle _destination;

        private int passesCount;
        private int downSample;
        private Material blurMaterial;

        private float _blurOffset;
        private Vector2 _halfPixel;

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            var width = Mathf.Max(1, cameraTextureDescriptor.width >> downSample);
            var height = Mathf.Max(1, cameraTextureDescriptor.height >> downSample);
            var blurTextureDescriptor = new RenderTextureDescriptor(width/2, height/2, cameraTextureDescriptor.colorFormat, 32, 0);
            var secondBlurTextureDescriptor = new RenderTextureDescriptor(width/4, height/4, cameraTextureDescriptor.colorFormat, 32, 0);
            tmpBlurRT1 = new RenderTargetIdentifier(tmpBlurTexId1);
            tmpBlurRT2 = new RenderTargetIdentifier(tmpBlurTexId2);

            cmd.GetTemporaryRT(tmpBlurTexId1, blurTextureDescriptor, FilterMode.Bilinear);
            cmd.GetTemporaryRT(tmpBlurTexId2, secondBlurTextureDescriptor, FilterMode.Bilinear);
            cmd.GetTemporaryRT(_destination.id, blurTextureDescriptor, FilterMode.Bilinear);
            ConfigureTarget(_destination.Identifier());

            cmd.SetGlobalFloat("_BlurOffset", _blurOffset);
            cmd.SetGlobalVector("_HalfPixel", _halfPixel);
        }

        public KawaseBlurPass(int downSample, int count, RenderTargetIdentifier source, RenderTargetHandle destination, float offset)
        {
            _source = source;
            _destination = destination;
            blurMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("BeresnevGames/KawaseBlur"));
            this.downSample = downSample;
            passesCount = count;
            _blurOffset = offset;
            _halfPixel = new Vector2(0.5f, 0.5f) / (new Vector2(Screen.width, Screen.height) * 0.5f);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("BlurPass");
            if (passesCount > 0)
            {
               
                for (int i = 0; i < passesCount - 1; i++)
                {
                    cmd.Blit(_source, tmpBlurRT1, blurMaterial, 0);
                    cmd.Blit(tmpBlurRT1, tmpBlurRT2, blurMaterial, 0);
                    cmd.Blit(tmpBlurRT2, tmpBlurRT1, blurMaterial, 1);
                    cmd.Blit(tmpBlurRT1, _source, blurMaterial, 1);
                }
                cmd.Blit(_source, _destination.Identifier());
            }
            else
            {
                cmd.Blit(_source, _destination.Identifier());
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tmpBlurTexId1);
            cmd.ReleaseTemporaryRT(tmpBlurTexId2);
            cmd.ReleaseTemporaryRT(_destination.id);
        }
    }
}