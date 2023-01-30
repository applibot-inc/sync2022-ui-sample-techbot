using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using Unity.Collections;
using UnityEngine.UI;

namespace Applibot
{
    public class SpriteVertexPositionChanger : MonoBehaviour, IMaterialModifier
    {
        public float scale = 1.5f;
        [NonSerialized] private Image _image;
        private int _textureRectId = Shader.PropertyToID("_textureRect");

        private void Start()
        {
            _image = GetComponent<Image>();

            // useSpriteMeshをtrueにする必要アリ
            _image.useSpriteMesh = true;
            Sprite sprite = _image.sprite;
            if (sprite.packed)
            {
                // sprite atlasを使っている場合はサイズを調整
                _image.rectTransform.sizeDelta *= scale;
            }
            
            ChangeMeshScale(sprite);
        }

        private void ChangeMeshScale(Sprite sprite)
        {
            // spriteの頂点を取得
            NativeSlice<Vector3> vertices = sprite.GetVertexAttribute<Vector3>(VertexAttribute.Position);
            NativeArray<Vector3> copy = new NativeArray<Vector3>(vertices.Length, Allocator.Temp);
            
            // 頂点を拡大
            for (int i = 0; i < vertices.Length; i++)
            {
                copy[i] = vertices[i] * scale;
            }

            sprite.SetVertexAttribute(VertexAttribute.Position, copy);
            copy.Dispose();
        }

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            SetAtlasInfo(baseMaterial);
            return baseMaterial;
        }

        private void SetAtlasInfo(Material material)
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
    }
}