using UnityEditor;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class EditorHandling
{
    static EditorHandling()
    {
        UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += SceneOpenedCallback;
    }

    static void SceneOpenedCallback( Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
    {
        // refresh window when scene is opened to display content images correctly
        if (null != VRCSdkControlPanel.window) VRCSdkControlPanel.window.Reset();
    }
}
