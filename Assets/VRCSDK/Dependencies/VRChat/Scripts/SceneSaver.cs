using System.Collections;
using UnityEngine;

public class SceneSaver
{
    static public void SaveScene()
    {
#if UNITY_EDITOR
			var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene);
			
			UnityEditor.EditorApplication.isPaused = false;
			UnityEditor.EditorApplication.isPlaying = false;

            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(activeScene.name);
#endif
    }
}
