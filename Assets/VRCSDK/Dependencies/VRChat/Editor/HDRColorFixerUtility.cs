using UnityEditor;
using UnityEngine;

public static class HDRColorFixerUtility
{
    [MenuItem("VRChat SDK/Utilities/Convert HDR Material Colors/To Linear", false, 995)]
    private static void ConvertToLinear()
    {
        if (!TryConvert())
            EditorUtility.DisplayDialog("Error", "Please select one or more HDR materials to convert.", "OK");
    }

    [MenuItem("VRChat SDK/Utilities/Convert HDR Material Colors/To Gamma", false, 996)]
    private static void ConvertToGamma()
    {
        if (!TryConvert(false))
            EditorUtility.DisplayDialog("Error", "Please select one or more HDR materials to convert.", "OK");
    }

    static bool TryConvert(bool toLinear = true)
    {
        Object[] selection = Selection.objects;
        if (selection == null)
            return false;

        int matCount = 0;
        int colorCount = 0;
        foreach (Material mat in selection)
        {
            matCount++;
            MaterialProperty[] props = MaterialEditor.GetMaterialProperties(new Material[] { mat });
            if ((props == null) || (props.Length == 0))
                return false;

            foreach (MaterialProperty m in props)
            {
                if (m.flags == MaterialProperty.PropFlags.HDR)
                {
                    //Color prev = m.colorValue;
                    m.colorValue = (toLinear ? m.colorValue.linear : m.colorValue.gamma);
                    colorCount++;
                    //Debug.Log("prev: " + prev + ", current: " + m.colorValue);
                }
            }
        }

        if (colorCount == 0)
            EditorUtility.DisplayDialog("Result", "Found no HDR Color properties in the selected material" + (matCount > 1 ? "s." : "."), "OK");
        else
            EditorUtility.DisplayDialog("Result", "Converted " + colorCount + " HDR Color propert"+ (colorCount > 1 ? "ies" : "y") + " in "+matCount+" material"+(matCount > 1 ? "s":"") + " to " + (toLinear ? "Linear" : "Gamma") + " color space.", "OK");

        return true;
    }
    
    [MenuItem("VRChat SDK/Utilities/Convert HDR Material Colors/To Linear", true, 995)]
    [MenuItem("VRChat SDK/Utilities/Convert HDR Material Colors/To Gamma", true, 996)]
    private static bool CheckSelection()
    {
        if (Selection.objects.Length == 0)
            return false;
        bool allMaterials = true;
        foreach (Object obj in Selection.objects)
        {
            if (obj.GetType() != typeof(Material))
            {
                allMaterials = false;
                break;
            }
        }
        return allMaterials;
    }

}
