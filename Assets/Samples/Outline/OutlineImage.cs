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

        private Image _image;

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
            }

            material.SetColor(_OutlineColor, _outlineColor);
            material.SetInt(_SrcFactor, (int)BlendMode.SrcAlpha);
            material.SetInt(_DstFactor, (int)BlendMode.OneMinusSrcAlpha);

            if (canvasScaler != null)
            {
                Vector2 canvasResolution = canvasScaler.referenceResolution;
                Vector2 texureSize = Vector2.one;

                if (_image != null && _image.sprite.packed)
                {
                    // sprite atlasが使われている場合
                    Rect r = _image.sprite.textureRect;
                    texureSize = new Vector2(r.width, r.height);
                }
                else
                {
                    Texture mainTexture = graphic.mainTexture;
                    texureSize = new Vector2(mainTexture.width, mainTexture.height);
                }

                // texture sizeによって線の太さに差が出ないよう、canvasに対しての比率をshaderで掛け合わせる
                float x = texureSize.x / canvasResolution.x;
                float y = texureSize.y / canvasResolution.y;
                material.SetVector("_scaleFactor", new Vector4(x, y));
            }
        }

        private void Awake()
        {
            _image = graphic as Image;

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