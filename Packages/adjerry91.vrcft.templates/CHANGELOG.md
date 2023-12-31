# Changelog

## [5.1.2] - 2024-01-03
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