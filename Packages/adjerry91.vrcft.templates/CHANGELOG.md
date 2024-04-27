# Changelog

## [5.3.7] - 2024-04-27
- Fix viseme save state between worlds
- Change eyebrow tracking to EyeTrackingActive

## [5.3.6] - 2024-04-26
- Fix tracking state when loading new worlds

## [5.3.5] - 2024-04-26
- Fix SRanipal debug panel animations
- Change SRanipal Lip Funnel Logic
- Change Tracking State Logic - Azuki
- Remove transition to self for eye tracking state

## [5.3.4] - 2024-02-28
- Bug with importing getting hung up by Blender files

## [5.3.3] - 2024-02-26
- Eye Dilation emulation is only on when toggle is off.
- Add eye dilation emulation to frown
- Add template version number to debug window
- Fix FBX import warnings on blendshape normals
- Change texture import size
- Revert Jaw Open and Mouth Closed logic as it breaks SRanipal tracking headsets. 

## [5.3.2] - 2024-02-22
Changes made to ARKit and UE templates
- Modify JawForward JawOpen limits, decrease to 30%
- Add JawForward LipFunnel limits, increase to 30%
- Modify JawX MouthX limits, reduce to 50%
- Modify MouthFrown Left&Right MouthX limits, reduce to 30%
- Add LipSuckLower MouthClosed limit, increase to 30%
- Add LipSuckUpper MouthClosed limit, increase to 30%
- Limit MouthRaiserUpper from MouthClosed as MouthRaiserUpper is undesirable as breaks MouthApeShape
- Limit Brow Sad emulation from MouthClosed
- Remove MouthUpperUp Left&Right Limit with MouthClosed
- Change logic on JawOpen and MouthClosed; converted to 2D blend

## [5.3.1] - 2024-02-16
- Add Unity 2022 dependency

## [5.3.0] - 2024-02-11
- Face tracking debug prefabs. Window to spawn to see face tracking blendshapes and raw face tracking values.

## [5.2.1] - 2024-02-10
- Add limits to MouthClosed. MouthClosed should never be larger value than JawOpen.
- Remove limits to CheekSuckLeft and CheekSuckRight
- Fix eye open state from 0.8 to 0.75. This has been issue as did not realize that neutral position changed with Unified Expressions for EyeLid parameter.
- Add EyeDilation breakout

## [5.2.0] - 2024-01-27
- Modify logic for MouthOpen and Closed
- Add MouthUpperUpLeft and MouthUpperUpRight limits
- Remove LipSuckLower and LipSuckUpper limits
- Add advance face tracking prefabs for advance options
- Add tongue helper to open the mouth slightly when tongue is sticking out to limit clipping
- Add info section on menu for references and common issues

## [5.1.5] - 2024-01-04
- Fix facial expressions state logic change looping

## [5.1.4] - 2024-01-03
- Cleanup sub-assets
- Facial expressions state logic change

## [5.1.1] - 2023-12-20
- Fix viseme toggle

## [5.1.0] - 2023-12-20
- Add MouthRaiserUpper shape to tracking
- Disabled MouthPress from tracking (Disabled Sync); Quest Pro does not track well
- Split MouthTighener to Left and Right; Does not track well with combined so decided to split the tracking.
- Revert tracking state changes, causing issues with normal avatar configurations
- Remove Editor and Runtime folders

## [5.0.10] - 2023-11-20
VRCFury recent made change that merges all the tracking states in the avatar. This is good directions as state controls are hard to manage if they are on different layers. The logic used does not work correctly with the merging with VRCFury so logic was changed to be compatible.

## [5.0.7] - 2023-11-05
- Fix Left Eye Lid Smile Control
- Left eye lid smile control used the incorrect animation

## [5.0.6] - 2023-11-05
- Eye Constrict when widing eyes
- Fix Tongue_DownRight_Morph for UE blendshapes
- Update logic states for eye lid controls
- Remove MouthUpperUp and MouthLowerDown limits on jaw open

## [5.0.5] - 2023-10-06
- Add MouthClosed limit to LipSuckUpper, CheekSuckLeft and CheckSuckRight

## [5.0.4] - 2023-10-04
- Updated Guide to VCC
- VCC support allows better version control in projects with updates to face tracking
- Unified Expressions fix to Eye Dilation/Constrict animation
- VRCFury is not required with VRCFT- Jerry's Templates package

## [5.0.3] - 2023-09-20 Pre-release
- VCC Support - Beta
- ⚠️ Use caution when using with old face tracking setups.

## [5.0.2] - 2023-09-20 Pre-release
- VCC Support
- Do not use, missing icons folder

## [5.0.1] - 2023-09-19 Pre-release
- VCC Support
- Do not use, will delete everything in Assets/VRCFaceTracking folder

## [5.0.0] - 2023-09-19 Pre-release
- VCC Support
- Do not use, asset ID linking broken.