// Copyright (C) 2022 Hatayama Masamichi
// Released under the MIT license
// https://opensource.org/licenses/mit-license.php

using UnityEngine;

namespace Applibot
{
    public class NormalDissolve : CustomImageBase
    {
        [SerializeField] private Texture2D _dissovleTex;
        [SerializeField, Range(0, 1)] private float _dissolveAmount;
        [SerializeField, Range(0, 1)] private float _dissolveRange;

        [SerializeField, ColorUsage(false, true)]
        private Color _glowColor;

        private int _dissolveTexId = Shader.PropertyToID("_DissolveTex");
        private int _dissolveRangeId = Shader.PropertyToID("_DissolveRange");
        private int _dissolveAmountId = Shader.PropertyToID("_DissolveAmount");
        private int _glowColorId = Shader.PropertyToID("_GlowColor");

        protected override void UpdateMaterial(Material baseMaterial)
        {
            // マテリアル複製
            if (material == null)
            {
                Shader s = Shader.Find("Applibot/UI/NormalDissolve");
                material = new Material(s);
                material.CopyPropertiesFromMaterial(baseMaterial);
                material.hideFlags = HideFlags.HideAndDontSave;
            }

            material.SetTexture(_dissolveTexId, _dissovleTex);
            material.SetFloat(_dissolveAmountId, _dissolveAmount);
            material.SetFloat(_dissolveRangeId, _dissolveRange);
            material.SetColor(_glowColorId, _glowColor);
        }
    }
}