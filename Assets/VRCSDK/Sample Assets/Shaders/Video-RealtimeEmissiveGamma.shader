// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Video/RealtimeEmissiveGamma" {
  Properties {
    _MainTex ("Emissive (RGB)", 2D) = "white" {}
    _Emission ("Emission Scale", Float) = 1
    [Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
  }
  SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 200

      CGPROGRAM
      // Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Standard fullforwardshadows

      // Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0
#pragma shader_feature _EMISSION
#pragma multi_compile APPLY_GAMMA_OFF APPLY_GAMMA

    fixed _Emission;
    sampler2D _MainTex;

    struct Input {
      float2 uv_MainTex;
    };

    // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
    // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
    // #pragma instancing_options assumeuniformscaling
    UNITY_INSTANCING_BUFFER_START(Props)
      // put more per-instance properties here
      UNITY_INSTANCING_BUFFER_END(Props)

      void surf (Input IN, inout SurfaceOutputStandard o) {
        // emissive comes from texture
        fixed4 e = tex2D (_MainTex, IN.uv_MainTex);
        o.Albedo = fixed4(0,0,0,0);
        o.Alpha = e.a;

#if APPLY_GAMMA
        e.rgb = pow(e.rgb,2.2);
#endif
        o.Emission = e * _Emission;
        o.Metallic = 0;
        o.Smoothness = 0;
      }
    ENDCG
  }
  FallBack "Diffuse"
  CustomEditor "RealtimeEmissiveGammaGUI"
}
