using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public enum ObjectFitMode
{
    Fit,
    Fill,
    Stretch,
    Manual
}

[ExecuteAlways]
public class MediaSource : MonoBehaviour
{
    [SerializeField] private string sourceName = "MediaSource";
    [SerializeField] private Texture inputMedia;
    [SerializeField] private ObjectFitMode objectFitMode;
    [SerializeField] private Material material;

    private Mesh _mesh;
    private Matrix4x4 _matrix;

    private Material _cloneMaterial;
    
    private void OnEnable()
    {
        _mesh = new Mesh();
        _mesh.SetVertices(new Vector3[]
        {
            new Vector3(-1, -1, 0),
            new Vector3(1, -1, 0),
            new Vector3(1, 1, 0),
            new Vector3(-1, 1, 0),
        });
        _mesh.SetUVs(0, new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1),
        });
        _mesh.SetIndices(new int[]
        {
            0, 1, 2, 0, 2, 3
        }, MeshTopology.Triangles, 0);
        
        _cloneMaterial = new Material(material);
        _cloneMaterial.SetTexture("_MainTex", inputMedia);
    }

    public void AddRenderQueue(CommandBuffer commandBuffer)
    {
        _matrix = Matrix4x4.TRS(new Vector3(transform.localPosition.x, transform.localPosition.y, 5), transform.localRotation, transform.localScale);
        
        commandBuffer.DrawMesh(_mesh, _matrix, _cloneMaterial);
    }
}