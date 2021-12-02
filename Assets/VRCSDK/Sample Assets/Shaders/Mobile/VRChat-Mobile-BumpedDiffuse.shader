// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Bumped shader. Differences from regular Bumped one:
// - no Main Color
// - Normalmap uses Tiling/Offset of the Base texture
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "VRChat/Mobile/Bumped Diffuse"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        [NoScaleOffset] _BumpMap ("Normalmap", 2D) = "bump" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 250

        CGPROGRAM
        #pragma target 3.0
        #pragma surface surf Lambert exclude_path:prepass exclude_path:deferred noforwardadd noshadow nodynlightmap nolppv noshadowmask

        UNITY_DECLARE_TEX2D(_MainTex);
        UNITY_DECLARE_TEX2D(_BumpMap);

        struct Input
        {
            float2 uv_MainTex;
            float4 color : COLOR;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = UNITY_SAMPLE_TEX2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb * IN.color;
            o.Alpha = 1.0f;
            o.Normal = UnpackNormal(UNITY_SAMPLE_TEX2D(_BumpMap, IN.uv_MainTex));
        }
        ENDCG
    }

    Fallback "VRChat/Mobile/Diffuse"
}
