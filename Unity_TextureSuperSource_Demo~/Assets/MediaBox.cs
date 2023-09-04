using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public enum CropMode
{
    Fit,
    Fill,
    Stretch,
    Manual
}

[ExecuteAlways]
public class MediaBox : MonoBehaviour
{
    [SerializeField] private Vector2 boxSize;
    [SerializeField] private CropMode cropMode;
    [SerializeField] private RawImage rawImage;
    [SerializeField] Texture inputMedia;
    [SerializeField] private Vector2 mediaPosition;
    [SerializeField] private RenderTexture outputMedia;
    
    void Start()
    {
        
    }

    public void Init()
    {
        var material = new Material(Resources.Load<Shader>("TextureSuperSource/SuperSourceBoxShader.shader"));
        material.SetTexture("_InputMedia", inputMedia);
    }
    // Update is called once per frame
    void Update()
    {
        rawImage.texture = inputMedia;
        rawImage.rectTransform.anchoredPosition = mediaPosition;
        rawImage.rectTransform.sizeDelta = boxSize;
    }
}
