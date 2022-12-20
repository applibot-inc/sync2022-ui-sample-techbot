// Copyright (C) 2022 Hatayama Masamichi
// Released under the MIT license
// https://opensource.org/licenses/mit-license.php

using UnityEngine;
using UnityEngine.Rendering;

namespace Applibot
{
    public class UICustomBlend : CustomImageBase
    {
        public enum MyCustomBlendMode : byte
        {
            Normal = 0,
            LinearDodge,
            Darken,
            Multiply,
            Subtract,
            Lighten,
        }
        
        [SerializeField] private MyCustomBlendMode _blendMode = MyCustomBlendMode.Normal;

        private readonly int _SrcFactor = Shader.PropertyToID("_SrcFactor");
        private readonly int _DstFactor = Shader.PropertyToID("_DstFactor");
        private readonly int _BlendOp = Shader.PropertyToID("_BlendOp");

        protected override void UpdateMaterial(Material baseMaterial)
        {
            if (material == null)
            {
                material = new Material(Shader.Find("Applibot/UI/CustomBlend"));
                material.hideFlags = HideFlags.HideAndDontSave;
            }

            // keywordを全てoffに
            material.enabledKeywords = new LocalKeyword[] { };

            switch (_blendMode)
            {
                case MyCustomBlendMode.Normal: // 通常 (プリマルチプライドの透明)
                    material.SetInt(_SrcFactor, (int)BlendMode.One);
                    material.SetInt(_DstFactor, (int)BlendMode.OneMinusSrcAlpha);
                    material.SetInt(_BlendOp, (int)BlendOp.Add);
                    break;

                case MyCustomBlendMode.LinearDodge: // 覆い焼き(リニア) - 加算
                    material.SetInt(_SrcFactor, (int)BlendMode.SrcAlpha);
                    material.SetInt(_DstFactor, (int)BlendMode.One);
                    material.SetInt(_BlendOp, (int)BlendOp.Add);
                    break;

                case MyCustomBlendMode.Multiply: // 乗算
                    material.SetInt(_SrcFactor, (int)BlendMode.Zero);
                    material.SetInt(_DstFactor, (int)BlendMode.SrcColor);
                    material.SetInt(_BlendOp, (int)BlendOp.Add);
                    break;

                case MyCustomBlendMode.Darken: // 比較 (暗) 
                    material.SetInt(_SrcFactor, (int)BlendMode.One);
                    material.SetInt(_DstFactor, (int)BlendMode.One);
                    material.SetInt(_BlendOp, (int)BlendOp.Min);
                    break;

                case MyCustomBlendMode.Subtract: // 減算
                    material.SetInt(_SrcFactor, (int)BlendMode.One);
                    material.SetInt(_DstFactor, (int)BlendMode.One);
                    material.SetInt(_BlendOp, (int)BlendOp.ReverseSubtract);
                    break;

                case MyCustomBlendMode.Lighten: // 比較 (明)
                    material.SetInt(_SrcFactor, (int)BlendMode.One);
                    material.SetInt(_DstFactor, (int)BlendMode.One);
                    material.SetInt(_BlendOp, (int)BlendOp.Max);
                    break;
            }

            material.EnableKeyword(_blendMode.ToString());
        }
    }
}