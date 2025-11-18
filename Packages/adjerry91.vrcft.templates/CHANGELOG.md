# Changelog

## [7.0.4] - 2025-11-18
- Corrections to rotation proxy animations for sensitivity - PR22 Antarcticsiclepop
- Add menu icons for sensitivity - PR22 Antarcticsiclepop
- Remove unused second keyframes in scaler animations for smoothing

## [7.0.3] - 2025-10-18
- Fix eye dilation emulation not working on lip tracking only

## [7.0.2] - 2025-10-13
- Fix parameters for MA prefabs

## [7.0.1] - 2025-10-11
- Fix TongueRoll1 missing from Parameters - Face Tracking - UE Blendshapes
- Add menu icons for new toggles

## [7.0.0] - 2025-10-08
- Add FaceTrackingEmulation toggle in settings menu (Default On). Turning emulation toggle off will disable any emulation of other face tracking blend shapes with other face tracking parameters. 
- Add FaceTrackingLimits toggle in setting menu (Default On). Turning limits toggle off will remove any limits from OSC values. 
- Remove MouthUpperUpLeft and MouthUpperUpRight emulation in favor of using native face tracking.
- Seperate MouthUpperUp to MouthUpperUpLeft and MouthUpperUpRight OSC
- Change MouthUpperUpLeft and MouthUpperUpRight to binary parameters
- Change MouthX from 4 bit binary to 3 bit binary
- Remove MouthDimple OSC and add to emulations with MouthSmileLeft and MouthSmileRight
- Add sensitivity control options in sub prefabs. This is not default included with the template prefabs as this addes fair amount of floats to an avatar.
- Add Eyelid Sync toggle
- Add TongueRoll to UE templates
- Viseme state saves

WILL BREAK AVATARS FOR EMBEDDED TEMPLATES

## [6.8.2] - 2025-09-08
- Remove Blender files as it causing failures to import

## [6.8.1] - 2025-07-20
- Add LipFunnel limits for TongueOut to help against false tongue trigger from Vive Face Tracker
- Change labeling on the debug menu to make more clear of Blendshapes (Active) and OSC (Raw)
- Add Head Mask to Additive - Eye Tracking - Eye Rotations controller to fix issues with other additive layer conflicts
- Fix UE Face Tracking Debug RawTongueUp blendshape breaking 

## [6.8.0] - 2025-05-29
- Add Modular Avatar prefabs
- Remove EyeTrackingActive requirement for brow emulation

## [6.7.2] - 2025-04-17
- Fix MouthLowerDown threshold on the UE Blendshapes TongueSteps animator

## [6.7.1] - 2025-04-11
- Reduced the sensitivity of EyeSquint

## [6.7.0] - 2025-03-12
- Lip Funnel should not have MouthUpperUp, MouthLowerDown, and LipPucker. These Shapes cause many conflicts and are not desirable.
  - Remove LipPucker Lip Funnel limits
  - Add Lip Funnel to Mouth Upper Up Limits
  - Add Lip Funnel to Mouth Lower Down Limits
  - Add Lip Funnel to Lip Pucker Limits
- Modify the Eye Dilation thresholds to reach maximum dilation and constrict
- Weird shape with MouthUpperUpLeft/Right with Mouth X. Reduce MouthX on MouthUpperUpLeft and MouthUpperUpRight by 75%
- Remove PDF guide from template

## [6.6.0] - 2025-02-19
- Update smoothing math
	- Changes in smoothing math 
	- Smoothing math is now > smoothing factor = frametime * scaler + mod offset

## [6.5.3] - 2025-02-08
- Remove JawFoward limits
- Remove ToungeOut limits
- Simplify eye lid tracking blend tree
	- Remove MouthSmile from triggering squint

## [6.5.2] - 2025-02-08
- Add Eye Closed and Brow Corrective blend shapes to Unified Expressions template
	- Add EyeClosedBrowDownLeftCorrective
	- Add EyeClosedBrowDownRightCorrective
	- Add EyeClosedBrowInnerUpCorrective
	- Add EyeClosedBrowOuterUpLeftCorrective
	- Add EyeClosedBrowOuterUpRightCorrective
- Add NoseSneer to brow down emulation

## [6.5.1] - 2025-02-02
- Change smoothing cutoff to 40ms+ CPU frame time

## [6.5.0] - 2024-12-23
- Add smoothing cutoff, disables smoothing for 30ms+ CPU frame time
- Slight modification to blend tree for EyeSquint and EyeClosed
- EyeClosedSquintCorrectiveLeft and EyeCloseQuintCorrectiveRight for EyeClosed + EyeSquint. This optional blendshape to be used to fix clipping issue with the combination of the two blendshapes.

## [6.4.2] - 2024-10-21
- Remove Gesture Disable Prefab. Causes issue with other avatar components. Custom setup will be required to setup FacialExpressionDisabled.

## [6.4.1] - 2024-10-10
- Add missing VRCFury Gesture Disable prefab to UE Blendshape TongueSteps 

## [6.4.0] - 2024-10-07
- Add VRCFury Gesture Disable prefab

## [6.3.3] - 2024-09-06
- Change debug prefab constraints to work with non-standard bone rotations causing the debug menu to spawn in undesirable locations.

## [6.3.2] - 2024-09-03
- VRC contraints for debug
- Fix viseme state
- Change eye tracking state back to old logic

## [6.3.1] - 2024-08-25
- Add eye tracking state back to FX controllers for blend shape eye tracking only setups.
- Update tracking state transitions

## [6.3.0] - 2024-08-22
- Optimized FX and Additive Controllers - Azuki Pull Request #10
	- One layer face tracking
	- Local Smoothing Adjust
	- Remote Smoothing Test
	- Performance upgrade 
- Advance prefabs has been moved to Legacy. Difficult to maintain and support. No more updates.
- Seperate CheekSquint from MouthSmile and reduce the influence by 50% and update to debug panel accordingly.

## [6.2.1] - 2024-07-15
- Update to Unity 2022.3.22f1
- Add new UE prefab to use TongueOutStep1 and TongueOutStep2. This prefab is to be used for avatars with tongue placed low in the mouth.
- Split out eye rotation from the main face tracking. Some avatar do not use eye rotation for eye tracking.
- Add Eye Lid States to ARkit
- Change logic for Eye Squint in Blendtrees

## [6.1.1] - 2024-06-10
- Fix visemes not saving previous saved states

## [6.1.0] - 2024-05-30
- Add EyeTrackingActive and LipTrackingActive to enable disable smoothing
- Remote and local smoothing is now in one blendtree
- Binary decode only animates for remote users
- Fix SRanipal and ARkit debug panel for v6

## [6.0.2] - 2024-05-23
- Fix broken eye rotation smoothing

## [6.0.1] - 2024-05-23
- Fix bugs with frame time smoothing for remote users
- Add disable smoothing with face tracking toggles

## [6.0.0] - 2024-04-30
- OSC Float local
- Frame time smoothing
- Frame time debug

WILL BREAK AVATARS FOR EMBEDDED TEMPLATES

## [5.3.7] - 2024-04-27
- Fix viseme save state between worlds
- Change eyebrow tracking from LipTrackingActive to EyeTrackingActive

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
