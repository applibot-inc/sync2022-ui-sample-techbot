// Copyright (C) 2022 Hatayama Masamichi
// Released under the MIT license
// https://opensource.org/licenses/mit-license.php

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using Unity.Collections;
using UnityEngine.UI;

namespace Applibot
{
    public class SpriteVertexPositionChanger : MonoBehaviour
    {
        public float scale = 1.5f;
        [NonSerialized] private Image _image;

        private void Start()
        {
            _image = GetComponent<Image>();
            if (_image == null)
            {
                Debug.LogError("Imageコンポーネントが必要です");
                return;
            }

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
            NativeSlice<Vector3> vertices = sprite.GetVertexAttribute<Vector3>(VertexAttribute.Position);
            NativeArray<Vector3> copy = new NativeArray<Vector3>(vertices.Length, Allocator.Temp);
            for (int i = 0; i < vertices.Length; i++)
            {
                copy[i] = vertices[i] * scale;
            }

            sprite.SetVertexAttribute(VertexAttribute.Position, copy);
            copy.Dispose();
        }
    }
}