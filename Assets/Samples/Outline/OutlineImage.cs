// Copyright (C) 2022 Hatayama Masamichi
// Released under the MIT license
// https://opensource.org/licenses/mit-license.php

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Applibot
{
    public class OutlineImage : CustomImageBase
    {
        [ColorUsage(true, true)] [SerializeField]
        private Color _outlineColor = Color.white;

        [SerializeField] private bool _isStatic = false;
        private int _OutlineColor = Shader.PropertyToID("_OutlineColor");
        private readonly int _SrcFactor = Shader.PropertyToID("_SrcFactor");
        private readonly int _DstFactor = Shader.PropertyToID("_DstFactor");

        protected override void UpdateMaterial(Material baseMaterial)
        {
            if (material == null)
            {
                Shader shader = Shader.Find("Applibot/UI/Outline");
                material = new Material(shader);
                material.hideFlags = HideFlags.HideAndDontSave;
            }

            material.SetColor(_OutlineColor, _outlineColor);
            
            material.SetInt(_SrcFactor, (int)BlendMode.SrcAlpha);
            material.SetInt(_DstFactor, (int)BlendMode.OneMinusSrcAlpha);
        }

        private void Awake()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            if (_isStatic)
            {
                Capture();
            }
        }

        public void Capture()
        {
            UpdateMaterial(null);
            material.SetInt(_SrcFactor, (int)BlendMode.One);
            material.SetInt(_DstFactor, (int)BlendMode.Zero);

            if (TryGetComponent(out RawImage rawImage))
            {
                Texture mainTexture = graphic.mainTexture;
                float w = (transform as RectTransform).rect.width;
                float h = (transform as RectTransform).rect.height;

                var rt = new RenderTexture((int)w, (int)h, 0, RenderTextureFormat.ARGBHalf);
                Graphics.Blit(mainTexture, rt, material);
                rawImage.texture = rt;

                DestroyMaterial();
                enabled = false;
            }
        }
    }
}