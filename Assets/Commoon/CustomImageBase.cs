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
        protected Material material;

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
            if (!isActiveAndEnabled || _graphic == null)
            {
                return baseMaterial;
            }

            UpdateMaterial(baseMaterial);
            return material;
        }

        private void OnDidApplyAnimationProperties()
        {
            if (!isActiveAndEnabled || _graphic == null)
            {
                return;
            }

            _graphic.SetMaterialDirty();
        }

        protected virtual void UpdateMaterial(Material baseMaterial)
        {
        }

        protected void OnEnable()
        {
            if (graphic == null)
            {
                return;
            }

            _graphic.SetMaterialDirty();
        }

        protected void OnDisable()
        {
            if (material != null)
            {
                DestroyMaterial();
            }

            if (graphic != null)
            {
                _graphic.SetMaterialDirty();
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