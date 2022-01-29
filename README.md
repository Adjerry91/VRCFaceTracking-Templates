# VRCFaceTracking-UnityDemo

VRC Face Tracking Unity Demo setup for [VRCFaceTracking](https://github.com/benaclejames/VRCFaceTracking)

## Demo Avatars:

* Modified version of Kita'vali v4.8 that is available at https://discord.com/invite/uwN8dKU</li>
  * PC FaceTracking Version
  * PC FaceTracking, Da-vali extra feathers, and sweater
  * Quest FaceTracking Version
* SRanipal Shieh

## Setup Order:
1. VRCSDK
2. DynamicBones
3. Standard [Poiyomi Toon Shader](https://github.com/poiyomi/PoiyomiToonShader) or my [Custom Poiyomi Audio Link](https://github.com/Adjerry91/PoiyomiToonShader/releases) Version 
4. Optional [AV3Emulator](https://github.com/lyuma/Av3Emulator/releases) (Emulator has to be disabled before uploading)
5. Optional [Audio Link](https://github.com/llealloo/vrc-udon-audio-link/releases)
6. Optional [VRCFacetracking Binary Parameter Tool](https://github.com/regzo2/BinaryParameterTool/releases) (Used to create the booleon state machines)
7. Import [VRCFaceTracking-UnityDemo](https://github.com/Adjerry91/VRCFaceTracking-UnityDemo/releases)

## Face Tracking Animation Tips:

* Modification to the thresholds may be needed for some animation sensitivity
* Do not mix write defaults on with write default off as it will cause undesirable animations. These animation are require to run with write defaults on.
* When using write defaults off the animator works from top to bottom. Reset layer need to happen before other layers (at the top). The reset layer is to drive the animations to default state after animations.

