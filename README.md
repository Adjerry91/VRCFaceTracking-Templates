## Overview

[VRCFT](https://github.com/benaclejames/VRCFaceTracking) - Jerry's templates is a Unity package that uses [VRCFury](https://vrcfury.com/download)/[Modular Avatar](https://modular-avatar.nadena.dev/) prefabs that simply add face tracking animations and controllers to an avatar. When applied to an avatar will link [VRCFT](https://github.com/benaclejames/VRCFaceTracking) OSC communication to drive face tracking blendshapes on the avatar. 

## Requirements
* ⚠️Avatar with [SRanipal](https://docs.vrcft.io/docs/v4.0/category/intermediate), [ARkit](https://arkit-face-blendshapes.com/), or [UnifiedExpressions](https://docs.vrcft.io/docs/tutorial-avatars/tutorial-avatars-extras/unified-blendshapes) face tracking blend shapes (Case Sensitive). ⚠️
   * See blendshapes [Face Tracking Conversion](https://docs.google.com/spreadsheets/d/118jo960co3Mgw8eREFVBsaJ7z0GtKNr52IB4Bz99VTA/edit) for each standard naming.
   * Blendshapes are __NOT__ related to the headset you are using.
* Unity 2022
* VRChat SDK 3.7.0+ 
* [VRCFaceTracking](https://docs.vrcft.io/docs/intro/getting-started) 
* ⚠️ Face tracking animations are pointed to the ```Body``` skinned mesh render (Case Sensitive) ⚠️
   * If face tracking shapes are on a different skinned mesh render. See Additional Setup - [Non-Standard Mesh Names](https://github.com/Adjerry91/VRCFaceTracking-Templates/wiki/Face-Tracking-Template-Setup#additional-setup---non-standard-mesh-names) in the detailed guide.
   * Rewrite feature is __NOT__ available on Modular Avatar prefabs.

## Quick Setup Guide

1. Add/Import [VRCFury](https://vrcfury.com/download) or [Modular Avatar](https://modular-avatar.nadena.dev/)
2. Add/Import VRCFT Jerry's Templates from [Jerry's VRCFT Templates Listing](https://adjerry91.github.io/VRCFaceTracking-Templates/)
3. Go to ```Packages/VRCFT - Jerry's Templates/Prefabs``` in Unity project window\
![PrefabFolder](https://github.com/user-attachments/assets/0421d5d7-c237-46e1-82be-a6e05ef9a5d8)\
   _Note - Prefix of the prefab "VF" is VRCFury and "MA" is Modular Avatar_\
   _Note - There are two version of Unified Expressions template. Use normal one for TongueOut blend shape and use TongueSteps for TongueOutStep1 and TongueOutStep2 blend shapes._
4. Add the corresponding face tracking prefab onto your avatar
![Prefab](https://github.com/user-attachments/assets/5b48ab3d-5291-4cdc-ba60-718a12b32b5f)
   * If not sure which standard shapes are being used, use the following [Face Tracking Spreadsheet](https://docs.google.com/spreadsheets/d/118jo960co3Mgw8eREFVBsaJ7z0GtKNr52IB4Bz99VTA/edit?usp=sharing)
5. Add ```FacialExpressionsDisabled``` and set to "FALSE" to the hand gestures transitions. The Prefab will NOT do this automatically. See detailed Wiki Guide for more information.

### See [Detailed Wiki guide](https://github.com/Adjerry91/VRCFaceTracking-Templates/wiki/Face-Tracking-Template-Setup) for more detailed setup directions.

### Prefab not working? Check the [Troubleshooting Wiki](https://github.com/Adjerry91/VRCFaceTracking-Templates/wiki/Troubleshooting)

## Video Guides
- Template setup guide [https://youtu.be/Ub1c6PiVc9U](https://youtu.be/Ub1c6PiVc9U)
- Template debug guide [https://youtu.be/Q4BN0xp_Tsg](https://youtu.be/Q4BN0xp_Tsg)

_Please note video guides can get out of date_

## Demos

Avatars using face tracking templates are available at [Jerry's Mod](https://vrchat.com/home/launch?worldId=wrld_b24fbb7c-9369-4cff-9242-32a35d44a8e8)

## Notes

* Do __NOT__ embed (copying out the animators) templates without VRCFury or modifiying animators in products. Embedding will break face tracking template support for future updates.
* All parameters are exposed on the template prefab. Do NOT use FT/v2/ parameters as these are the raw data coming from OSC and majority of them are floats that are not networked sync'd. Please use OSCm/Proxy/FT/v2 parameters when using in custom animations.

## Change Log
[VRCFT - Jerry's Template Change Log](https://github.com/Adjerry91/VRCFaceTracking-Templates/blob/main/Packages/adjerry91.vrcft.templates/CHANGELOG.md)
