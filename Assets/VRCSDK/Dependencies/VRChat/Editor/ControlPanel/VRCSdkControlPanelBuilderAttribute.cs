using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace VRC.SDKBase.Editor
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [MeansImplicitUse]
    public class VRCSdkControlPanelBuilderAttribute : Attribute
    {
        public Type Type { get; }
        public VRCSdkControlPanelBuilderAttribute(Type type)
        {
            Type = type;
        }
    }
}
