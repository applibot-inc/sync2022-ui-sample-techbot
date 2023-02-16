// Copyright (C) 2022 Hatayama Masamichi
// Released under the MIT license
// https://opensource.org/licenses/mit-license.php

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Applibot
{
    [ExecuteAlways]
    [RequireComponent(typeof(Graphic))]
    public class CustomImageBase : MonoBehaviour, IMaterialModifier
    {
        [NonSerialized] private Graphic _graphic;
        [NonSerialized] private Image _image;
        protected Material material;
        private CanvasScaler _canvasScaler;
        private int _textureRectId = Shader.PropertyToID("_textureRect");

        protected CanvasScaler canvasScaler
        {
            get
            {
                if (_canvasScaler == null)
                {
                    _canvasScaler = graphic.canvas.GetComponent<CanvasScaler>();
                }

                return _canvasScaler;
            }
        }

        public Graphic graphic
        {
            get
            {
                if (_graphic == null)
                {
                    _graphic = GetComponent<Graphic>();
                }

                return _graphic;
            }
        }

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            if (!isActiveAndEnabled || graphic == null)
            {
                return baseMaterial;
            }

            UpdateMaterial(baseMaterial);
            SetAtlasInfo();
            return material;
        }

        private void OnDidApplyAnimationProperties()
        {
            if (!isActiveAndEnabled || graphic == null)
            {
                return;
            }

            graphic.SetMaterialDirty();
        }

        protected virtual void UpdateMaterial(Material baseMaterial)
        {
        }
        
        private void SetAtlasInfo()
        {
            if (_image == null)
            {
                return;
            }
            
            if (!_image.sprite.packed)
            {
                material.DisableKeyword("USE_ATLAS");
                return;
            }

            Rect textureRect = _image.sprite.textureRect;
            Vector4 r = new Vector4(
                textureRect.x,
                textureRect.y,
                textureRect.width,
                textureRect.height);
            material.SetVector(_textureRectId, r);
            material.EnableKeyword("USE_ATLAS");
        }

        protected void OnEnable()
        {
            if (graphic == null)
            {
                return;
            }

            _image = graphic as Image;
            graphic.SetMaterialDirty();
        }

        protected void OnDisable()
        {
            if (material != null)
            {
                DestroyMaterial();
            }

            if (graphic != null)
            {
                graphic.SetMaterialDirty();
            }
        }

        protected void OnDestroy()
        {
            if (material != null)
            {
                DestroyMaterial();
            }
        }

        public void DestroyMaterial()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying == false)
            {
                DestroyImmediate(material);
                material = null;
                return;
            }
#endif

            Destroy(material);
            material = null;
        }


#if UNITY_EDITOR
        protected void OnValidate()
        {
            if (!isActiveAndEnabled || graphic == null)
            {
                return;
            }

            graphic.SetMaterialDirty();
        }
#endif
    }
}