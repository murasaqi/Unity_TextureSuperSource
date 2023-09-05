using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace TextureSuperSource
{
    public enum BoundingBoxType
    {
        ScaleToInnerBounds,
        ScaleToOuterBounds,
        StretchToBounds,
        ScaleToWidthOfBounds,
        ScaleToHeightOfBounds,
    }

    public enum TransformMode
    {
        Transform,
        RectTransform
    }

    [Serializable]
    public struct Crop
    {
        public float left;
        public float right;
        public float top;
        public float bottom;
    }

    [ExecuteAlways]
    public class TextureSource : MonoBehaviour, ISource
    {
        [SerializeField] private Texture inputTexture;
        [SerializeField] private BoundingBoxType boundingBoxType;
        [SerializeField] private Material material;
        [SerializeField] private TransformMode transformMode;
        [SerializeField] private Crop crop;

        private Mesh _mesh;
        private Matrix4x4 _matrix;

        private Material _cloneMaterial;

        private RectTransform _rectTransform;

        private float _sceneWidth;
        private float _sceneHeight;

        private float _textureWidth;
        private float _textureHeight;

        private void OnEnable()
        {
            ResetMesh();

            _cloneMaterial = new Material(material);
            _cloneMaterial.SetTexture("_MainTex", inputTexture);

            _rectTransform = transformMode == TransformMode.RectTransform ? GetComponent<RectTransform>() : null;

            _textureWidth = inputTexture.width;
            _textureHeight = inputTexture.height;
        }

        public void RegisterSceneData(float sceneWidth, float sceneHeight)
        {
            _sceneWidth = sceneWidth;
            _sceneHeight = sceneHeight;
        }

        public void AddRenderQueue(CommandBuffer commandBuffer)
        {
            _matrix = transform.localToWorldMatrix;
            
            if (transformMode == TransformMode.RectTransform)
            {
                var vertices = new Vector3[4];
                _rectTransform.GetLocalCorners(vertices);
                for(var i = 0; i < vertices.Length; i++)
                {
                    vertices[i].z = 0;
                }
                SetVertices(vertices);
            }
            else
            {
                var sceneWidthHalf = _sceneWidth / 2;
                var sceneHeightHalf = _sceneHeight / 2;
                var vertices = new Vector3[4]
                {
                    new Vector3(-sceneWidthHalf, -sceneHeightHalf, 0),
                    new Vector3(-sceneWidthHalf, sceneHeightHalf, 0),
                    new Vector3(sceneWidthHalf, sceneHeightHalf, 0),
                    new Vector3(sceneWidthHalf, -sceneHeightHalf, 0),
                };
                SetVertices(vertices);
            }
            
            _mesh.RecalculateBounds();
            
            ApplyBoundingBoxTypeToVertices();
            ApplyCrop();

            commandBuffer.DrawMesh(_mesh, _matrix, _cloneMaterial);
        }

        private void ResetMesh()
        {
            _mesh = new Mesh();
            
            var sceneWidthHalf = _sceneWidth / 2;
            var sceneHeightHalf = _sceneHeight / 2;
            _mesh.SetVertices(new Vector3[]
            {
                new Vector3(-sceneWidthHalf, -sceneHeightHalf, 0),
                new Vector3(-sceneWidthHalf, sceneHeightHalf, 0),
                new Vector3(sceneWidthHalf, sceneHeightHalf, 0),
                new Vector3(sceneWidthHalf, -sceneHeightHalf, 0),
            });
            _mesh.SetUVs(0, new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
            });
            _mesh.SetIndices(new int[]
            {
                0, 1, 2, 0, 2, 3
            }, MeshTopology.Triangles, 0);
        }

        private void SetVertices(Vector3[] vertices)
        {
            if (vertices.Length != 4)
                throw new Exception("Vertex positions must be 4");

            _mesh.SetVertices(vertices);
        }

        private void ApplyBoundingBoxTypeToVertices()
        {
            var vertices = _mesh.vertices;
            var boundsAspectRatio = (_mesh.bounds.size.x * transform.lossyScale.x) / (_mesh.bounds.size.y * transform.lossyScale.y);
            var textureAspectRatio = _textureWidth / _textureHeight;
            var isTextureMoreHorizontal = boundsAspectRatio < textureAspectRatio;
            var center = Vector2.zero;
            
            if (transformMode == TransformMode.RectTransform)
            {
                if (_rectTransform.pivot != new Vector2(0.5f, 0.5f))
                {
                    var scaleX = _rectTransform.lossyScale.x;
                    var scaleY = _rectTransform.lossyScale.y;
                    var x = _rectTransform.sizeDelta.x / 2f * scaleX;
                    var y = _rectTransform.sizeDelta.y / 2f * scaleY;
                    center.x += Mathf.Lerp(x, -x, _rectTransform.pivot.x);
                    center.y += Mathf.Lerp(y, -y, _rectTransform.pivot.y);
                }
                
                for(var i = 0; i < vertices.Length; i++)
                {
                    vertices[i].x -= center.x;
                    vertices[i].y -= center.y;
                }
            }

            for (var i = 0; i < vertices.Length; i++)
            {
                switch (boundingBoxType)
                {
                    case BoundingBoxType.ScaleToInnerBounds:
                        vertices[i].x *= isTextureMoreHorizontal ? 1f : 1f / boundsAspectRatio;
                        vertices[i].y *= isTextureMoreHorizontal ? boundsAspectRatio : 1f;

                        vertices[i].x *= isTextureMoreHorizontal ? 1f : textureAspectRatio;
                        vertices[i].y *= isTextureMoreHorizontal ? 1f / textureAspectRatio : 1f;
                        break;
                
                    case BoundingBoxType.ScaleToOuterBounds:
                        vertices[i].x *= isTextureMoreHorizontal ? 1f / boundsAspectRatio : 1f;
                        vertices[i].y *= isTextureMoreHorizontal ? 1f : boundsAspectRatio;

                        vertices[i].x *= isTextureMoreHorizontal ? textureAspectRatio : 1f;
                        vertices[i].y *= isTextureMoreHorizontal ? 1f : 1f / textureAspectRatio;
                        break;
                    
                    case BoundingBoxType.StretchToBounds:
                        break;
                    
                    case BoundingBoxType.ScaleToWidthOfBounds:
                        vertices[i].y *= boundsAspectRatio;
                        vertices[i].x *= textureAspectRatio;
                        break;
                    
                    case BoundingBoxType.ScaleToHeightOfBounds:
                        vertices[i].x /= boundsAspectRatio;
                        vertices[i].x *= textureAspectRatio;
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (transformMode == TransformMode.RectTransform)
            {
                for(var i = 0; i < vertices.Length; i++)
                {
                    vertices[i].x += center.x;
                    vertices[i].y += center.y;
                }
            }
            
            _mesh.SetVertices(vertices);
        }

        private void ApplyCrop()
        {
            var left = crop.left / _textureWidth;
            var right = 1.0f - crop.right / _textureWidth;
            var top = crop.bottom / _textureHeight;
            var bottom = 1.0f - crop.top / _textureHeight;
            
            _mesh.SetUVs(0, new Vector2[]
            {
                new Vector2(left, top),
                new Vector2(left, bottom),
                new Vector2(right, bottom),
                new Vector2(right, top),
            });
        }
    }
}