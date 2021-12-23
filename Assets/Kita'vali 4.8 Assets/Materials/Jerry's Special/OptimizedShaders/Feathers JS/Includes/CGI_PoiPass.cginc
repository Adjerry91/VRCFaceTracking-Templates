#ifndef POI_PASS
#define POI_PASS
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"
#include "UnityShaderVariables.cginc"
#ifdef POI_META_PASS
	#include "UnityMetaPass.cginc"
#endif
#include "CGI_PoiMacros.cginc"
#include "CGI_PoiDefines.cginc"
#include "CGI_FunctionsArtistic.cginc"
#include "CGI_Poicludes.cginc"
#include "CGI_PoiHelpers.cginc"
#include "CGI_PoiBlending.cginc"
#include "CGI_PoiPenetration.cginc"
#include "CGI_PoiVertexManipulations.cginc"
#include "CGI_PoiSpawnInVert.cginc"
#include "CGI_PoiV2F.cginc"
#include "CGI_PoiVert.cginc"
#ifdef TESSELATION
	#include "CGI_PoiTessellation.cginc"
#endif
#include "CGI_PoiDithering.cginc"
#ifdef COLOR_GRADING_LOG_VIEW
	#include "CGI_PoiAudioLink.cginc"
#endif
#include "CGI_PoiData.cginc"
#include "CGI_PoiSpawnInFrag.cginc"
#ifdef WIREFRAME
	#include "CGI_PoiWireframe.cginc"
#endif
#ifdef FUR
#endif
#ifdef VIGNETTE_MASKED
	#include "CGI_PoiLighting.cginc"
#endif
#include "CGI_PoiMainTex.cginc"
#include "CGI_PoiBlending.cginc"
#include "CGI_PoiGrab.cginc"
#ifdef _EMISSION
	#include "CGI_PoiEmission.cginc"
#endif
#include "CGI_PoiAlphaToCoverage.cginc"
#include "CGI_PoiFrag.cginc"
#endif
