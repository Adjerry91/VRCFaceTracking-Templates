using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;

namespace VRC.SDKBase.Editor.ShaderStripping
{
    public class StripPostProcessing : IPreprocessShaders
    {
        public int callbackOrder => 0;

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
            string shaderName = shader.name;
            if(string.IsNullOrEmpty(shaderName) || !shaderName.Contains("PostProcessing"))
            {
                return;
            }

            data.Clear();
        }
    }
}
