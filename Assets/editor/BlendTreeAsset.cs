using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;

public class CreateBlendtreeAsset : MonoBehaviour
{

    [MenuItem("AnimTools/GameObject/Asset from Blendtree")]
    static void CreateBlendtree()
    {
        string path = "Assets/";

        string currentPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (currentPath != null)
        {
            path = currentPath;
        }

        BlendTree BT = Selection.activeObject as BlendTree;

        BlendTree BTcopy = Instantiate<BlendTree>(BT);

        AssetDatabase.CreateAsset(BTcopy, AssetDatabase.GenerateUniqueAssetPath(path + "_" + BT.name + ".asset"));
    }
}