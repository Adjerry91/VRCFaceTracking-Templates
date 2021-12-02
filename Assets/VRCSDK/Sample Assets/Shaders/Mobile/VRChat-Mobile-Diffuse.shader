// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "VRChat/Mobile/Diffuse"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 150
    
        CGPROGRAM
        #pragma target 3.0
        #pragma surface surf Lambert exclude_path:prepass exclude_path:deferred noforwardadd noshadow nodynlightmap nolppv noshadowmask

        UNITY_DECLARE_TEX2D(_MainTex);

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
        }
        ENDCG
    }

    FallBack "Diffuse"
}
