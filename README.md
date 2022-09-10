# VRCFaceTracking-Templates

VRC Face Tracking Unity Demo setup for [VRCFaceTracking](https://github.com/benaclejames/VRCFaceTracking)

## Demo Avatars:

* Modified version of Kita'vali v4.8 that is available at https://discord.com/invite/uwN8dKU</li>
  * PC FaceTracking Version
  * PC FaceTracking, Da-vali extra feathers, and sweater
  * Quest FaceTracking Version
* SRanipal Shieh

## Setup Order:
1. VRCSDK
2. Standard [Poiyomi Toon Shader](https://github.com/poiyomi/PoiyomiToonShader) or my [Custom Poiyomi Audio Link](https://github.com/Adjerry91/PoiyomiToonShader/releases) Version 
3. Recommended [AV3Emulator](https://github.com/lyuma/Av3Emulator/releases) (Testing in unity)
5. Recommended [VRLABS Gesture Manager](https://github.com/VRLabs/Avatars-3.0-Manager/releases) (Merging Templates)
4. Optional [Audio Link](https://github.com/llealloo/vrc-udon-audio-link/releases) (Testing Audio Link in Unity)
5. Optional [VRCFacetracking Binary Parameter Tool](https://github.com/regzo2/BinaryParameterTool/releases) (Used to create the booleon float drive and smoothing state machines)
6. Import [VRCFaceTracking-UnityDemo](https://github.com/Adjerry91/VRCFaceTracking-UnityDemo/releases)

## Face Tracking Animation Tips:

* Modification to the thresholds may be needed for some animation sensitivity for different faces
*	:warning: There is bug on older version of the template that causes animator crashes, severe lag with the avatar to you and others. This is caused when there is many blend trees referencing the _Do_Nothing animation, the newest template v3.1.4 and higher . 

