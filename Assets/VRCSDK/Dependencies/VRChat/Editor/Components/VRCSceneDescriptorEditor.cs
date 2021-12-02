#if VRC_SDK_VRCSDK2
using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(VRCSDK2.VRC_SceneDescriptor))]
public class VRCSceneDescriptorEditor : Editor
{
    VRCSDK2.VRC_SceneDescriptor sceneDescriptor;
    VRC.Core.PipelineManager pipelineManager;

    public override void OnInspectorGUI()
    {
        if(sceneDescriptor == null)
            sceneDescriptor = (VRCSDK2.VRC_SceneDescriptor)target;

        if(pipelineManager == null)
        {
            pipelineManager = sceneDescriptor.GetComponent<VRC.Core.PipelineManager>();
            if(pipelineManager == null)
                sceneDescriptor.gameObject.AddComponent<VRC.Core.PipelineManager>();
        }

        DrawDefaultInspector();


    }
}
#endif
