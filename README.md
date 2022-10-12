# VRCFaceTracking-Templates

VRC Face Tracking Unity templates to be used with [VRCFaceTracking](https://github.com/benaclejames/VRCFaceTracking)

## Prerequisites

* [Vive SRanipal Runtime](https://developer.vive.com/us/support/sdk/category_howto/how-to-update-vive-eye-tracking-runtime.html)
* [Latest VRCFaceTracking Release](https://github.com/benaclejames/VRCFaceTracking/releases/latest)
* [Avatar with SRanipal Blend Shapes](https://github.com/benaclejames/VRCFaceTracking/wiki/Blend-Shapes-Setup) - Case Sensitive and on skinned mesh named "Body"

## Setup 

Import the latest template from [Releases](https://github.com/Adjerry91/VRCFaceTracking-Templates/releases/latest)

Setup PDF guide included in the *VRCFaceTracking* folder

## Face Tracking Animation Tips:

* Modification to the thresholds may be needed for some animation sensitivity for different faces
* Template is write defaults off and on compatible. To ensure best animator stability keep all animations the same type. Write defaults off is the most stable.
*	:warning: There is bug on older version of the template that causes animator crashes, severe lag with the avatar to you and others. This is caused when there is many blend trees referencing the _Do_Nothing animation, the newest template v3.1.4 and higher. 

