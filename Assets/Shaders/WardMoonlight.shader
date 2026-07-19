Shader "MonsterMaster/UI/WardMoonlight"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _NightColor ("Night Color", Color) = (0.16,0.31,0.42,1)
        _MoonColor ("Moonlight Color", Color) = (0.54,0.82,1,1)
        _WarmColor ("Bedside Lamp Color", Color) = (1,0.66,0.28,1)
        _NightAmount ("Night Amount", Range(0,1)) = 0.48
        _MoonIntensity ("Moonlight Intensity", Range(0,2)) = 0.44
        _WarmIntensity ("Warm Light Intensity", Range(0,2)) = 0.24
        _MotionSpeed ("Motion Speed", Range(0,2)) = 0.07
        _ShadowStrength ("Window Shadow Strength", Range(0,1)) = 0.32

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
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
            Name "WardMoonlight"

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
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            fixed4 _NightColor;
            fixed4 _MoonColor;
            fixed4 _WarmColor;
            float _NightAmount;
            float _MoonIntensity;
            float _WarmIntensity;
            float _MotionSpeed;
            float _ShadowStrength;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            float softRect(float2 uv, float2 minUV, float2 maxUV, float feather)
            {
                float2 inside = smoothstep(minUV, minUV + feather, uv)
                              * (1.0 - smoothstep(maxUV - feather, maxUV, uv));
                return inside.x * inside.y;
            }

            float band(float value, float center, float halfWidth, float feather)
            {
                return 1.0 - smoothstep(halfWidth, halfWidth + feather, abs(value - center));
            }

            float softEllipse(float2 uv, float2 center, float2 radius)
            {
                float distanceToCenter = length((uv - center) / radius);
                return 1.0 - smoothstep(0.45, 1.0, distanceToCenter);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.texcoord;
                fixed4 source = (tex2D(_MainTex, uv) + _TextureSampleAdd) * i.color;

                // Preserve luminance while moving the ambient palette toward a calm blue-green night.
                float luminance = dot(source.rgb, float3(0.299, 0.587, 0.114));
                float3 nightBase = source.rgb * _NightColor.rgb;
                nightBase = lerp(nightBase, nightBase + luminance * _NightColor.rgb * 0.22, 0.45);
                float3 color = lerp(source.rgb, nightBase, _NightAmount);

                float t = _Time.y * _MotionSpeed;
                float breathe = 0.985 + 0.015 * sin(t * 1.37);
                float sway = 0.0015 * sin(t * 0.71);

                // Existing left window: local glow constrained to its opening.
                float window = softRect(uv, float2(0.184, 0.505), float2(0.337, 0.787), 0.018);
                float mullionV = band(uv.x, 0.260, 0.0045, 0.004);
                float mullionH = band(uv.y, 0.641, 0.0055, 0.004);
                float windowPane = window * (1.0 - saturate(mullionV + mullionH) * _ShadowStrength);

                // A broad, barely-visible shaft reads as atmosphere instead of a graphic overlay.
                float shaftDomain = softRect(uv, float2(0.17, 0.24), float2(0.63, 0.76), 0.11);
                float rayCoord = uv.x + (uv.y - 0.50) * 0.40 + sway;
                float shafts = band(rayCoord, 0.43, 0.075, 0.14) * shaftDomain;
                shafts *= (1.0 - smoothstep(0.48, 0.76, uv.y)) * smoothstep(0.22, 0.38, uv.y);

                // Suppress the shaft over the solid cabinet and chair silhouettes.
                float cabinetOcclusion = softRect(uv, float2(0.378, 0.29), float2(0.493, 0.66), 0.025);
                float chairOcclusion = softRect(uv, float2(0.445, 0.15), float2(0.595, 0.46), 0.035);
                shafts *= 1.0 - saturate(cabinetOcclusion + chairOcclusion) * 0.92;

                // One perspective-stretched pool with only the actual window mullions, not a tiled grid.
                float floorDomain = softEllipse(uv, float2(0.405, 0.205), float2(0.29, 0.20));
                float projectedX = uv.x + uv.y * 0.36 + sway * 0.35;
                float verticalBar = band(projectedX, 0.49, 0.010, 0.020);
                float horizontalBar = band(uv.y + uv.x * 0.055, 0.235, 0.010, 0.018);
                float paneLight = floorDomain * (1.0 - saturate(verticalBar + horizontalBar) * _ShadowStrength);
                paneLight *= smoothstep(0.075, 0.16, uv.y) * (1.0 - chairOcclusion * 0.94);

                float moonMask = saturate(windowPane * 0.48 + shafts * 0.11 + paneLight * 0.30);
                color += _MoonColor.rgb * moonMask * _MoonIntensity * breathe;

                // Restrained bedside practical light, with a tiny independent flicker.
                float2 lampDelta = (uv - float2(0.571, 0.445)) / float2(0.075, 0.105);
                float lampFalloff = pow(saturate(1.0 - length(lampDelta)), 2.8);
                float lampFlicker = 0.995 + 0.005 * sin(t * 4.1 + sin(t * 1.9));
                color += _WarmColor.rgb * lampFalloff * _WarmIntensity * lampFlicker;

                // Gentle highlight roll-off avoids blown-out UI textures on mobile/SDR.
                color = color / (1.0 + max(color - 1.0, 0.0));

                fixed4 result = fixed4(color, source.a);
                #ifdef UNITY_UI_CLIP_RECT
                result.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(result.a - 0.001);
                #endif
                return result;
            }
            ENDCG
        }
    }
}
