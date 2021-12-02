using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

public class VRCSdk3Analysis
{
    static Assembly GetAssemblyByName(string name)
    {
        return AppDomain.CurrentDomain.GetAssemblies().
               SingleOrDefault(assembly => assembly.GetName().Name == name);
    }

    static List<Component> GetSceneComponentsFromAssembly( Assembly assembly )
    {
        if (assembly == null)
            return new List<Component>();

        Type[] types = assembly.GetTypes();

        List<Component> present = new List<Component>();
        foreach (var type in types )
        {
            if (!type.IsSubclassOf(typeof(MonoBehaviour)))
                continue;

            var monos = VRC.Tools.FindSceneObjectsOfTypeAll(type);
            present.AddRange(monos);
        }
        return present;
    }

    public enum SdkVersion
    {
        VRCSDK2,
        VRCSDK3
    };

    public static List<Component> GetSDKInScene(SdkVersion version)
    {
        var assembly = GetAssemblyByName( version.ToString() );
        return GetSceneComponentsFromAssembly(assembly);
    }

    public static bool IsSdkDllActive(SdkVersion version)
    {
        string assembly = version.ToString();
        PluginImporter importer = GetImporterForAssemblyString(assembly);
        if (importer == false)
        {
            //Handle Avatar Dll Split
            importer = GetImporterForAssemblyString(assembly + "A");
            if (importer == false)
                return false;
        }

        return importer.GetCompatibleWithAnyPlatform();
    }

    public static PluginImporter GetImporterForAssemblyString(string assembly)
    {
#if VRCUPM
        return AssetImporter.GetAtPath($"Packages/com.vrchat.{assembly.ToLower()}/Runtime/VRCSDK/Plugins/{assembly}.dll") as PluginImporter;
#else
        return AssetImporter.GetAtPath($"Assets/VRCSDK/Plugins/{assembly}.dll") as PluginImporter;
#endif
    }
}
