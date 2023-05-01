# VRCFaceTracking-Templates

VRChat Face Tracking Unity templates to be used with [VRCFaceTracking](https://github.com/benaclejames/VRCFaceTracking)

## Prerequisites

* [Latest VRCFaceTracking Release](https://github.com/benaclejames/VRCFaceTracking/releases/latest)
* Avatar with [SRanipal](https://github.com/benaclejames/VRCFaceTracking/wiki/Blend-Shapes-Setup), [ARKit (Perfect Sync)](https://arkit-face-blendshapes.com/), or [Unified Expressions](https://docs.google.com/spreadsheets/d/118jo960co3Mgw8eREFVBsaJ7z0GtKNr52IB4Bz99VTA/edit?usp=sharing) face tracking shapekeys
* Shapekeys must be on the mesh named "Body" parented to the root armature

## Setup 

1. Import the latest template from [Releases](https://github.com/Adjerry91/VRCFaceTracking-Templates/releases/latest)
2. Merge animator controller per the face tracking type on the avatar (SRanipal, ARKit, or UnifiedExpressions) via [AV3 Avatar Manager](https://github.com/VRLabs/Avatars-3.0-Manager)
3. Copy parameters with [AV3 Avatar Manager](https://github.com/VRLabs/Avatars-3.0-Manager)
4. Add sub menu to menu of the avatar for face tracking control settings

Detailed setup PDF guide included in the *VRCFaceTracking* folder

## Support

See the avatar-help-form for advance support on VRCFaceTracking Discord 

[![Discord](https://discord.com/api/guilds/849300336128032789/widget.png)](https://discord.gg/Fh4FNehzKn)

## Face Tracking Animation Tips:

* Modification to the thresholds may be needed for some animation sensitivity for different faces
* Driver, binary, and smooth layer require write defaults ON
*	:warning: There is bug on older version of the template that causes animator crashes, severe lag with the avatar to you and others. This is caused when there is many blend trees referencing the _Do_Nothing animation, the newest template v3.1.4 and higher. 

