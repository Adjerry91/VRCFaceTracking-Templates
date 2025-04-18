# VRCFaceTracking-Templates

VRChat Face Tracking Unity templates to be used with [VRCFaceTracking](https://github.com/benaclejames/VRCFaceTracking)

## Prerequisites
* Unity 2022 (Required after v5.2.0+)
* VRChat SDK 3.7.0+ (Required after v6.3.2+ for VRC Constraints used in the face tracking debug panel)
* [VRCFaceTracking](https://docs.vrcft.io/docs/intro/getting-started) v5 setup and working. i.e. Test public face tracking avatars first before doing customs. 
* Avatar with [SRanipal](https://docs.vrcft.io/docs/v4.0/category/intermediate), [ARkit](https://arkit-face-blendshapes.com/), and [UnifiedExpressions](https://docs.vrcft.io/docs/tutorial-avatars/tutorial-avatars-extras/unified-blendshapes) Blend Shapes - _Case Sensitive_ 
Note what blendshapes are being used on the avatar. If you don’t know take look at this [Face Tracking Conversion](https://docs.google.com/spreadsheets/d/118jo960co3Mgw8eREFVBsaJ7z0GtKNr52IB4Bz99VTA/edit) for naming
* Avoid unpacking FBX, make sure eye bones are assigned rig configuration before unpacking.
* Face tracking animations are pointed to the ```Body``` skinned mesh render by default. If face tracking shapes are on a different skinned mesh render, you will need to change VRCFury component to rewrite animations clips prefix. For example if face tracking shapes are on ```Face``` mesh you will need to rewrite ```Body``` to ```Face```. See Additional Setup - [Non-Standard Mesh Names](https://github.com/Adjerry91/VRCFaceTracking-Templates/wiki/Face-Tracking-Template-Setup#additional-setup---non-standard-mesh-names) section 

## Setup 

1. Add/Import [VRCFury](https://vrcfury.com/download)
2. Add/Import VRCFT Jerry's Templates from [Jerry's VRCFT Templates Listing](https://adjerry91.github.io/VRCFaceTracking-Templates/) repository listing URL https://Adjerry91.github.io/VRCFaceTracking-Templates/index.json
3. Go to ```Packages/VRCFT - Jerry's Templates/Prefabs``` in Unity project window
4. Add the corresponding face tracking prefab onto your avatar
   * If not sure which standard shapes are being used, use the following [Face Tracking Spreadsheet](https://docs.google.com/spreadsheets/d/118jo960co3Mgw8eREFVBsaJ7z0GtKNr52IB4Bz99VTA/edit?usp=sharing)
5. Add ```FacialExpressionsDisabled``` and set to "FALSE" to the hand gestures transitions. The Prefab will NOT do this automatically. See detailed Wiki Guide for more information.

[Detailed Wiki guide](https://github.com/Adjerry91/VRCFaceTracking-Templates/wiki/Face-Tracking-Template-Setup)

## Video Guides
- Template setup guide [https://youtu.be/Ub1c6PiVc9U](https://youtu.be/Ub1c6PiVc9U)
- Template debug guide [https://youtu.be/Q4BN0xp_Tsg](https://youtu.be/Q4BN0xp_Tsg)

_Please note video guides can get out of date_

## Change Log
[VRCFT - Jerry's Template Change Log](https://github.com/Adjerry91/VRCFaceTracking-Templates/blob/main/Packages/adjerry91.vrcft.templates/CHANGELOG.md)

## Support

Post in ```#template-help``` for advance support on [Jerry's Face Tracking Discord](https://discord.gg/yQtTsVSqx8)

## Face Tracking Animation Tips:

* It is recommend to remove template and add again when updating versions.
* Do not embed templates without VRCFury or modifiying animators in products. Embedding will break face tracking template support for future updates.
* Change FBX rig configuration muscle settings for bone rotation adjustments.
* OSC does not work on Test Avatar Uploads
* Eye movement should be on bones by default. Any eye movements with blendshapes will double the movement amount. To remove doubling of rotation unpack and remove the "Eye Rotation" sub prefab.
* All parameters are exposed on the template prefab. Do NOT use FT/v2/ parameters as these are the raw data coming from OSC and majority of them are floats that are not networked sync'd. Please use OSCm/Proxy/FT/v2 parameters when using in custom animations.
