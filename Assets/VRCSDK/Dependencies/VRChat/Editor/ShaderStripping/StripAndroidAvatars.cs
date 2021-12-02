using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

public class StripAndroidAvatars : IPreprocessShaders, IVRCSDKBuildRequestedCallback, IPostprocessBuildWithReport
{
    private static VRCSDKRequestedBuildType? _buildType = null;

    public int callbackOrder => 0;

    public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
    {
        _buildType = requestedBuildType;
        return true;
    }

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        if(_buildType != VRCSDKRequestedBuildType.Avatar)
        {
            return;
        }

        if(EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            return;
        }

        data.Clear();
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        _buildType = null;
    }
}
