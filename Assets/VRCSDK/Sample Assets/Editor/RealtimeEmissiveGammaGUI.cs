using UnityEngine;
using UnityEditor;

public class RealtimeEmissiveGammaGUI : ShaderGUI 
{
    public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI (materialEditor, properties);

        Material mtl = materialEditor.target as Material;
        mtl.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

    }
}
