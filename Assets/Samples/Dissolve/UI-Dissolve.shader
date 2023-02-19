// Copyright (C) 2022 Hatayama Masamichi
// Released under the MIT license
// https://opensource.org/licenses/mit-license.php

// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Applibot/UI/Dissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #include "../../Commoon/UvUtil.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #pragma multi_compile_local _ USE_ATLAS
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                half4 mask : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

            sampler2D _DissolveTex;
            float _DissolveRange;
            float _YAmount;
            float _YRange;
            float3 _GlowColor;
            float4 _Scroll;
            float _Distortion;

            float4 _MainTex_TexelSize;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                float4 vPosition = UnityObjectToClipPos(v.vertex);
                OUT.worldPosition = v.vertex;
                OUT.vertex = vPosition;

                float2 pixelSize = vPosition.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                float2 maskUV = (v.vertex.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                OUT.mask = half4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw,
                                 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                float2 uvForDissolveTex = IN.texcoord;
                #if defined(USE_ATLAS)
                    // atlasを使っている場合、0.2 〜 0.6 のような中途半端なuv値が渡ってくる
                    // それを0 〜 1の正規化された情報に変形する
                    uvForDissolveTex = AtlasUVtoMeshUV(IN.texcoord, _MainTex_TexelSize.zw, _textureRect);
                #endif
                
                // textureの下部を0とし、0から_yAmountまでの部分を表示します
                // _yRangeで指定された分、なだらかに変化します
                _YAmount = remap(_YAmount, 0, 1, -_YRange, 1 + _YRange);
                float fromY = _YAmount - _YRange;
                // yがfromYの時点から徐々に表示され、yが_yAmountのときに1を取るように変換
                float alphaY = remap(uvForDissolveTex.y, fromY, _YAmount, 0, 1);
                alphaY = saturate(alphaY);

                // dissovle用 textureスクロールのため
                uvForDissolveTex += _Scroll * _Time.x;
                half dissolveTexAlpha = tex2D(_DissolveTex, uvForDissolveTex).r;
                half reverseAlphaY = 1 - alphaY;

                // y方向境界部分の歪み用
                half2 uvDiff = IN.texcoord + half2(0, reverseAlphaY * dissolveTexAlpha * _Distortion);
                half4 color = IN.color * tex2D(_MainTex, uvDiff);

                #if defined(USE_ATLAS)
                    color *= IsInner(uvDiff, _MainTex_TexelSize.zw, _textureRect);
                #endif
                
                // 適用度がy方向に反映されるよう、y方向の情報と合成
                dissolveTexAlpha *= alphaY;
                _DissolveRange *= reverseAlphaY;
                if (dissolveTexAlpha < reverseAlphaY + _DissolveRange)
                {
                    color.rgb += _GlowColor;
                }
                
                if (dissolveTexAlpha < reverseAlphaY)
                {
                    color = 0;
                }
                
                return color;
            }
            ENDCG
        }
    }
}