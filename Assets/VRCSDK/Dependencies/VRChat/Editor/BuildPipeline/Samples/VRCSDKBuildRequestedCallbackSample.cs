#if VRC_SDK_PIPELINE_SAMPLES

using UnityEditor;

namespace VRC.SDKBase.Editor.BuildPipeline.Samples
{
    public class VRCSDKBuildRequestedCallbackSample : IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => 0;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            return EditorUtility.DisplayDialog("Build Confirmation", "Are you sure you want to build?", "Yes", "Not Yes");
        }
    }
}

#endif
