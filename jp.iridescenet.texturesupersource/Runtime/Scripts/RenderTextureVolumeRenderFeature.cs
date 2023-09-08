using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TextureSuperSource
{
    public class RenderTextureVolumeRenderFeature : ScriptableRendererFeature
    {
        private sealed class Pass : ScriptableRenderPass
        {
            private const string ProfilerTag = "RenderTextureWithPostProcess";
            private static readonly ProfilingSampler ProfilingSampler = new(ProfilerTag);

            private RenderTexture _input;
            private Material _material;
            private int _propertyIdColor;

            public Pass(RenderPassEvent evt)
            {
                profilingSampler = new ProfilingSampler(nameof(Pass));
                renderPassEvent = evt;
                _propertyIdColor = Shader.PropertyToID("_Color");
            }

            public void Setup(Material material, RenderTexture input)
            {
                _input = input;
                _material = material;
            }

            public override void Execute(
                ScriptableRenderContext context,
                ref RenderingData renderingData)
            {
                var cmd = CommandBufferPool.Get();
                using (new ProfilingScope(cmd, ProfilingSampler))
                {
                    _material.SetTexture("_MainTex", _input);
                    cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                    var handle = renderingData.cameraData.renderer.cameraColorTargetHandle;
                    cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _material);
                    cmd.SetViewProjectionMatrices(renderingData.cameraData.camera.worldToCameraMatrix, renderingData.cameraData.camera.projectionMatrix);
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        [SerializeField] private RenderTexture input;

        private Material _material;
        
        private Pass _pass;

        public override void Create()
        {
            _material = CoreUtils.CreateEngineMaterial(Shader.Find("Sprites/Default"));
            _pass = new Pass(RenderPassEvent.BeforeRenderingPostProcessing);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            _pass.Setup(_material, input);
            renderer.EnqueuePass(_pass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(_material);
            base.Dispose(disposing);
        }
    }
}
