#ifndef POI_LIGHTING
#define POI_LIGHTING
float _LightingRampType;
float _LightingIgnoreAmbientColor;
float _UseShadowTexture;
float _LightingEnableAO;
float _LightingDetailShadowsEnabled;
float _LightingOnlyUnityShadows;
float _LightingMode;
float _ForceLightDirection;
float _ShadowStrength;
float _OutlineShadowStrength;
float _ShadowOffset;
float3 _LightDirection;
float _ForceShadowStrength;
float _CastedShadowSmoothing;
float _AttenuationMultiplier;
float _EnableLighting;
float _LightingControlledUseLightColor;
fixed _LightingStandardSmoothness;
fixed _LightingStandardControlsToon;
fixed _LightingMinLightBrightness;
float _LightingUseShadowRamp;
float _LightingMinShadowBrightnessRatio;
fixed _LightingMonochromatic;
fixed _LightingGradientStart;
fixed _LightingGradientEnd;
float3 _LightingShadowColor;
float _AOStrength;
fixed _LightingDetailStrength;
fixed _LightingAdditiveDetailStrength;
fixed _LightingNoIndirectMultiplier;
fixed _LightingNoIndirectThreshold;
float _LightingUncapped;
float _LightingDirectColorMode;
float _LightingIndirectColorMode;
float _LightingAdditiveType;
fixed _LightingAdditiveGradientStart;
fixed _LightingAdditiveGradientEnd;
fixed _LightingAdditivePassthrough;
float _LightingDirectAdjustment;
float _LightingIndirect;
float _LightingEnableHSL;
float _LightingShadowHue;
float _LightingShadowSaturation;
float _LightingShadowLightness;
float _LightingHSLIntensity;
float4 _1st_ShadeColor;
float _Use_BaseAs1st;
float4 _2nd_ShadeColor;
float _Use_1stAs2nd;
float _BaseColor_Step;
float _BaseShade_Feather;
float _ShadeColor_Step;
float _1st2nd_Shades_Feather;
float _Use_1stShadeMapAlpha_As_ShadowMask;
float _1stShadeMapMask_Inverse;
float _Tweak_1stShadingGradeMapLevel;
float _Use_2ndShadeMapAlpha_As_ShadowMask;
float _2ndShadeMapMask_Inverse;
float _Tweak_2ndShadingGradeMapLevel;
float _SkinScatteringProperties;
float _SssWeight;
float _SssMaskCutoff ;
float _SssBias;
float _SssScale;
float _SssBumpBlur;
float4 _SssTransmissionAbsorption;
float4 _SssColorBleedAoWeights;
half4 shadowStrength;
sampler2D _SkinLUT;
UNITY_DECLARE_TEX2D(_ToonRamp);
POI_TEXTURE_NOSAMPLER(_1st_ShadeMap);
POI_TEXTURE_NOSAMPLER(_2nd_ShadeMap);
POI_TEXTURE_NOSAMPLER(_LightingDetailShadows);
POI_TEXTURE_NOSAMPLER(_LightingAOTex);
POI_TEXTURE_NOSAMPLER(_LightingShadowMask);
float3 directLighting;
float3 indirectLighting;
float _LightingWrappedWrap;
float _LightingWrappedNormalization;
float RTWrapFunc(in float dt, in float w, in float norm)
{
    float cw = saturate(w);
    float o = (dt + cw) / ((1.0 + cw) * (1.0 + cw * norm));
    float flt = 1.0 - 0.85 * norm;
    if (w > 1.0)
    {
        o = lerp(o, flt, w - 1.0);
    }
    return o;
}
float3 GreenWrapSH(float fA) // Greens unoptimized and non-normalized
{
    float fAs = saturate(fA);
    float4 t = float4(fA + 1, fAs - 1, fA - 2, fAs + 1); // DJL edit: allow wrapping to L0-only at w=2
    return float3(t.x, -t.z * t.x / 3, 0.25 * t.y * t.y * t.w);
}
float3 GreenWrapSHOpt(float fW) // optimised and normalized https://blog.selfshadow.com/2012/01/07/righting-wrap-part-2/
{
    const float4 t0 = float4(0.0, 1.0 / 4.0, -1.0 / 3.0, -1.0 / 2.0);
    const float4 t1 = float4(1.0, 2.0 / 3.0, 1.0 / 4.0, 0.0);
    float3 fWs = float3(fW, fW, saturate(fW)); // DJL edit: allow wrapping to L0-only at w=2
    float3 r;
    r.xyz = t0.xxy * fWs + t0.xzw;
    r.xyz = r.xyz * fWs + t1.xyz;
    return r;
}
float3 ShadeSH9_wrapped(float3 normal, float wrap)
{
    float3 x0, x1, x2;
    float3 conv = lerp(GreenWrapSH(wrap), GreenWrapSHOpt(wrap), float(0)); // Should try optimizing this...
    conv *= float3(1, 1.5, 4); // Undo pre-applied cosine convolution by using the inverse
    x0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
    float3 L2_0 = float3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / - 3.0;
    x0 -= L2_0;
    x1.r = dot(unity_SHAr.xyz, normal);
    x1.g = dot(unity_SHAg.xyz, normal);
    x1.b = dot(unity_SHAb.xyz, normal);
    float4 vB = normal.xyzz * normal.yzzx;
    x2.r = dot(unity_SHBr, vB);
    x2.g = dot(unity_SHBg, vB);
    x2.b = dot(unity_SHBb, vB);
    float vC = normal.x * normal.x - normal.y * normal.y;
    x2 += unity_SHC.rgb * vC;
    x2 += L2_0;
    return x0 * conv.x + x1 * conv.y + x2 * conv.z;
}
float shEvaluateDiffuseL1Geomerics_local(float L0, float3 L1, float3 n)
{
    float R0 = max(0, L0);
    float3 R1 = 0.5f * L1;
    float lenR1 = length(R1);
    float q = dot(normalize(R1), n) * 0.5 + 0.5;
    q = saturate(q); // Thanks to ScruffyRuffles for the bug identity.
    float p = 1.0f + 2.0f * lenR1 / R0;
    float a = (1.0f - lenR1 / R0) / (1.0f + lenR1 / R0);
    return R0 * (a + (1.0f - a) * (p + 1.0f) * pow(q, p));
}
half3 BetterSH9(half4 normal)
{
    float3 indirect;
    float3 L0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w) + float3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / 3.0;
    indirect.r = shEvaluateDiffuseL1Geomerics_local(L0.r, unity_SHAr.xyz, normal.xyz);
    indirect.g = shEvaluateDiffuseL1Geomerics_local(L0.g, unity_SHAg.xyz, normal.xyz);
    indirect.b = shEvaluateDiffuseL1Geomerics_local(L0.b, unity_SHAb.xyz, normal.xyz);
    indirect = max(0, indirect);
    indirect += SHEvalLinearL2(normal);
    return indirect;
}
float3 BetterSH9(float3 normal)
{
    return BetterSH9(float4(normal, 1));
}
UnityLight CreateLight(float3 normal, fixed detailShadowMap)
{
    UnityLight light;
    light.dir = poiLight.direction;
    light.color = saturate(_LightColor0.rgb * lerp(1, poiLight.attenuation, float(0)) * detailShadowMap);
    light.ndotl = DotClamped(normal, poiLight.direction);
    return light;
}
float FadeShadows(float attenuation)
{
    #if HANDLE_SHADOWS_BLENDING_IN_GI || ADDITIONAL_MASKED_DIRECTIONAL_SHADOWS
        #if ADDITIONAL_MASKED_DIRECTIONAL_SHADOWS
            attenuation = lerp(1, poiLight.attenuation, float(0));
        #endif
        float viewZ = dot(_WorldSpaceCameraPos - poiMesh.worldPos, UNITY_MATRIX_V[2].xyz);
        float shadowFadeDistance = UnityComputeShadowFadeDistance(poiMesh.worldPos, viewZ);
        float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
        float bakedAttenuation = UnitySampleBakedOcclusion(poiMesh.lightmapUV.xy, poiMesh.worldPos);
        attenuation = UnityMixRealtimeAndBakedShadows(
            attenuation, bakedAttenuation, shadowFade
        );
    #endif
    return attenuation;
}
void ApplySubtractiveLighting(inout UnityIndirect indirectLight)
{
    #if SUBTRACTIVE_LIGHTING
        poiLight.attenuation = FadeShadows(lerp(1, poiLight.attenuation, float(0)));
        float ndotl = saturate(dot(i.normal, _WorldSpaceLightPos0.xyz));
        float3 shadowedLightEstimate = ndotl * (1 - poiLight.attenuation) * _LightColor0.rgb;
        float3 subtractedLight = indirectLight.diffuse - shadowedLightEstimate;
        subtractedLight = max(subtractedLight, unity_ShadowColor.rgb);
        subtractedLight = lerp(subtractedLight, indirectLight.diffuse, _LightShadowData.x);
        indirectLight.diffuse = min(subtractedLight, indirectLight.diffuse);
    #endif
}
float3 weightedBlend(float3 layer1, float3 layer2, float2 weights)
{
    return(weights.x * layer1 + weights.y * layer2) / (weights.x + weights.y);
}
UnityIndirect CreateIndirectLight(float3 normal)
{
    UnityIndirect indirectLight;
    indirectLight.diffuse = 0;
    indirectLight.specular = 0;
    #if defined(FORWARD_BASE_PASS)
        #if defined(LIGHTMAP_ON)
            indirectLight.diffuse = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, poiMesh.lightmapUV.xy));
            #if defined(DIRLIGHTMAP_COMBINED)
                float4 lightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(
                    unity_LightmapInd, unity_Lightmap, poiMesh.lightmapUV.xy
                );
                indirectLight.diffuse = DecodeDirectionalLightmap(
                    indirectLight.diffuse, lightmapDirection, normal
                );
            #endif
            ApplySubtractiveLighting(indirectLight);
        #endif
        #if defined(DYNAMICLIGHTMAP_ON)
            float3 dynamicLightDiffuse = DecodeRealtimeLightmap(
                UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, poiMesh.lightmapUV.zw)
            );
            #if defined(DIRLIGHTMAP_COMBINED)
                float4 dynamicLightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(
                    unity_DynamicDirectionality, unity_DynamicLightmap,
                    poiMesh.lightmapUV.zw
                );
                indirectLight.diffuse += DecodeDirectionalLightmap(
                    dynamicLightDiffuse, dynamicLightmapDirection, normal
                );
            #else
                indirectLight.diffuse += dynamicLightDiffuse;
            #endif
        #endif
        #if !defined(LIGHTMAP_ON) && !defined(DYNAMICLIGHTMAP_ON)
            #if UNITY_LIGHT_PROBE_PROXY_VOLUME
                if (unity_ProbeVolumeParams.x == 1)
                {
                    indirectLight.diffuse = SHEvalLinearL0L1_SampleProbeVolume(
                        float4(normal, 1), poiMesh.worldPos
                    );
                    indirectLight.diffuse = max(0, indirectLight.diffuse);
                    #if defined(UNITY_COLORSPACE_GAMMA)
                        indirectLight.diffuse = LinearToGammaSpace(indirectLight.diffuse);
                    #endif
                }
                else
                {
                    indirectLight.diffuse += max(0, ShadeSH9(float4(normal, 1)));
                }
            #else
                indirectLight.diffuse += max(0, ShadeSH9(float4(normal, 1)));
            #endif
        #endif
        float3 reflectionDir = reflect(-poiCam.viewDir, normal);
        Unity_GlossyEnvironmentData envData;
        envData.roughness = 1 - float(0);
        envData.reflUVW = BoxProjection(
            reflectionDir, poiMesh.worldPos.xyz,
            unity_SpecCube0_ProbePosition,
            unity_SpecCube0_BoxMin.xyz, unity_SpecCube0_BoxMax.xyz
        );
        float3 probe0 = Unity_GlossyEnvironment(
            UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData
        );
        envData.reflUVW = BoxProjection(
            reflectionDir, poiMesh.worldPos.xyz,
            unity_SpecCube1_ProbePosition,
            unity_SpecCube1_BoxMin.xyz, unity_SpecCube1_BoxMax.xyz
        );
        #if UNITY_SPECCUBE_BLENDING
            float interpolator = unity_SpecCube0_BoxMin.w;
            
            if (interpolator < 0.99999)
            {
                float3 probe1 = Unity_GlossyEnvironment(
                    UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0),
                    unity_SpecCube0_HDR, envData
                );
                indirectLight.specular = lerp(probe1, probe0, interpolator);
            }
            else
            {
                indirectLight.specular = probe0;
            }
        #else
            indirectLight.specular = probe0;
        #endif
        float occlusion = 1;
        
        if (float(0))
        {
            occlusion = lerp(1, POI2D_SAMPLER_PAN(_LightingAOTex, _MainTex, poiMesh.uv[float(0)], float4(0,0,0,0)).r, float(1));
        }
        indirectLight.diffuse *= occlusion;
        indirectLight.diffuse = max(indirectLight.diffuse, float(0));
        indirectLight.specular *= occlusion;
    #endif
    return indirectLight;
}
half PoiDiffuse(half NdotV, half NdotL, half LdotH)
{
    half fd90 = 0.5 + 2 * LdotH * LdotH * SmoothnessToPerceptualRoughness(.5);
    half lightScatter = (1 + (fd90 - 1) * Pow5(1 - NdotL));
    half viewScatter = (1 + (fd90 - 1) * Pow5(1 - NdotV));
    return lightScatter * viewScatter;
}
float3 ShadeSH9Indirect()
{
    return ShadeSH9(half4(0.0, -1.0, 0.0, 1.0));
}
float3 ShadeSH9Direct()
{
    return ShadeSH9(half4(0.0, 1.0, 0.0, 1.0));
}
float3 ShadeSH9Normal(float3 normalDirection)
{
    return ShadeSH9(half4(normalDirection, 1.0));
}
half3 GetSHLength()
{
    half3 x, x1;
    x.r = length(unity_SHAr);
    x.g = length(unity_SHAg);
    x.b = length(unity_SHAb);
    x1.r = length(unity_SHBr);
    x1.g = length(unity_SHBg);
    x1.b = length(unity_SHBb);
    return x + x1;
}
half3 GetSHDirectionL1()
{
    float3 grayscale = float3(.33333, .33333, .33333);
    half3 r = Unity_SafeNormalize(half3(unity_SHAr.x, unity_SHAr.y, unity_SHAr.z));
    half3 g = Unity_SafeNormalize(half3(unity_SHAg.x, unity_SHAg.y, unity_SHAg.z));
    half3 b = Unity_SafeNormalize(half3(unity_SHAb.x, unity_SHAb.y, unity_SHAb.z));
    return Unity_SafeNormalize(grayscale.r * r + grayscale.g * g + grayscale.b * b);
}
float3 GetSHDirectionL1_()
{
    return Unity_SafeNormalize((unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz));
}
half3 GetSHMaxL1()
{
    float3 maxDirection = GetSHDirectionL1();
    return ShadeSH9_wrapped(maxDirection, 0);
}
float3 calculateRealisticLighting(float4 colorToLight, fixed detailShadowMap)
{
    return UNITY_BRDF_PBS(1, 0, 0, float(0), poiMesh.normals[1], poiCam.viewDir, CreateLight(poiMesh.normals[1], detailShadowMap), CreateIndirectLight(poiMesh.normals[1])).xyz;
}
void calculateBasePassLightMaps()
{
    #if defined(FORWARD_BASE_PASS) || defined(POI_META_PASS)
        float AOMap = 1;
        float AOStrength = 0;
        float3 lightColor = poiLight.color;
        bool lightExists = false;
        if (any(_LightColor0.rgb >= 0.002))
        {
            lightExists = true;
        }
        #ifndef OUTLINE
            
            if (float(0))
            {
                AOMap = POI2D_SAMPLER_PAN(_LightingAOTex, _MainTex, poiMesh.uv[float(0)], float4(0,0,0,0)).r;
                AOStrength = float(1);
                poiLight.occlusion = lerp(1, AOMap, AOStrength);
            }
            #ifdef FORWARD_BASE_PASS
                if (lightExists)
                {
                    lightColor = _LightColor0.rgb + BetterSH9(float4(0, 0, 0, 1));
                }
                else
                {
                    lightColor = BetterSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb));
                }
            #endif
        #endif
        float3 grayscale_vector = float3(.33333, .33333, .33333);
        float3 ShadeSH9Plus = GetSHLength();
        float3 ShadeSH9Minus = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w) + float3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / 3.0;
        shadowStrength = 1;
        #ifndef OUTLINE
            shadowStrength = POI2D_SAMPLER_PAN(_LightingShadowMask, _MainTex, poiMesh.uv[float(0)], float4(0,0,0,0)) * float(1);
        #else
            shadowStrength = float(1);
        #endif
        float bw_lightColor = dot(lightColor, grayscale_vector);
        float bw_directLighting = (((poiLight.nDotL * 0.5 + 0.5) * bw_lightColor * lerp(1, poiLight.attenuation, float(0))) + dot(ShadeSH9Normal(poiMesh.normals[1]), grayscale_vector));
        float bw_bottomIndirectLighting = dot(ShadeSH9Minus, grayscale_vector);
        float bw_topIndirectLighting = dot(ShadeSH9Plus, grayscale_vector);
        float lightDifference = ((bw_topIndirectLighting + bw_lightColor) - bw_bottomIndirectLighting);
        fixed detailShadow = 1;
        
        if (float(0))
        {
            detailShadow = lerp(1, POI2D_SAMPLER_PAN(_LightingDetailShadows, _MainTex, poiMesh.uv[float(0)], float4(0,0,0,0)), float(1)).r;
        }
        
        if (float(0))
        {
            poiLight.lightMap = poiLight.attenuation;
        }
        else
        {
            poiLight.lightMap = smoothstep(0, lightDifference, bw_directLighting - bw_bottomIndirectLighting);
        }
        poiLight.lightMap *= detailShadow;
        indirectLighting = 0;
        directLighting = 0;
        
        if (float(0) == 1)
        {
            indirectLighting = BetterSH9(float4(poiMesh.normals[1], 1));
        }
        else
        {
            indirectLighting = ShadeSH9Minus;
        }
        poiLight.directLighting = lightColor;
        poiLight.indirectLighting = indirectLighting;
        
        if (float(0) == 0)
        {
            float3 magic = max(BetterSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)), 0);
            float3 normalLight = _LightColor0.rgb + BetterSH9(float4(0, 0, 0, 1));
            float magiLumi = calculateluminance(magic);
            float normaLumi = calculateluminance(normalLight);
            float maginormalumi = magiLumi + normaLumi;
            float magiratio = magiLumi / maginormalumi;
            float normaRatio = normaLumi / maginormalumi;
            float target = calculateluminance(magic * magiratio + normalLight * normaRatio);
            float3 properLightColor = magic * poiLight.occlusion + normalLight;
            float properLuminance = calculateluminance(magic + normalLight);
            directLighting = properLightColor * max(0.0001, (target / properLuminance));
        }
        else
        {
            if (lightExists)
            {
                directLighting = _LightColor0.rgb + BetterSH9(float4(0, 0, 0, 1)) * poiLight.occlusion;
            }
            else
            {
                directLighting = max(BetterSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)), 0);
            }
        }
        
        if (!float(0))
        {
            float directluminance = calculateluminance(directLighting);
            float indirectluminance = calculateluminance(indirectLighting);
            directLighting = min(directLighting, directLighting / max(0.0001, (directluminance / 1)));
            indirectLighting = min(indirectLighting, indirectLighting / max(0.0001, (indirectluminance / 1)));
        }
        directLighting = lerp(directLighting, dot(directLighting, float3(0.299, 0.587, 0.114)), float(0));
        indirectLighting = lerp(indirectLighting, dot(indirectLighting, float3(0.299, 0.587, 0.114)), float(0));
        if (max(max(indirectLighting.x, indirectLighting.y), indirectLighting.z) <= _LightingNoIndirectThreshold && max(max(directLighting.x, directLighting.y), directLighting.z) >= 0)
        {
            indirectLighting = directLighting * _LightingNoIndirectMultiplier;
        }
        
        if (float(0))
        {
            float directluminance = clamp(directLighting.r * 0.299 + directLighting.g * 0.587 + directLighting.b * 0.114, 0, 1);
            if (directluminance > 0)
            {
                indirectLighting = max(0.001, indirectLighting);
            }
            float indirectluminance = clamp(indirectLighting.r * 0.299 + indirectLighting.g * 0.587 + indirectLighting.b * 0.114, 0, 1);
            float targetluminance = directluminance * float(0);
            if (indirectluminance < targetluminance)
            {
                indirectLighting = indirectLighting / max(0.0001, indirectluminance / targetluminance);
            }
        }
        poiLight.rampedLightMap = 1 - smoothstep(0, .5, 1 - poiLight.lightMap);
        poiLight.finalLighting = directLighting;
        indirectLighting = max(indirectLighting,0);
        directLighting = max(directLighting,0);
        switch(float(0))
        {
            case 0: // Ramp Texture
            {
                poiLight.rampedLightMap = lerp(1, UNITY_SAMPLE_TEX2D(_ToonRamp, poiLight.lightMap + float(0)).rgb, shadowStrength.r);
                
                if (float(0))
                {
                    poiLight.finalLighting = lerp(poiLight.rampedLightMap * directLighting * poiLight.occlusion, directLighting, poiLight.rampedLightMap);
                }
                else
                {
                    poiLight.finalLighting = lerp(indirectLighting * poiLight.occlusion, directLighting, poiLight.rampedLightMap);
                }
            }
            break;
            case 1: // Math Gradient
            {
                poiLight.rampedLightMap = saturate(1 - smoothstep(float(0) - .000001, float(0.5), 1 - poiLight.lightMap));
                float3 shadowColor = float4(1,1,1,1);
                
                if (_UseShadowTexture)
                {
                    shadowColor = 1;
                }
                
                if (float(0))
                {
                    poiLight.finalLighting = lerp((directLighting * shadowColor * poiLight.occlusion), (directLighting), saturate(poiLight.rampedLightMap + 1 - float(1)));
                }
                else
                {
                    poiLight.finalLighting = lerp((indirectLighting * shadowColor * poiLight.occlusion), (directLighting), saturate(poiLight.rampedLightMap + 1 - float(1)));
                }
            }
            break;
            case 2:
            {
                poiLight.rampedLightMap = saturate(1 - smoothstep(0, .5, 1 - poiLight.lightMap));
                poiLight.finalLighting = directLighting;
            }
            break;
        }
        if (float(4) == 2) // Wrapped
        {
            float wrap = float(0);
            float3 directcolor = (_LightColor0.rgb) * saturate(RTWrapFunc(poiLight.nDotL, wrap, float(0)));
            float directatten = lerp(1, poiLight.attenuation, float(0));
            uint normalsindex = float(0) > 0 ? 1: 0;
                    float3 envlight = ShadeSH9_wrapped(poiMesh.normals[normalsindex], wrap);
                    envlight *= poiLight.occlusion;
                    poiLight.directLighting = directcolor * detailShadow * directatten;
                    poiLight.indirectLighting = envlight;
                    float3 ShadeSH9Plus_2 = GetSHMaxL1();
                    float bw_topDirectLighting_2 = dot(_LightColor0.rgb, grayscale_vector);
                    float bw_directLighting = dot(poiLight.directLighting, grayscale_vector);
                    float bw_indirectLighting = dot(poiLight.indirectLighting, grayscale_vector);
                    float bw_topIndirectLighting = dot(ShadeSH9Plus_2, grayscale_vector);
                    poiLight.lightMap = smoothstep(0, bw_topIndirectLighting + bw_topDirectLighting_2, bw_indirectLighting + bw_directLighting);
                    poiLight.rampedLightMap = 1;
                    
                    if (float(0) == 0) // Ramp Texture
                    {
                        poiLight.rampedLightMap = lerp(1, UNITY_SAMPLE_TEX2D(_ToonRamp, poiLight.lightMap + float(0)).rgb, shadowStrength.r);
                    }
                    else if (float(0) == 1) // Math Gradient
                    {
                        poiLight.rampedLightMap = lerp(float4(1,1,1,1) * lerp(poiLight.indirectLighting, 1, float(0)), float3(1, 1, 1), saturate(1 - smoothstep(float(0) - .000001, float(0.5), 1 - poiLight.lightMap)));
                        poiLight.rampedLightMap = lerp(float3(1, 1, 1), poiLight.rampedLightMap, shadowStrength.r);
                    }
                    poiLight.finalLighting = (poiLight.indirectLighting + poiLight.directLighting) * saturate(poiLight.rampedLightMap + 1 - float(1));
                }
                if (!float(0))
                {
                    poiLight.finalLighting = saturate(poiLight.finalLighting);
                }
            #endif
        }
        float3 calculateNonImportantLighting(float attenuation, float attenuationDotNL, float3 albedo, float3 lightColor, half dotNL, half correctedDotNL)
        {
            fixed detailShadow = 1;
            
            if (float(0))
            {
                detailShadow = lerp(1, POI2D_SAMPLER_PAN(_LightingDetailShadows, _MainTex, poiMesh.uv[float(0)], float4(0,0,0,0)), float(1)).r;
            }
            
            if (float(1) == 0)
            {
                return lightColor * attenuationDotNL * detailShadow; // Realistic
            }
            else if (float(1) == 1) // Toon
            {
                return lerp(lightColor * attenuation, lightColor * float(0.5) * attenuation, smoothstep(float(0), float(0.5), dotNL)) * detailShadow;
            }
            else //if(float(1) == 2) // Wrapped
            {
                float uv = saturate(RTWrapFunc(-dotNL, float(0), float(0))) * detailShadow;
                poiLight.rampedLightMap = 1;
                if (float(0) == 1) // Math Gradient
                poiLight.rampedLightMap = lerp(float4(1,1,1,1), float3(1, 1, 1), saturate(1 - smoothstep(float(0) - .000001, float(0.5), 1 - uv)));
                return lightColor * poiLight.rampedLightMap * saturate(attenuation * uv);
            }
        }
        void applyShadeMaps(inout float4 albedo)
        {
            
            if (float(0) == 2)
            {
                float3 baseColor = albedo.rgb;
                float MainColorFeatherStep = float(0.5) - float(0.0001);
                float firstColorFeatherStep = float(0) - float(0.0001);
                #if defined(PROP_1ST_SHADEMAP) || !defined(OPTIMIZER_ENABLED)
                    float4 firstShadeMap = POI2D_SAMPLER_PAN(_1st_ShadeMap, _MainTex, poiMesh.uv[float(0)], float4(0,0,0,0));
                #else
                    float4 firstShadeMap = float4(1, 1, 1, 1);
                #endif
                firstShadeMap = lerp(firstShadeMap, albedo, float(0));
                #if defined(PROP_2ND_SHADEMAP) || !defined(OPTIMIZER_ENABLED)
                    float4 secondShadeMap = POI2D_SAMPLER_PAN(_2nd_ShadeMap, _MainTex, poiMesh.uv[float(0)], float4(0,0,0,0));
                #else
                    float4 secondShadeMap = float4(1, 1, 1, 1);
                #endif
                secondShadeMap = lerp(secondShadeMap, firstShadeMap, float(0));
                firstShadeMap.rgb *= float4(1,1,1,1).rgb; //* lighColor
                secondShadeMap.rgb *= float4(1,1,1,1).rgb; //* LightColor;
                float shadowMask = 1;
                shadowMask *= float(0) ?(float(0) ?(1.0 - firstShadeMap.a): firstShadeMap.a): 1;
                shadowMask *= float(0) ?(float(0) ?(1.0 - secondShadeMap.a): secondShadeMap.a): 1;
                float mainShadowMask = saturate(1 - ((poiLight.lightMap) - MainColorFeatherStep) / (float(0.5) - MainColorFeatherStep) * (shadowMask));
                float firstSecondShadowMask = saturate(1 - ((poiLight.lightMap) - firstColorFeatherStep) / (float(0) - firstColorFeatherStep) * (shadowMask));
                #if defined(PROP_LIGHTINGSHADOWMASK) || !defined(OPTIMIZER_ENABLED)
                    float removeShadow = POI2D_SAMPLER_PAN(_LightingShadowMask, _MainTex, poiMesh.uv[float(0)], float4(0,0,0,0)).r;
                #else
                    float removeShadow = 1;
                #endif
                mainShadowMask *= removeShadow;
                firstSecondShadowMask *= removeShadow;
                albedo.rgb = lerp(albedo.rgb, lerp(firstShadeMap.rgb, secondShadeMap.rgb, firstSecondShadowMask), mainShadowMask);
            }
        }
        float3 calculateFinalLighting(inout float3 albedo, float4 finalColor)
        {
            float3 finalLighting = 1;
            #ifdef FORWARD_ADD_PASS
                fixed detailShadow = 1;
                
                if (float(0))
                {
                    detailShadow = lerp(1, POI2D_SAMPLER_PAN(_LightingDetailShadows, _MainTex, poiMesh.uv[float(0)], float4(0,0,0,0)), float(1)).r;
                }
                
                if (float(1) == 0) // Realistic
                {
                    finalLighting = poiLight.color * poiLight.attenuation * max(0, poiLight.nDotL) * detailShadow * poiLight.additiveShadow;
                }
                else if (float(1) == 1) // Toon
                {
                    #if defined(POINT) || defined(SPOT)
                        finalLighting = lerp(poiLight.color * max(poiLight.additiveShadow, float(0.5)), poiLight.color * float(0.5), smoothstep(float(0), float(0.5), 1 - (.5 * poiLight.nDotL + .5))) * poiLight.attenuation * detailShadow;
                    #else
                        finalLighting = lerp(poiLight.color * max(poiLight.attenuation, float(0.5)), poiLight.color * float(0.5), smoothstep(float(0), float(0.5), 1 - (.5 * poiLight.nDotL + .5))) * detailShadow;
                    #endif
                }
                else //if(float(1) == 2) // Wrapped
                {
                    float uv = saturate(RTWrapFunc(poiLight.nDotL, float(0), float(0))) * detailShadow;
                    poiLight.rampedLightMap = 1;
                    
                    if (float(0) == 1) // Math Gradient
                    poiLight.rampedLightMap = lerp(float4(1,1,1,1), float3(1, 1, 1), saturate(1 - smoothstep(float(0) - .000001, float(0.5), 1 - uv)));
                    float shadowatten = max(poiLight.additiveShadow, float(0.5));
                    return poiLight.color * poiLight.rampedLightMap * saturate(poiLight.attenuation * uv * shadowatten);
                }
            #endif
            #if defined(FORWARD_BASE_PASS) || defined(POI_META_PASS)
                #ifdef VERTEXLIGHT_ON
                    poiLight.vFinalLighting = 0;
                    for (int index = 0; index < 4; index++)
                    {
                        poiLight.vFinalLighting += calculateNonImportantLighting(poiLight.vAttenuation[index], poiLight.vAttenuationDotNL[index], albedo, poiLight.vColor[index], poiLight.vDotNL[index], poiLight.vCorrectedDotNL[index]);
                    }
                #endif
                switch(float(4))
                {
                    case 0: // Toon Lighting
                    case 2: // or wrapped
                    {
                        
                        if (float(0))
                        {
                            float3 HSLMod = float3(float(0.5) * 2 - 1, float(0.5) * 2 - 1, float(0.5) * 2 - 1) * (1 - poiLight.rampedLightMap);
                            albedo = lerp(albedo.rgb, ModifyViaHSL(albedo.rgb, HSLMod), float(1));
                        }
                        
                        if (float(0) > 0)
                        {
                            poiLight.finalLighting = max(0.001, poiLight.finalLighting);
                            float finalluminance = calculateluminance(poiLight.finalLighting);
                            finalLighting = max(poiLight.finalLighting, poiLight.finalLighting / max(0.0001, (finalluminance / float(0))));
                            poiLight.finalLighting = finalLighting;
                        }
                        else
                        {
                            finalLighting = poiLight.finalLighting;
                        }
                    }
                    break;
                    case 1: // realistic
                    {
                        fixed detailShadow = 1;
                        
                        if (float(0))
                        {
                            detailShadow = lerp(1, POI2D_SAMPLER_PAN(_LightingDetailShadows, _MainTex, poiMesh.uv[float(0)], float4(0,0,0,0)), float(1)).r;
                        }
                        float3 realisticLighting = calculateRealisticLighting(finalColor, detailShadow).rgb;
                        finalLighting = lerp(realisticLighting, dot(realisticLighting, float3(0.299, 0.587, 0.114)), float(0));
                    }
                    break;
                    case 3: // Skin
                    {
                        float subsurfaceShadowWeight = 0.0h;
                        float3 ambientNormalWorld = poiMesh.normals[1];//aTangentToWorld(s, s.blurredNormalTangent);
                        float subsurface = 1;
                        float skinScatteringMask = _SssWeight * saturate(1.0h / _SssMaskCutoff * subsurface);
                        float skinScattering = saturate(subsurface * float(1) * 2 + _SssBias);
                        half3 absorption = exp((1.0h - subsurface) * float4(-8,-40,-64,0).rgb);
                        absorption *= saturate(finalColor.rgb * unity_ColorSpaceDouble.rgb);
                        ambientNormalWorld = normalize(lerp(poiMesh.normals[1], ambientNormalWorld, float(0.7)));
                        float ndlBlur = dot(poiMesh.normals[1], poiLight.direction) * 0.5h + 0.5h;
                        float lumi = dot(poiLight.color, half3(0.2126h, 0.7152h, 0.0722h));
                        float4 sssLookupUv = float4(ndlBlur, skinScattering * lumi, 0.0f, 0.0f);
                        half3 sss = poiLight.lightMap * poiLight.attenuation * tex2Dlod(_SkinLUT, sssLookupUv).rgb;
                        finalLighting = min(lerp(indirectLighting * float4(1,1,1,1), float4(1,1,1,1), float(0)) + (sss * directLighting), directLighting);
                    }
                    break;
                    case 4:
                    {
                        finalLighting = directLighting;
                    }
                    break;
                }
            #endif
            return finalLighting;
        }
        void applyLighting(inout float4 finalColor, float3 finalLighting)
        {
            #ifdef VERTEXLIGHT_ON
                finalColor.rgb *= finalLighting + poiLight.vFinalLighting;
            #else
                finalColor.rgb *= finalLighting;
            #endif
        }
    #endif
