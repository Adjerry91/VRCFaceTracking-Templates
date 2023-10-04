# VRCFaceTracking-Templates

VRChat Face Tracking Unity templates to be used with [VRCFaceTracking](https://github.com/benaclejames/VRCFaceTracking)

## Prerequisites

* [Latest VRCFaceTracking Release](https://github.com/benaclejames/VRCFaceTracking/releases/latest)
* Avatar with [SRanipal](https://docs.vrcft.io/docs/v4.0/category/intermediate), [ARKit (Perfect Sync)](https://arkit-face-blendshapes.com/), or [Unified Expressions](https://docs.vrcft.io/docs/tutorial-avatars/tutorial-avatars-extras/unified-blendshapes) face tracking shapekeys
* Face tracking animations are pointed to the ```Body``` skinned mesh render by default. If face tracking shapes are on a different skinned mesh render, you will need to change VRCFury component to rewrite animations clips prefix. For example if face tracking shapes are on ```Face``` mesh you will need to rewrite ```Body``` to ```Face```

## Setup 

1. Add/Import [VRCFury](https://vrcfury.com/download)
2. Add/Import VRCFT Jerry's Templates from [Jerry's VRCFT Templates Listing](https://adjerry91.github.io/VRCFaceTracking-Templates/)
3. Add face tracking prefab to your avatar located in ```Packages/VRCFT - Jerry's Templates/Prefabs```
   * Uses the corresponding face tracking shapes prefab, if not sure look this [Face Tracking Spreadsheet](https://docs.google.com/spreadsheets/d/118jo960co3Mgw8eREFVBsaJ7z0GtKNr52IB4Bz99VTA/edit?usp=sharing)

Detailed setup PDF guide included in the ```Packages/VRCFT - Jerry's Tempates``` folder

## Support

Post in ```#template-help``` for advance support on [Jerry's Face Tracking Discord](https://discord.gg/yQtTsVSqx8)

## Face Tracking Animation Tips:

* Modification to the thresholds may be needed for some animation sensitivity for different faces
* Driver, binary, and smooth layer require write defaults ON


