using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace TextureSuperSource
{
    [ExecuteAlways]
    public class SuperSourceScene : MonoBehaviour
    {
        [SerializeField] private string sceneName = "SuperSourceScene";
        [SerializeField] private RenderTexture output;

        [SerializeField] private List<TextureSource> sources;

        private CommandBuffer _commandBuffer;

        private void OnEnable()
        {
            _commandBuffer = new CommandBuffer();
            _commandBuffer.name = name;
            _commandBuffer.SetRenderTarget(output, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            _commandBuffer.SetViewMatrix(Matrix4x4.LookAt(new Vector3(0, 0, -5), new Vector3(0, 0, 1), Vector3.up));
            _commandBuffer.SetProjectionMatrix(Matrix4x4.Ortho(-output.width / 2, output.width / 2, -output.height / 2, output.height / 2, 0.01f, 100));
            
            RegisterDataToSources();
        }

        private void OnValidate()
        {
            RegisterDataToSources();
        }

        private void LateUpdate()
        {
            _commandBuffer.ClearRenderTarget(true, true, Color.black);

            foreach (var mediaSource in sources)
            {
                mediaSource.AddRenderQueue(_commandBuffer);
            }

            Graphics.ExecuteCommandBuffer(_commandBuffer);
        }

        private void OnDisable()
        {
            _commandBuffer?.Dispose();
            _commandBuffer = null;
        }

        private void RegisterDataToSources()
        {
            foreach (var source in sources)
            {
                source.RegisterSceneData(output.width, output.height);
            }
        }
    }
}