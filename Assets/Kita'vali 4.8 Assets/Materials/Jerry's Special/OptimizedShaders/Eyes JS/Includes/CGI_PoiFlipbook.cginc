#ifndef POI_FLIPBOOK
#define POI_FLIPBOOK
#if defined(PROP_FLIPBOOKTEXARRAY) || !defined(OPTIMIZER_ENABLED)
    UNITY_DECLARE_TEX2DARRAY(_FlipbookTexArray); float4 _FlipbookTexArray_ST;
#endif
#if defined(PROP_FLIPBOOKMASK) || !defined(OPTIMIZER_ENABLED)
    POI_TEXTURE_NOSAMPLER(_FlipbookMask);
#endif
float4 _FlipbookColor;
float _FlipbookFPS;
float _FlipbookTotalFrames;
float4 _FlipbookScaleOffset;
float _FlipbookTiled;
float _FlipbookCurrentFrame;
float _FlipbookEmissionStrength;
float _FlipbookRotation;
float _EnableFlipbook;
float _FlipbookTexArrayUV;
float _FlipbookAlphaControlsFinalAlpha;
float _FlipbookRotationSpeed;
float _FlipbookIntensityControlsAlpha;
float _FlipbookColorReplaces;
float2 _FlipbookTexArrayPan;
float _FlipbookReplace;
float _FlipbookMultiply;
float _FlipbookAdd;
float _FlipbookMovementType;
float4 _FlipbookStartEndOffset;
float _FlipbookMovementSpeed;
float _FlipbookCrossfadeEnabled;
float2 _FlipbookCrossfadeRange;
float _FlipbookHueShiftEnabled;
float _FlipbookHueShiftSpeed;
float _FlipbookHueShift;
float4 flipBookPixel;
float4 flipBookPixelMultiply;
float flipBookMask;
half _AudioLinkFlipbookScaleBand;
half4 _AudioLinkFlipbookScale;
half _AudioLinkFlipbookAlphaBand;
half2 _AudioLinkFlipbookAlpha;
half _AudioLinkFlipbookEmissionBand;
half2 _AudioLinkFlipbookEmission;
half _AudioLinkFlipbookFrameBand;
half2 _AudioLinkFlipbookFrame;
#ifndef POI_SHADOW
    void applyFlipbook(inout float4 finalColor, inout float3 flipbookEmission)
    {
        #if defined(PROP_FLIPBOOKMASK) || !defined(OPTIMIZER_ENABLED)
            flipBookMask = POI2D_SAMPLER_PAN(_FlipbookMask, _MainTex, poiMesh.uv[float(0)], float4(0,0,0,0)).r;
        #else
            flipBookMask = 1;
        #endif
        float4 flipbookScaleOffset = float4(1,1,0,0);
        #ifdef POI_AUDIOLINK
            flipbookScaleOffset.xy += lerp(float4(0,0,0,0).xy, float4(0,0,0,0).zw, poiMods.audioLink[float(0)]);
        #endif
        flipbookScaleOffset.xy = 1 - flipbookScaleOffset.xy;
        float2 uv = frac(poiMesh.uv[float(0)]);
        float theta = radians(float(0) + _Time.z * float(0));
        float cs = cos(theta);
        float sn = sin(theta);
        float2 spriteCenter = flipbookScaleOffset.zw + .5;
        uv = float2((uv.x - spriteCenter.x) * cs - (uv.y - spriteCenter.y) * sn + spriteCenter.x, (uv.x - spriteCenter.x) * sn + (uv.y - spriteCenter.y) * cs + spriteCenter.y);
        float2 newUV = remap(uv, float2(0, 0) + flipbookScaleOffset.xy / 2 + flipbookScaleOffset.zw, float2(1, 1) - flipbookScaleOffset.xy / 2 + flipbookScaleOffset.zw, float2(0, 0), float2(1, 1));
        
        if (float(0) == 0)
        {
            if (max(newUV.x, newUV.y) > 1 || min(newUV.x, newUV.y) < 0)
            {
                flipBookPixel = 0;
                return;
            }
        }
        #if defined(PROP_FLIPBOOKTEXARRAY) || !defined(OPTIMIZER_ENABLED)
            float currentFrame = fmod(float(-1), float(17));
            if (float(-1) < 0)
            {
                currentFrame = (_Time.y / (1 / float(10))) % float(17);
            }
            #ifdef POI_AUDIOLINK
                currentFrame += lerp(float4(0,0,0,0).x, float4(0,0,0,0).y, poiMods.audioLink[float(0)]);
            #endif
            flipBookPixel = UNITY_SAMPLE_TEX2DARRAY(_FlipbookTexArray, float3(TRANSFORM_TEX(newUV, _FlipbookTexArray) + _Time.x * float4(0,0,0,0), floor(currentFrame)));
            
            if (float(0))
            {
                float4 flipbookNextPixel = UNITY_SAMPLE_TEX2DARRAY(_FlipbookTexArray, float3(TRANSFORM_TEX(newUV, _FlipbookTexArray) + _Time.x * float4(0,0,0,0), floor((currentFrame + 1) % float(17))));
                flipBookPixel = lerp(flipBookPixel, flipbookNextPixel, smoothstep(float4(0.75,1,0,1).x, float4(0.75,1,0,1).y, frac(currentFrame)));
            }
        #else
            flipBookPixel = 1;
        #endif
        
        if (float(0))
        {
            flipBookPixel.a = poiMax(flipBookPixel.rgb);
        }
        
        if (float(0))
        {
            flipBookPixel.rgb = float4(1,1,1,1).rgb;
        }
        else
        {
            flipBookPixel.rgb *= float4(1,1,1,1).rgb;
        }
        #ifdef POI_BLACKLIGHT
            
            if (_BlackLightMaskFlipbook != 4)
            {
                flipBookMask *= blackLightMask[_BlackLightMaskFlipbook];
            }
        #endif
        
        if (float(0))
        {
            flipBookPixel.rgb = hueShift(flipBookPixel.rgb, float(0) + _Time.x * float(3));
        }
        half flipbookAlpha = 1;
        #ifdef POI_AUDIOLINK
            flipbookAlpha = saturate(lerp(float4(1,1,0,0).x, float4(1,1,0,0).y, poiMods.audioLink[float(0)]));
        #endif
        finalColor.rgb = lerp(finalColor.rgb, flipBookPixel.rgb, flipBookPixel.a * float4(1,1,1,1).a * float(0) * flipBookMask * flipbookAlpha);
        finalColor.rgb = finalColor + flipBookPixel.rgb * float(0) * flipBookMask * flipbookAlpha;
        finalColor.rgb = finalColor * lerp(1, flipBookPixel.rgb, flipBookPixel.a * float4(1,1,1,1).a * flipBookMask * float(0) * flipbookAlpha);
        
        if (float(0))
        {
            finalColor.a = lerp(finalColor.a, flipBookPixel.a * float4(1,1,1,1).a, flipBookMask);
        }
        float flipbookEmissionStrength = float(2);
        #ifdef POI_AUDIOLINK
            flipbookEmissionStrength += max(lerp(float4(0,0,0,0).x, float4(0,0,0,0).y, poiMods.audioLink[float(0)]), 0);
        #endif
        flipbookEmission = lerp(0, flipBookPixel.rgb * flipbookEmissionStrength, flipBookPixel.a * float4(1,1,1,1).a * flipBookMask * flipbookAlpha);
    }
#else
    float applyFlipbookAlphaToShadow(float2 uv)
    {
        
        if (float(0))
        {
            float flipbookShadowAlpha = 0;
            float4 flipbookScaleOffset = float4(1,1,0,0);
            flipbookScaleOffset.xy = 1 - flipbookScaleOffset.xy;
            float theta = radians(float(0));
            float cs = cos(theta);
            float sn = sin(theta);
            float2 spriteCenter = flipbookScaleOffset.zw + .5;
            uv = float2((uv.x - spriteCenter.x) * cs - (uv.y - spriteCenter.y) * sn + spriteCenter.x, (uv.x - spriteCenter.x) * sn + (uv.y - spriteCenter.y) * cs + spriteCenter.y);
            float2 newUV = remap(uv, float2(0, 0) + flipbookScaleOffset.xy / 2 + flipbookScaleOffset.zw, float2(1, 1) - flipbookScaleOffset.xy / 2 + flipbookScaleOffset.zw, float2(0, 0), float2(1, 1));
            #if defined(PROP_FLIPBOOKTEXARRAY) || !defined(OPTIMIZER_ENABLED)
                float currentFrame = fmod(float(-1), float(17));
                if (float(-1) < 0)
                {
                    currentFrame = (_Time.y / (1 / float(10))) % float(17);
                }
                half4 flipbookColor = UNITY_SAMPLE_TEX2DARRAY(_FlipbookTexArray, float3(TRANSFORM_TEX(newUV, _FlipbookTexArray) + _Time.x * float4(0,0,0,0), floor(currentFrame)));
                
                if (float(0))
                {
                    float4 flipbookNextPixel = UNITY_SAMPLE_TEX2DARRAY(_FlipbookTexArray, float3(TRANSFORM_TEX(newUV, _FlipbookTexArray) + _Time.x * float4(0,0,0,0), floor((currentFrame + 1) % float(17))));
                    flipbookColor = lerp(flipbookColor, flipbookNextPixel, smoothstep(float4(0.75,1,0,1).x, float4(0.75,1,0,1).y, frac(currentFrame)));
                }
            #else
                half4 flipbookColor = 1;
            #endif
            if (float(0))
            {
                flipbookColor.a = poiMax(flipbookColor.rgb);
            }
            
            if (float(0) == 0)
            {
                if (max(newUV.x, newUV.y) > 1 || min(newUV.x, newUV.y) < 0)
                {
                    flipbookColor.a = 0;
                }
            }
            return flipbookColor.a * float4(1,1,1,1).a;
        }
        return 1;
    }
#endif
#endif
