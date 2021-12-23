#ifndef POI_AUDIOLINK
#define POI_AUDIOLINK
#define ALPASS_DFT                      uint2(0,4)  //Size: 128, 2
#define ALPASS_WAVEFORM                 uint2(0,6)  //Size: 128, 16
#define ALPASS_AUDIOLINK                uint2(0,0)  //Size: 128, 4
#define ALPASS_AUDIOBASS                uint2(0,0)  //Size: 128, 1
#define ALPASS_AUDIOLOWMIDS             uint2(0,1)  //Size: 128, 1
#define ALPASS_AUDIOHIGHMIDS            uint2(0,2)  //Size: 128, 1
#define ALPASS_AUDIOTREBLE              uint2(0,3)  //Size: 128, 1
#define ALPASS_AUDIOLINKHISTORY         uint2(1,0)  //Size: 127, 4
#define ALPASS_GENERALVU                uint2(0,22) //Size: 12, 1
#define ALPASS_GENERALVU_INSTANCE_TIME  uint2(2,22)
#define ALPASS_GENERALVU_LOCAL_TIME     uint2(3,22)
#define ALPASS_GENERALVU_NETWORK_TIME   uint2(4,22)
#define ALPASS_GENERALVU_PLAYERINFO     uint2(6,22)
#define ALPASS_THEME_COLOR0             uint2(0,23)
#define ALPASS_THEME_COLOR1             uint2(1,23)
#define ALPASS_THEME_COLOR2             uint2(2,23)
#define ALPASS_THEME_COLOR3             uint2(3,23)
#define ALPASS_CCINTERNAL               uint2(12,22) //Size: 12, 2
#define ALPASS_CCCOLORS                 uint2(24,22) //Size: 12, 1 (Note Color #0 is always black, Colors start at 1)
#define ALPASS_CCSTRIP                  uint2(0,24)  //Size: 128, 1
#define ALPASS_CCLIGHTS                 uint2(0,25)  //Size: 128, 2
#define ALPASS_AUTOCORRELATOR           uint2(0,27)  //Size: 128, 1
#define ALPASS_FILTEREDAUDIOLINK        uint2(0,28)  //Size: 16, 4
#define ALPASS_CHRONOTENSITY            uint2(16,28) //Size: 8, 4
#define ALPASS_FILTEREDVU               uint2(24,28) //Size: 4, 4
#define ALPASS_FILTEREDVU_INTENSITY     uint2(24,28) //Size: 4, 1
#define ALPASS_FILTEREDVU_MARKER        uint2(24,29) //Size: 4, 1
#define AUDIOLINK_SAMPHIST              3069        // Internal use for algos, do not change.
#define AUDIOLINK_SAMPLEDATA24          2046
#define AUDIOLINK_EXPBINS               24
#define AUDIOLINK_EXPOCT                10
#define AUDIOLINK_ETOTALBINS            (AUDIOLINK_EXPBINS * AUDIOLINK_EXPOCT)
#define AUDIOLINK_WIDTH                 128
#define AUDIOLINK_SPS                   48000       // Samples per second
#define AUDIOLINK_ROOTNOTE              0
#define AUDIOLINK_4BAND_FREQFLOOR       0.123
#define AUDIOLINK_4BAND_FREQCEILING     1
#define AUDIOLINK_BOTTOM_FREQUENCY      13.75
#define AUDIOLINK_BASE_AMPLITUDE        2.5
#define AUDIOLINK_DELAY_COEFFICIENT_MIN 0.3
#define AUDIOLINK_DELAY_COEFFICIENT_MAX 0.9
#define AUDIOLINK_DFT_Q                 4.0
#define AUDIOLINK_TREBLE_CORRECTION     5.0
#define COLORCHORD_EMAXBIN              192
#define COLORCHORD_IIR_DECAY_1          0.90
#define COLORCHORD_IIR_DECAY_2          0.85
#define COLORCHORD_CONSTANT_DECAY_1     0.01
#define COLORCHORD_CONSTANT_DECAY_2     0.0
#define COLORCHORD_NOTE_CLOSEST         3.0
#define COLORCHORD_NEW_NOTE_GAIN        8.0
#define COLORCHORD_MAX_NOTES            10
UNITY_DECLARE_TEX2D(_AudioTexture);
float4 _AudioTexture_ST;
fixed _AudioLinkDelay;
fixed _AudioLinkAveraging;
fixed _AudioLinkAverageRange;
fixed _EnableAudioLinkDebug;
fixed _AudioLinkDebugTreble;
fixed _AudioLinkDebugHighMid;
fixed _AudioLinkDebugLowMid;
fixed _AudioLinkDebugBass;
fixed _AudioLinkDebugAnimate;
fixed _AudioLinkTextureVisualization;
fixed _AudioLinkAnimToggle;
void AudioTextureExists()
{
	half testw = 0;
	half testh = 0;
	_AudioTexture.GetDimensions(testw, testh);
	poiMods.audioLinkTextureExists = testw >= 32;
	poiMods.audioLinkTextureExists *= float(1);
	switch(testw)
	{
		case 32: // V1
		poiMods.audioLinkVersion = 1;
		break;
		case 128: // V2
		poiMods.audioLinkVersion = 2;
		break;
		default:
		poiMods.audioLinkVersion = 1;
		break;
	}
}
float getBandAtTime(float band, fixed time, fixed width)
{
	float versionUvMultiplier = 1;
	if (poiMods.audioLinkVersion == 2)
	{
		versionUvMultiplier = 0.0625;
	}
	return UNITY_SAMPLE_TEX2D(_AudioTexture, float2(time * width, (band * .25 + .125) * versionUvMultiplier)).r;
}
void initAudioBands()
{
	AudioTextureExists();
	float versionUvMultiplier = 1;
	if (poiMods.audioLinkVersion == 2)
	{
		versionUvMultiplier = 0.0625;
	}
	if (poiMods.audioLinkTextureExists)
	{
		poiMods.audioLink.x = UNITY_SAMPLE_TEX2D(_AudioTexture, float2(float(0), .125 * versionUvMultiplier));
		poiMods.audioLink.y = UNITY_SAMPLE_TEX2D(_AudioTexture, float2(float(0), .375 * versionUvMultiplier));
		poiMods.audioLink.z = UNITY_SAMPLE_TEX2D(_AudioTexture, float2(float(0), .625 * versionUvMultiplier));
		poiMods.audioLink.w = UNITY_SAMPLE_TEX2D(_AudioTexture, float2(float(0), .875 * versionUvMultiplier));
		
		if (float(0))
		{
			float uv = saturate(float(0) + float(0.5) * .25);
			poiMods.audioLink.x += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .125 * versionUvMultiplier));
			poiMods.audioLink.y += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .375 * versionUvMultiplier));
			poiMods.audioLink.z += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .625 * versionUvMultiplier));
			poiMods.audioLink.w += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .875 * versionUvMultiplier));
			uv = saturate(float(0) + float(0.5) * .5);
			poiMods.audioLink.x += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .125 * versionUvMultiplier));
			poiMods.audioLink.y += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .375 * versionUvMultiplier));
			poiMods.audioLink.z += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .625 * versionUvMultiplier));
			poiMods.audioLink.w += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .875 * versionUvMultiplier));
			uv = saturate(float(0) + float(0.5) * .75);
			poiMods.audioLink.x += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .125 * versionUvMultiplier));
			poiMods.audioLink.y += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .375 * versionUvMultiplier));
			poiMods.audioLink.z += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .625 * versionUvMultiplier));
			poiMods.audioLink.w += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .875 * versionUvMultiplier));
			uv = saturate(float(0) + float(0.5));
			poiMods.audioLink.x += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .125 * versionUvMultiplier));
			poiMods.audioLink.y += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .375 * versionUvMultiplier));
			poiMods.audioLink.z += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .625 * versionUvMultiplier));
			poiMods.audioLink.w += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .875 * versionUvMultiplier));
			poiMods.audioLink /= 5;
		}
	}
	#ifndef OPTIMIZER_ENABLED
		
		if (float(0))
		{
			poiMods.audioLink.x = float(0);
			poiMods.audioLink.y = float(0);
			poiMods.audioLink.z = float(0);
			poiMods.audioLink.w = float(0);
			if (float(0))
			{
				poiMods.audioLink.x *= (sin(_Time.w * 3.1) + 1) * .5;
				poiMods.audioLink.y *= (sin(_Time.w * 3.2) + 1) * .5;
				poiMods.audioLink.z *= (sin(_Time.w * 3.3) + 1) * .5;
				poiMods.audioLink.w *= (sin(_Time.w * 3) + 1) * .5;
			}
			poiMods.audioLinkTextureExists = 1;
		}
		
		if (float(0))
		{
			poiMods.audioLinkTexture = UNITY_SAMPLE_TEX2D(_AudioTexture, poiMesh.uv[0]);
		}
	#endif
}
#endif
#ifdef AUDIOLINK_STANDARD_INDEXING
    #define AudioLinkData(xycoord) tex2Dlod(_AudioTexture, float4(uint2(xycoord) * _AudioTexture_TexelSize.xy, 0, 0))
#else
    #define AudioLinkData(xycoord) _AudioTexture[uint2(xycoord)]
#endif
