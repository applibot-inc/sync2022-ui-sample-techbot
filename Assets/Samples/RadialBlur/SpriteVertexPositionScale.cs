using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using Unity.Collections;
using UnityEngine.UI;

namespace Applibot
{
    public class SpriteVertexPositionScale : MonoBehaviour, IMaterialModifier
    {
        [NonSerialized] private Image _image;
        private Material _material;

        public float scale = 1.5f;

        
        private void Start()
        {
            _image = GetComponent<Image>();

            // useSpriteMeshをtrueにする必要アリ
            _image.useSpriteMesh = true;
            Sprite sprite = _image.sprite;
            // spriteの頂点を取得
            NativeSlice<Vector3> vertices = sprite.GetVertexAttribute<Vector3>(VertexAttribute.Position);
            var copy = new NativeArray<Vector3>(vertices.Length, Allocator.Temp);
            // 頂点を拡大
            for (int j = 0, m = vertices.Length; j < m; j++)
            {
                copy[j] = vertices[j] * scale;
            }

            sprite.SetVertexAttribute(VertexAttribute.Position, copy);
            copy.Dispose();

            // sprite atlasを使っている場合はサイズを調整
            if (sprite.packed)
            {
                _image.rectTransform.sizeDelta *= scale;
            }
        }

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            SetMaskForScaling(baseMaterial);
            return baseMaterial;
        }

        private int _textureRectId = Shader.PropertyToID("_textureRect");

        private void SetMaskForScaling(Material material)
        {
            if (_image == null)
            {
                return;
            }

            Rect textureRect = _image.sprite.textureRect;
            Vector4 r = new Vector4(
                textureRect.x,
                textureRect.y,
                textureRect.width,
                textureRect.height);

            material.SetVector(_textureRectId, r);
            if (_image.sprite.packed)
            {
                material.EnableKeyword("USE_ATLAS");
            }
            else
            {
                material.DisableKeyword("USE_ATLAS");
            }
        }
    }
}