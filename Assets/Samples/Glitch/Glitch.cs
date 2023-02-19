// Copyright (C) 2022 Hatayama Masamichi
// Released under the MIT license
// https://opensource.org/licenses/mit-license.php

//
// KinoGlitch - Video glitch effect
//
// Copyright (C) 2015 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using UnityEngine;
using UnityEngine.UI;

namespace Applibot
{
    [ExecuteAlways]
    public class Glitch : CustomImageBase
    {
        [SerializeField, Range(0, 2)]
        float _scanLineJitter = 0;
        
        [SerializeField, Range(1, 300)]
        float _JitterSize = 300;

        [SerializeField, Range(0, 1)]
        float _verticalJump = 0;

        [SerializeField, Range(0, 1)]
        float _horizontalShake = 0;

        [SerializeField, Range(-2, 2)]
        float _colorDrift = 0;

        [SerializeField] private Color _ScanlineColor;
        [SerializeField] private float _ScanlineSize = 1.2f;
        [SerializeField, Range(1, 5)] private float _ColorStrength = 1;

        float _verticalJumpTime;
        private RawImage _RawImage;
        
        protected override void UpdateMaterial(Material baseMaterial)
        {
            if (material == null)
            {
                material = new Material(Shader.Find("Custom/UI/Glitch"));
                material.hideFlags = HideFlags.DontSave;
            }

            _verticalJumpTime += Time.deltaTime * _verticalJump * 11.3f;

            var sl_thresh = Mathf.Clamp01(1.0f - _scanLineJitter * 1.2f);
            var sl_disp = 0.002f + Mathf.Pow(_scanLineJitter, 3) * 0.05f;
            material.SetVector("_ScanLineJitter", new Vector2(sl_disp, sl_thresh));
            material.SetFloat("_JitterSize", _JitterSize);

            var vj = new Vector2(_verticalJump, _verticalJumpTime);
            material.SetVector("_VerticalJump", vj);

            material.SetFloat("_HorizontalShake", _horizontalShake * 0.2f);
            material.SetFloat("_ColorDriftAmount", _colorDrift * 0.04f);
            
            material.SetColor("_ScanlineColor", _ScanlineColor);
            material.SetFloat("_ScanlineSize", _ScanlineSize);
            material.SetFloat("_ColorStrength", _ColorStrength);
        }
    }
}
