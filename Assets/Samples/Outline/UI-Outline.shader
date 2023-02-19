// Copyright (C) 2022 Hatayama Masamichi
// Released under the MIT license
// https://opensource.org/licenses/mit-license.php

// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Applibot/UI/Outline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _scale ("scale", Float) = 1
        
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
        Blend [_SrcFactor] [_DstFactor]
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

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

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
                float4 mask : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

            float2 _scaleFactor;

            // custom properties
            float4 _MainTex_TexelSize;
            float4 _OutlineColor;

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
                OUT.mask = float4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw,
                                  0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));

                OUT.color = v.color * _Color;
                return OUT;
            }

            half luminance(float4 c)
            {
                half value = c.r * 0.298912 + c.g * 0.586611 + c.b * 0.114478;
                return value * c.a;
            }

            fixed4 sobel(v2f IN)
            {
                // uv座標での1pxは_MainTex_TexelSize.xy。uv atlasであってもこの単位は変わらない
                float dx = _MainTex_TexelSize.x * _scaleFactor.x;
                float dy = _MainTex_TexelSize.y * _scaleFactor.y;
                
                half4 c00rgba = tex2D(_MainTex, IN.texcoord + half2(-dx, -dy));
                half c00 = luminance(c00rgba);

                half4 c01rgba = tex2D(_MainTex, IN.texcoord + half2(-dx, 0.0));
                half c01 = luminance(c01rgba);

                half4 c02rgba = tex2D(_MainTex, IN.texcoord + half2(-dx, dy));
                half c02 = luminance(c02rgba);

                half4 c10rgba = tex2D(_MainTex, IN.texcoord + half2(0.0, -dy));
                half c10 = luminance(c10rgba);

                half4 c12rgba = tex2D(_MainTex, IN.texcoord + half2(0.0, dy));
                half c12 = luminance(c12rgba);

                half4 c20rgba = tex2D(_MainTex, IN.texcoord + half2(dx, -dy));
                half c20 = luminance(c20rgba);

                half4 c21rgba = tex2D(_MainTex, IN.texcoord + half2(dx, 0.0));
                half c21 = luminance(c21rgba);

                half4 c22rgba = tex2D(_MainTex, IN.texcoord + half2(dx, dy));
                half c22 = luminance(c22rgba);

                half sxColor = c00 * -1.0 + c10 * -2.0 + c20 * -1.0 + c02 + c12 * 2.0 + c22;
                half syColor = c00 * -1.0 + c01 * -2.0 + c02 * -1.0 + c20 + c21 * 2.0 + c22;

                half sxAlpha = c00rgba.a * -1.0 + c10rgba.a * -2.0 + c20rgba.a * -1.0 + c02rgba.a + c12rgba.a * 2.0 +
                    c22rgba.a;
                half syAlpha = c00rgba.a * -1.0 + c01rgba.a * -2.0 + c02rgba.a * -1.0 + c20rgba.a + c21rgba.a * 2.0 +
                    c22rgba.a;

                half outlineRGB = sqrt(sxColor * sxColor + syColor * syColor);
                half outlineAlpha = sqrt(sxAlpha * sxAlpha + syAlpha * syAlpha);
                half outline = max(outlineRGB, outlineAlpha);
                outline = saturate(outline);
                
                half4 color = half4(_OutlineColor.rgb, _OutlineColor.a * outline * IN.color.a);
                return color;
            }
           

            half4 frag(v2f IN) : SV_Target
            {
                half4 color = sobel(IN);

                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif
                
                return color;
            }
            ENDCG
        }
    }
}