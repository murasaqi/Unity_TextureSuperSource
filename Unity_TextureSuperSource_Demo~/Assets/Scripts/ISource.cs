using UnityEngine.Rendering;

namespace TextureSuperSource
{
    public interface ISource
    {
        public void RegisterSceneData(float sceneWidth, float sceneHeight);
        public void AddRenderQueue(CommandBuffer commandBuffer);
    }
}
