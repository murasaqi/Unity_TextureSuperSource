using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace TextureSuperSource
{
    [ExecuteAlways]
    public class SuperSourceScene : MonoBehaviour
    {
        [SerializeField] private RenderTexture output;

        [SerializeField] private List<TextureSource> sources;

        private CommandBuffer _commandBuffer;

        private bool _Initialized;

        private void OnEnable()
        {
            Initialize();
        }

        private void OnDisable()
        {
            _commandBuffer?.Release();
        }

        private void OnValidate()
        {
            Initialize();
        }

        private void LateUpdate()
        {
            if (Time.frameCount > 10 && !_Initialized)
            {
                _Initialized = true;

                Initialize();
            }

            if (!Application.isPlaying)
            {
                Initialize();
            }
            
            Graphics.ExecuteCommandBuffer(_commandBuffer);
        }

        private void RegisterDataToSources()
        {
            foreach (var source in sources)
            {
                source.RegisterSceneData(output.width, output.height);
            }
        }

        private void Initialize()
        {
            RegisterDataToSources();
            
            _commandBuffer = new CommandBuffer();
            _commandBuffer.name = name;
            
            _commandBuffer.SetRenderTarget(output, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            _commandBuffer.SetViewMatrix(Matrix4x4.LookAt(new Vector3(0, 0, -5), new Vector3(0, 0, 1), Vector3.up));
            _commandBuffer.SetProjectionMatrix(Matrix4x4.Ortho(-output.width / 2, output.width / 2, -output.height / 2, output.height / 2, 0.01f, 100));
            
            _commandBuffer.ClearRenderTarget(true, true, Color.black);
            
            foreach (var textureSources in sources)
            {
                textureSources.AddRenderQueue(_commandBuffer);
            }
        }
        
        /*
        [ContextMenu("Add Postprocess Feature")]
        private void AddPostprocessFeature()
        { 
            var camera = gameObject.AddComponent<Camera>();
            var rt = new RenderTexture(output);
            camera.targetTexture = rt;
            
            var rtPath = EditorUtility.SaveFilePanelInProject("Save RenderTexture", $"{name}", "renderTexture", "", "Assets");
            if (string.IsNullOrEmpty(rtPath))
            {
                return;
            }
            
            AssetDatabase.CreateAsset(rt, rtPath);
            AssetDatabase.Refresh();

            var urd = ScriptableObject.CreateInstance<UniversalRendererData>();
            var rtvrf = ScriptableObject.CreateInstance<RenderTextureVolumeRenderFeature>();
            rtvrf.SetRenderTexture(rt);
            rtvrf.name = $"{name}_RenderTextureVolumeRenderFeature";
            urd.rendererFeatures.Add(rtvrf);
            
            var rendererDataPath = EditorUtility.SaveFilePanelInProject("Save Universal Renderer Data", $"{name}", "asset", "", "Assets");
            if (string.IsNullOrEmpty(rendererDataPath))
            {
                return;
            }
            
            AssetDatabase.CreateAsset(urd, rendererDataPath);
            AssetDatabase.Refresh();

            var urppa = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urppa == null)
            {
                Debug.Log("Not Found UniversalRenderPipelineAsset");
                return;
            }
            
            var field = urppa.GetType().GetField("m_Renderers", BindingFlags.NonPublic|BindingFlags.Instance);;
            if (field == null)
            {
                Debug.Log("Not Found Field");
                return;
            }

            if (field.GetValue(urppa) is not ScriptableRenderer[] renderers)
            {
                Debug.Log("Not Found Renderers");
                return;
            }
                
            var newRendererIndex = renderers.Length;
            var newRenderers = new ScriptableRenderer[renderers.Length + 1];
            Array.Copy(renderers, newRenderers, renderers.Length);
            var newRenderer = new UniversalRenderer(urd);
            newRenderers[newRendererIndex] = newRenderer;
            field.SetValue(urppa, newRenderers);

            var additionalData = camera.GetUniversalAdditionalCameraData();
            additionalData.SetRenderer(newRendererIndex);
            additionalData.renderPostProcessing = true;
            
            Debug.Log("Add Postprocess Feature Complete");
        }
        */
        
    }
}