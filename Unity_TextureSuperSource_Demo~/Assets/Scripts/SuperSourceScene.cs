using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class SuperSourceScene : MonoBehaviour
{
    [SerializeField] private string sceneName = "SuperSourceScene";
    [SerializeField] private RenderTexture output;
    
    [SerializeField] private List<MediaSource> mediaSources;
    
    private CommandBuffer _commandBuffer;
    
    private void OnEnable()
    {
        _commandBuffer = new CommandBuffer();
        _commandBuffer.name = "SuperSourceScene";
        _commandBuffer.SetRenderTarget(output, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
        _commandBuffer.SetViewMatrix(Matrix4x4.LookAt(new Vector3(0, 0, 0), new Vector3(0, 0, -1), Vector3.up));
        _commandBuffer.SetProjectionMatrix(Matrix4x4.Ortho(-1, 1, -1, 1, 1, 100));
    }

    private void LateUpdate()
    {
        _commandBuffer.ClearRenderTarget(true, true, Color.black);
        
        foreach (var mediaSource in mediaSources)
        {
            Debug.Log(mediaSource.name);
            mediaSource.AddRenderQueue(_commandBuffer);
        }
        
        Graphics.ExecuteCommandBuffer(_commandBuffer);
    }
}