// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Bumped Specular shader. Differences from regular Bumped Specular one:
// - no Main Color nor Specular Color
// - specular lighting directions are approximated per vertex
// - writes zero to alpha channel
// - Normalmap uses Tiling/Offset of the Base texture
// - no Deferred Lighting support
// - no Lightmap support
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "VRChat/Mobile/Bumped Mapped Specular"
{
    Properties
    {
        _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
        _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
        _SpecColor ("Specular Color", Color) = (1,1,1,1)
        [NoScaleOffset] _BumpMap ("Normalmap", 2D) = "bump" {}
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 250

        CGPROGRAM
        #pragma target 3.0
        #pragma surface surf BlinnPhong exclude_path:prepass exclude_path:deferred noforwardadd noshadow nodynlightmap nolppv noshadowmask

        UNITY_DECLARE_TEX2D(_MainTex);
        UNITY_DECLARE_TEX2D(_BumpMap);
        half _Shininess;

        struct Input
        {
            float2 uv_MainTex;
            float4 color : COLOR;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 tex = UNITY_SAMPLE_TEX2D(_MainTex, IN.uv_MainTex);
            o.Albedo = tex.rgb * IN.color;
            o.Gloss = tex.a;
            o.Alpha = 1.0f;
            o.Specular = _Shininess;
            o.Normal = UnpackNormal(UNITY_SAMPLE_TEX2D(_BumpMap, IN.uv_MainTex));
        }
        ENDCG
    }

    Fallback "VRChat/Mobile/Diffuse"
}
