#ifndef SHADOW_FRAG
#define SHADOW_FRAG
float2 _MainDistanceFade;
float _ForceOpaque;
float _MainShadowClipMod;
float2 _ClippingMaskPan;
float _ClippingMaskUV;
sampler3D _DitherMaskLOD;
float2 _MainTexPan;
float _MainTextureUV;
float _Inverse_Clipping;
float _MainDistanceFadeMin;
float _MainDistanceFadeMax;
half _MainMinAlpha;
half _MainMaxAlpha;
#if defined(PROP_MAINFADETEXTURE) || !defined(OPTIMIZER_ENABLED)
    POI_TEXTURE_NOSAMPLER(_MainFadeTexture);
#endif
float distanceFade()
{
    #if defined(PROP_MAINFADETEXTURE) || !defined(OPTIMIZER_ENABLED)
        half fadeMap = POI2D_SAMPLER_PAN(_MainFadeTexture, _MainTex, poiMesh.uv[float(0)], float4(0,0,0,0)).r;
    #else
        half fadeMap = 1;
    #endif
    return lerp(float(0), float(1), smoothstep(float(0), float(0), distance(poiMesh.worldPos, poiCam.worldPos)));
}
half4 fragShadowCaster(
    #if !defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || defined(UNITY_STANDARD_USE_SHADOW_UVS)
        V2FShadow i, uint facing: SV_IsFrontFace
        #endif
    ): SV_Target
    {
        poiMesh.uv[0] = i.uv;
        poiMesh.uv[1] = i.uv1;
        poiMesh.uv[2] = i.uv2;
        poiMesh.uv[3] = i.uv3;
        float4 mainTexture = UNITY_SAMPLE_TEX2D(_MainTex, TRANSFORM_TEX(poiMesh.uv[float(0)], _MainTex) + _Time.x * float4(0,0,0,0));
        float clipValue = clamp(float(0.5) + float(0), - .001, 1.001);
        poiMesh.vertexColor = saturate(i.vertexColor);
        poiMesh.worldPos = i.worldPos;
        poiMesh.localPos = i.localPos;
        poiCam.worldPos = _WorldSpaceCameraPos;
        #ifdef POI_MIRROR
            applyMirrorRenderFrag();
        #endif
        #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
            half4 alpha = mainTexture;
            #if defined(PROP_MIRRORTEXTURE) || !defined(OPTIMIZER_ENABLED)
                
                if (float(0))
                {
                    if (IsInMirror())
                    {
                        alpha.a = UNITY_SAMPLE_TEX2D_SAMPLER(_MirrorTexture, _MainTex, TRANSFORM_TEX(i.uv, _MirrorTexture)).a;
                    }
                }
            #endif
            alpha.a *= distanceFade();
            half alphaMask = POI2D_PAN(_ClippingMask, poiMesh.uv[float(0)], float4(0,0,0,0));
            
            if (float(0))
            {
                alphaMask = 1 - alphaMask;
            }
            alpha.a *= alphaMask;
            alpha.a *= float4(1,1,1,1).a + .0001;
            alpha.a += float(0);
            alpha.a = saturate(alpha.a);
            
            if (float(0) == 0)
            {
                alpha.a = 1;
            }
            
            if (float(0) == 1)
            {
                applyShadowDithering(alpha.a, calcScreenUVs(i.grabPos).xy);
            }
            #ifdef POI_DISSOLVE
                float3 fakeEmission = 1;
                calculateDissolve(alpha, fakeEmission);
            #endif
            
            if (float(0) == 1)
            {
                clip(alpha.a - 0.001);
            }
            
            if (float(0) == 1)
            {
                clip(alpha.a - clipValue);
            }
            
            if (float(0) > 1)
            {
                float dither = tex3D(_DitherMaskLOD, float3(i.pos.xy * .25, alpha.a * 0.9375)).a;
                clip(dither - 0.01);
            }
        #endif
        SHADOW_CASTER_FRAGMENT(i)
    }
#endif
