// Copyright (C) 2022 Hatayama Masamichi
// Released under the MIT license
// https://opensource.org/licenses/mit-license.php

using UnityEngine;

namespace Applibot
{
    public class RadialBlurImage : CustomImageBase
    {
        public static readonly string SHADER_NAME = "Custom/UI/RadialBlur";

        public float BlurRadius = 30f;
        [Range(0, 30)] public int SampleCount = 10;

        private Shader _shader;
        private int _BlurRadiusId = Shader.PropertyToID("_BlurRadius");
        private int _SampleCountId = Shader.PropertyToID("_SampleCount");

        protected override void UpdateMaterial(Material baseMaterial)
        {
            if (material == null)
            {
                Shader shader;
                shader = Shader.Find(SHADER_NAME);
                material = new Material(shader);
            }
            
            material.SetFloat(_BlurRadiusId, BlurRadius);
            material.SetInt(_SampleCountId, SampleCount);
            
            float scale = 0.0005f;
            if (canvasScaler != null)
            {
                Vector2 texureSize = canvasScaler.referenceResolution;
                scale = 1f / Mathf.Max(texureSize.x, texureSize.y);
            }
            material.SetFloat("_scaleFactor", scale);
        }
    }
}