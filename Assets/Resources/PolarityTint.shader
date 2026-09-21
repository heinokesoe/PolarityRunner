Shader "Polarity Runner/Polarity Tint"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _PolarityColor ("Polarity Color", Color) = (1, 0, 0, 1)
        _TintNeutral ("Tint Neutral Colors", Range(0, 1)) = 0
        [MaterialToggle] PixelSnap ("Pixel Snap", Float) = 0
        [HideInInspector] _RendererColor ("Renderer Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _Flip ("Flip", Vector) = (1, 1, 1, 1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment PolarityFragment
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            fixed4 _PolarityColor;
            fixed _TintNeutral;

            fixed4 PolarityFragment(v2f input) : SV_Target
            {
                fixed4 pixel = SampleSpriteTexture(input.texcoord) * input.color;
                fixed brightest = max(pixel.r, max(pixel.g, pixel.b));
                fixed darkest = min(pixel.r, min(pixel.g, pixel.b));
                fixed saturation = brightest - darkest;
                fixed tintAmount = max(smoothstep(0.05, 0.25, saturation), _TintNeutral);
                pixel.rgb = lerp(pixel.rgb, _PolarityColor.rgb * brightest, tintAmount);
                pixel.rgb *= pixel.a;
                return pixel;
            }
            ENDCG
        }
    }
}
