#ifndef POI_SPECULAR
#define POI_SPECULAR
float _SpecWhatTangent;
float _SpecularType;
float _SmoothnessFrom;
POI_TEXTURE_NOSAMPLER(_SpecularMetallicMap);
POI_TEXTURE_NOSAMPLER(_SpecularMap);
fixed _CenterOutSpecColor;
POI_TEXTURE_NOSAMPLER(_SpecularAnisoJitterMicro);
float _SpecularAnisoJitterMirrored;
POI_TEXTURE_NOSAMPLER(_SpecularAnisoJitterMacro);
POI_TEXTURE_NOSAMPLER(_SpecularAnisoFakeUV);
POI_TEXTURE_NOSAMPLER(_AnisoTangentMap);
POI_TEXTURE_NOSAMPLER(_SpecularMask);
float _SpecularAnisoJitterMicroMultiplier;
float _SpecularAnisoJitterMacroMultiplier;
float4 _SpecularTint;
float _SpecularSmoothness;
float _Spec1Offset;
float _Spec1JitterStrength;
float _Spec2Smoothness;
float _Spec2Offset;
float _Spec2JitterStrength;
float _AnisoUseTangentMap;
float _AnisoSpec1Alpha;
float _AnisoSpec2Alpha;
float _SpecularInvertSmoothness;
half _SpecularMetallic;
float _SpecularNormal;
float _SpecularNormal1;
float _SpecularMaxBrightness;
fixed _SpecularToonStart;
fixed _SpecularToonEnd;
half4 _SpecularToonInnerOuter;
UnityIndirect ZeroIndirect()
{
    UnityIndirect ind;
    ind.diffuse = 0;
    ind.specular = 0;
    return ind;
}
half4 poiRealisticSpecular(half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness,
float3 normal, float3 halfDir,
UnityLight light, UnityIndirect gi)
{
    float perceptualRoughness = SmoothnessToPerceptualRoughness(smoothness);
    #define UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV 0
    #if UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV
        half shiftAmount = dot(normal, poiCam.viewDir);
        normal = shiftAmount < 0.0f ? normal + poiCam.viewDir * (-shiftAmount + 1e-5f): normal;
        float nv = saturate(dot(normal, poiCam.viewDir));
    #else
        half nv = abs(dot(normal, poiCam.viewDir));
    #endif
    float nl = saturate(dot(normal, light.dir));
    float nh = saturate(dot(normal, halfDir));
    half lv = saturate(dot(light.dir, poiCam.viewDir));
    half lh = saturate(dot(light.dir, halfDir));
    half diffuseTerm = DisneyDiffuse(nv, nl, lh, perceptualRoughness) * nl;
    float roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    roughness = max(roughness, 0.002);
    float V = SmithJointGGXVisibilityTerm(nl, nv, roughness);
    float D = GGXTerm(nh, roughness);
    float specularTerm = V * D * UNITY_PI;
    #ifdef UNITY_COLORSPACE_GAMMA
        specularTerm = sqrt(max(1e-4h, specularTerm));
    #endif
    specularTerm = max(0, specularTerm * nl);
    #if defined(_POI_SPECULARHIGHLIGHTS_OFF)
        specularTerm = 0.0;
    #endif
    half surfaceReduction;
    #ifdef UNITY_COLORSPACE_GAMMA
        surfaceReduction = 1.0 - 0.28 * roughness * perceptualRoughness;
    #else
        surfaceReduction = 1.0 / (roughness * roughness + 1.0);
    #endif
    specularTerm *= any(specColor) ? 1.0: 0.0;
    half grazingTerm = saturate(smoothness + (1 - oneMinusReflectivity));
    half3 color = diffColor * (gi.diffuse + light.color * diffuseTerm)
    + specularTerm * light.color * FresnelTerm(specColor, lh)
    + surfaceReduction * gi.specular * FresnelLerp(specColor, grazingTerm, nv);
    return half4(color, 1);
}
half3 calculateRealisticSpecular(float4 albedo, float2 uv, float4 specularTint, float specularSmoothness, float invertSmoothness, float mixAlbedoWithTint, float4 specularMap, float3 specularLight, float3 normal, float attenuation, float3 lightDirection, float nDotL, float3 halfDir)
{
    half oneMinusReflectivity;
    half3 finalSpecular;
    UnityLight unityLight;
    unityLight.color = specularLight;
    unityLight.dir = lightDirection;
    unityLight.ndotl = nDotL;
    
    if (float(1) == 0)
    {
        half3 diffColor = EnergyConservationBetweenDiffuseAndSpecular(albedo, specularMap.rgb * specularTint.rgb, /*out*/ oneMinusReflectivity);
        finalSpecular = poiRealisticSpecular(diffColor, specularMap.rgb, oneMinusReflectivity, specularMap.a * specularSmoothness * lerp(1, -1, invertSmoothness), normal, halfDir, unityLight, ZeroIndirect());
    }
    else
    {
        half3 diffColor = EnergyConservationBetweenDiffuseAndSpecular(albedo, specularTint.rgb, /*out*/ oneMinusReflectivity);
        float smoothness = max(max(specularMap.r, specularMap.g), specularMap.b);
        finalSpecular = poiRealisticSpecular(diffColor, 1, oneMinusReflectivity, smoothness * specularSmoothness * lerp(1, -1, invertSmoothness), normal, halfDir, unityLight, ZeroIndirect());
    }
    finalSpecular *= lerp(1, albedo.rgb, mixAlbedoWithTint);
    return finalSpecular;
}
half3 calculateToonSpecular(float4 albedo, float2 uv, float2 specularToonInnerOuter, float specularMixAlbedoIntoTint, float smoothnessFrom, float4 specularMap, float3 specularLight, float3 normal, float3 halfDir, float attenuation)
{
    half3 finalSpecular = smoothstep(1 - specularToonInnerOuter.y, 1 - specularToonInnerOuter.x, dot(halfDir, normal)) * specularLight;
    
    if (smoothnessFrom == 0)
    {
        finalSpecular.rgb *= specularMap.rgb * lerp(1, albedo.rgb, specularMixAlbedoIntoTint);
        finalSpecular *= specularMap.a;
    }
    else
    {
        finalSpecular *= specularMap.r * lerp(1, albedo.rgb, specularMixAlbedoIntoTint);
    }
    return finalSpecular;
}
float3 strandSpecular(float TdotL, float TdotV, float specPower, float nDotL)
{
    #ifdef FORWARD_ADD_PASS
        nDotL *= poiLight.attenuation * poiLight.additiveShadow;
    #endif
    float Specular = saturate(nDotL) * pow(saturate(sqrt(1.0 - (TdotL * TdotL)) * sqrt(1.0 - (TdotV * TdotV)) - TdotL * TdotV), specPower);
    half normalization = sqrt((specPower + 1) * ((specPower) + 1)) / (8 * pi);
    Specular *= normalization;
    return Specular;
}
half3 AnisotropicSpecular(
    float specWhatTangent, float anisoUseTangentMap, float specularSmoothness, float spec2Smoothness,
    float anisoSpec1Alpha, float anisoSpec2Alpha, float4 specularTint, float specularMixAlbedoIntoTint, float4 specularMap, float3 specularLight, float3 lightDirection, float3 halfDir, float nDotL, float jitter, float4 packedTangentMap, in float4 albedo)
{
    float3 tangentOrBinormal = specWhatTangent ? poiMesh.tangent: poiMesh.binormal;
    float3 normalLocalAniso = lerp(float3(0, 0, 1), UnpackNormal(packedTangentMap), anisoUseTangentMap);
    normalLocalAniso = BlendNormals(normalLocalAniso, poiMesh.tangentSpaceNormal);
    float3 normalDirectionAniso = Unity_SafeNormalize(mul(normalLocalAniso, poiTData.tangentTransform));
    float3 tangentDirection = mul(poiTData.tangentTransform, tangentOrBinormal).xyz;
    float3 viewReflectDirectionAniso = reflect(-poiCam.viewDir, normalDirectionAniso); // possible bad negation
    float3 tangentDirectionMap = mul(poiTData.tangentToWorld, float3(normalLocalAniso.rg, 0.0)).xyz;
    tangentDirectionMap = normalize(lerp(tangentOrBinormal, tangentDirectionMap, anisoUseTangentMap));
    tangentDirectionMap += float(0) +jitter;
    float TdotL = dot(lightDirection, tangentDirectionMap);
    float TdotV = dot(poiCam.viewDir, tangentDirectionMap);
    float TdotH = dot(halfDir, tangentDirectionMap);
    half specPower = RoughnessToSpecPower(1.0 - specularSmoothness * specularMap.a);
    half spec2Power = RoughnessToSpecPower(1.0 - spec2Smoothness * specularMap.a);
    half Specular = 0;
    float3 spec = strandSpecular(TdotL, TdotV, specPower, nDotL) * anisoSpec1Alpha;
    float3 spec2 = strandSpecular(TdotL, TdotV, spec2Power, nDotL) * anisoSpec2Alpha;
    return max(spec, spec2) * specularMap.rgb * specularTint.a * specularLight * lerp(1, albedo.rgb, specularMixAlbedoIntoTint);
}
inline float3 toonAnisoSpecular(float specWhatTangent, float anisoUseTangentMap, float3 lightDirection, float halfDir, float4 specularMap, float nDotL, fixed gradientStart, fixed gradientEnd, float4 specColor, float4 finalColor, fixed metallic, float jitter, float mirrored, float4 packedTangentMap)
{
    float3 tangentOrBinormal = specWhatTangent ? poiMesh.tangent: poiMesh.binormal;
    float3 normalLocalAniso = lerp(float3(0, 0, 1), UnpackNormal(packedTangentMap), anisoUseTangentMap);
    normalLocalAniso = BlendNormals(normalLocalAniso, poiMesh.tangentSpaceNormal);
    float3 normalDirectionAniso = Unity_SafeNormalize(mul(normalLocalAniso, poiTData.tangentTransform));
    float3 tangentDirection = mul(poiTData.tangentTransform, tangentOrBinormal).xyz;
    float3 viewReflectDirectionAniso = reflect(-poiCam.viewDir, normalDirectionAniso); // possible bad negation
    float3 tangentDirectionMap = mul(poiTData.tangentToWorld, float3(normalLocalAniso.rg, 0.0)).xyz;
    tangentDirectionMap = normalize(lerp(tangentOrBinormal, tangentDirectionMap, anisoUseTangentMap));
    if (!mirrored)
    {
        tangentDirectionMap += jitter;
    }
    float TdotL = dot(lightDirection, tangentDirectionMap);
    float TdotV = dot(poiCam.viewDir, tangentDirectionMap);
    float TdotH = dot(halfDir, tangentDirectionMap);
    float specular = saturate(sqrt(1.0 - (TdotL * TdotL)) * sqrt(1.0 - (TdotV * TdotV)) - TdotL * TdotV);
    fixed smoothAlpha = specular;
    if (mirrored)
    {
        smoothAlpha = max(specular - jitter, 0);
    }
    specular = smoothstep(gradientStart, gradientEnd, smoothAlpha);
    #ifdef FORWARD_ADD_PASS
        nDotL *= poiLight.attenuation * poiLight.additiveShadow;
    #endif
    return saturate(nDotL) * specular * poiLight.color * specColor * specularMap.rgb * lerp(1, finalColor, metallic) * specularMap.a;
}
inline float SpecularHQ(half roughness, half dotNH, half dotLH)
{
    roughness = saturate(roughness);
    roughness = max((roughness * roughness), 0.002);
    half roughnessX2 = roughness * roughness;
    half denom = dotNH * dotNH * (roughnessX2 - 1.0) + 1.0f;
    half D = roughnessX2 / (3.14159 * denom * denom);
    half k = roughness / 2.0f;
    half k2 = k * k;
    half invK2 = 1.0f - k2;
    half vis = rcp(dotLH * dotLH * invK2 + k2);
    float specTerm = vis * D;
    return specTerm;
}
float3 calculateNewSpecular(in float3 specularMap, uint colorFrom, in float4 albedo, in float3 specularTint, in float specularMetallic, in float specularSmoothness, in half dotNH, in half dotLH, in float3 lightColor, in float attenuation)
{
    float3 specColor = specularTint;
    float metallic = specularMetallic;
    float roughness = 1 - specularSmoothness;
    float perceptualRoughness = roughness;
    float3 specMapColor = lerp(specularMap, 1, colorFrom);
    float3 specularColor = lerp(DielectricSpec.rgb * specMapColor, lerp(specularMap, albedo.rgb, colorFrom), metallic);
    return clamp(specularColor * lightColor * attenuation * specularTint * SpecularHQ(perceptualRoughness, dotNH, dotLH), 0, lightColor * specularTint);
}
float3 calculateSpecular(in float4 albedo)
{
    half3 finalSpecular = 0;
    half3 finalSpecular1 = 0;
    float4 realisticAlbedo = albedo;
    float4 realisticAlbedo1 = albedo;
    float4 specularMap = POI2D_SAMPLER_PAN(_SpecularMap, _MainTex, poiMesh.uv[float(0)], float4(0,0,0,0));
    half metallic = POI2D_SAMPLER_PAN(_SpecularMetallicMap, _MainTex, poiMesh.uv[float(0)], float4(0,0,0,0)).r * float(0);
    half specularMask = POI2D_SAMPLER_PAN(_SpecularMask, _MainTex, poiMesh.uv[float(0)], float4(0,0,0,0)).r;
    float attenuation = saturate(poiLight.nDotL);
    float3 specularLightColor = poiLight.color;
    
    if (float(0))
    {
        specularLightColor = clamp(poiLight.color, 0, float(0));
    }
    #ifdef FORWARD_ADD_PASS
        attenuation *= poiLight.attenuation * poiLight.additiveShadow;
    #endif
    #ifdef POI_LIGHTING
        
        if (float(1) == 0 && float(0) == 1)
        {
            attenuation = poiLight.rampedLightMap;
        }
    #endif
    
    if (float(1) == 1) // Realistic
    {
        if (float(1) == 1)
        {
            specularMap.a = specularMap.r;
            specularMap.rgb = 1;
        }
        if (float(0))
        {
            specularMap.a = 1 - specularMap.a;
        }
        finalSpecular += calculateNewSpecular(specularMap.rgb, float(1), realisticAlbedo, float4(1,1,1,1), metallic, float(0.731) * specularMap.a, poiLight.dotNH, poiLight.dotLH, specularLightColor, attenuation);
    }
    
    if (float(1) == 4)
    {
        float jitter = 0;
        float microJitter = POI2D_SAMPLER_PAN(_SpecularAnisoJitterMicro, _MainTex, float2(poiMesh.uv[float(0)]), float4(0,0,0,0)).r;
        fixed jitterOffset = (1 - float(0)) * .5;
        jitter += (POI2D_SAMPLER_PAN(_SpecularAnisoJitterMicro, _MainTex, float2(poiMesh.uv[float(0)]), float4(0,0,0,0)).r - jitterOffset) * float(0);
        jitter += (POI2D_SAMPLER_PAN(_SpecularAnisoJitterMacro, _MainTex, float2(poiMesh.uv[float(0)]), float4(0,0,0,0)).r - jitterOffset) * float(0);
        jitter += float(0);
        float4 packedTangentMap = POI2D_SAMPLER_PAN(_AnisoTangentMap, _MainTex, poiMesh.uv[float(0)], float4(0,0,0,0));
        finalSpecular += toonAnisoSpecular(float(0), float(0), poiLight.direction, poiLight.halfDir, specularMap, poiLight.nDotL, float(0.95), float(1), float4(1,1,1,1), albedo, metallic, jitter, float(0), packedTangentMap);
        finalSpecular *= attenuation;
    }
    #ifdef FORWARD_BASE_PASS
        
        if (float(1) == 2) // Toon
        {
            finalSpecular += calculateToonSpecular(albedo, poiMesh.uv[0], float4(0.25,0.3,0,1), metallic, float(1), specularMap, specularLightColor, poiMesh.normals[float(1)], poiLight.halfDir, poiLight.attenuation);
            finalSpecular *= float4(1,1,1,1);
        }
        
        if (float(1) == 3) // anisotropic
        {
            float jitter = 0;
            float microJitter = POI2D_SAMPLER_PAN(_SpecularAnisoJitterMicro, _MainTex, float2(poiMesh.uv[float(0)]), float4(0,0,0,0)).r;
            fixed jitterOffset = (1 - float(0)) * .5;
            jitter += (POI2D_SAMPLER_PAN(_SpecularAnisoJitterMicro, _MainTex, float2(poiMesh.uv[float(0)]), float4(0,0,0,0)).r - jitterOffset) * float(0);
            jitter += (POI2D_SAMPLER_PAN(_SpecularAnisoJitterMacro, _MainTex, float2(poiMesh.uv[float(0)]), float4(0,0,0,0)).r - jitterOffset) * float(0);
            jitter += float(0);
            float4 packedTangentMap = POI2D_SAMPLER_PAN(_AnisoTangentMap, _MainTex, poiMesh.uv[float(0)], float4(0,0,0,0));
            finalSpecular += AnisotropicSpecular(float(0), float(0), float(0.731), float(0), float(1), float(1), float4(1,1,1,1), metallic, specularMap, specularLightColor, poiLight.direction, poiLight.halfDir, poiLight.nDotL, jitter, packedTangentMap, albedo);
            finalSpecular *= float4(1,1,1,1);
            finalSpecular *= attenuation;
        }
    #endif
    #ifdef VERTEXLIGHT_ON
        for (int index = 0; index < 4; index++)
        {
            
            if (float(1) == 1) // Realistic
            {
                finalSpecular += calculateNewSpecular(specularMap.rgb, float(1), realisticAlbedo, float4(1,1,1,1), metallic, float(0.731) * specularMap.a, poiLight.vDotNH[index], poiLight.vDotLH[index], poiLight.vColor[index], poiLight.vAttenuationDotNL[index]);
            }
        }
    #endif
    finalSpecular *= float4(1,1,1,1).a;
    finalSpecular = finalSpecular.rgb;
    finalSpecular *= specularMask;
    return finalSpecular + finalSpecular1;
}
#endif
