# VRCFaceTracking-Templates

VRChat Face Tracking Unity templates to be used with [VRCFaceTracking](https://github.com/benaclejames/VRCFaceTracking)

## Prerequisites

* [Latest VRCFaceTracking Release](https://github.com/benaclejames/VRCFaceTracking/releases/latest)
* Avatar with [SRanipal](https://github.com/benaclejames/VRCFaceTracking/wiki/Blend-Shapes-Setup), [ARKit (Perfect Sync)](https://arkit-face-blendshapes.com/), or [Unified Expressions](https://docs.google.com/spreadsheets/d/118jo960co3Mgw8eREFVBsaJ7z0GtKNr52IB4Bz99VTA/edit?usp=sharing) face tracking shapekeys
* Face tracking animations are pointed to the "Body" skinned mesh render by default. If face tracking shapes are on a different skinned mesh render, you will need to change VRCFury component to rewrite animations clips prefix. For example if face tracking shapes are on "Face" mesh you will need to rewrite "Body" to "Face"

## Setup 

1. Import the latest template from [Releases](https://github.com/Adjerry91/VRCFaceTracking-Templates/releases/latest)
2. Add [VRCFury](https://vrcfury.com/) Face Tracking Prefab (SRanipal, ARKit, or UnifiedExpressions) to your avatar 

Detailed setup PDF guide included in the *VRCFaceTracking* folder

## Support

Post in #template-help for advance support on [Jerry's Face Tracking Discord](https://discord.gg/yQtTsVSqx8)

## Face Tracking Animation Tips:

* Modification to the thresholds may be needed for some animation sensitivity for different faces
* Driver, binary, and smooth layer require write defaults ON


