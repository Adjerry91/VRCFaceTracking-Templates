# VRCFaceTracking-Templates

VRChat Face Tracking Unity templates to be used with [VRCFaceTracking](https://github.com/benaclejames/VRCFaceTracking)

## Prerequisites

* Unity 2022 (Required after v5.2.0+)
* VRChat SDK 3.7.0 (Required after v6.3.2+ for VRC Constraints used in the face tracking debug panel)
* [Latest VRCFaceTracking Release](https://github.com/benaclejames/VRCFaceTracking/releases/latest)
* Avatar with [SRanipal](https://docs.vrcft.io/docs/v4.0/category/intermediate), [ARKit (Perfect Sync)](https://arkit-face-blendshapes.com/), or [Unified Expressions](https://docs.vrcft.io/docs/tutorial-avatars/tutorial-avatars-extras/unified-blendshapes) face tracking shapekeys
* Face tracking animations are pointed to the ```Body``` skinned mesh render by default. If face tracking shapes are on a different skinned mesh render, you will need to change VRCFury component to rewrite animations clips prefix. For example if face tracking shapes are on ```Face``` mesh you will need to rewrite ```Body``` to ```Face```

## Setup 

1. Add/Import [VRCFury](https://vrcfury.com/download)
2. Add/Import VRCFT Jerry's Templates from [Jerry's VRCFT Templates Listing](https://adjerry91.github.io/VRCFaceTracking-Templates/) repository listing URL https://Adjerry91.github.io/VRCFaceTracking-Templates/index.json
3. Go to ```Packages/VRCFT - Jerry's Templates/Prefabs``` in Unity project window
4. Add the corresponding face tracking prefab onto your avatar
   * If not sure which standard shapes are being used, use the following [Face Tracking Spreadsheet](https://docs.google.com/spreadsheets/d/118jo960co3Mgw8eREFVBsaJ7z0GtKNr52IB4Bz99VTA/edit?usp=sharing)

[Detailed setup PDF guide](https://github.com/Adjerry91/VRCFaceTracking-Templates/blob/main/Packages/adjerry91.vrcft.templates/VRCFaceTracking%20Template%20Setup.pdf)

## Video Guides

- Template setup guide [https://youtu.be/Ub1c6PiVc9U](https://youtu.be/Ub1c6PiVc9U)
- Template debug guide [https://youtu.be/Q4BN0xp_Tsg](https://youtu.be/Q4BN0xp_Tsg)

## Change Log
[VRCFT - Jerry's Template Change Log](https://github.com/Adjerry91/VRCFaceTracking-Templates/blob/main/Packages/adjerry91.vrcft.templates/CHANGELOG.md)

## Support

Post in ```#template-help``` for advance support on [Jerry's Face Tracking Discord](https://discord.gg/yQtTsVSqx8)

## Face Tracking Animation Tips:

* It is recommend to remove template and add again when updating versions.
* Do not embed templates without VRCFury or modifiying animators in products. Embedding will break face tracking template support for future updates.
* Change FBX rig configuration muscle settings for bone rotation adjustments.
* OSC does not work on Test Avatar Uploads

