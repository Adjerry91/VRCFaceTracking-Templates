// This code was adapted from: https://github.com/Joshuarox100/Fix-Your-Animators/blob/main/FixYourAnimators.cs
// MIT License
// 
// Copyright (c) 2021 Joshuarox100
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace VRC.SDKBase.Editor
{
    public static class FixAnimatorControllers
    {
        private static readonly HashSet<Type> types = new HashSet<Type>
        {
            typeof(AnimatorState),
            typeof(AnimatorStateMachine),
            typeof(StateMachineBehaviour),
            typeof(AnimatorStateTransition),
            typeof(AnimatorTransition),
            typeof(BlendTree)
        };

        [InitializeOnLoadMethod]
        private static void RegisterDelegates()
        {
            Selection.selectionChanged += AutoFixHideFlags;
            EditorApplication.quitting += UnregisterDelegates;
        }

        public static void UnregisterDelegates()
        {
            Selection.selectionChanged -= AutoFixHideFlags;
            EditorApplication.quitting -= UnregisterDelegates;
        }

        // Automatically corrects HideFlags for objects with types included in 'types' when trying to inspect them.
        public static void AutoFixHideFlags()
        {
            bool dirty = false;
            foreach(UnityEngine.Object selection in Selection.objects)
            {
                if(selection == null)
                {
                    continue;
                }

                if(selection.hideFlags != (HideFlags.HideInHierarchy | HideFlags.HideInInspector) || !types.Contains(selection.GetType()))
                {
                    continue;
                }

                if(!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(selection)))
                {
                    EditorUtility.SetDirty(selection);
                }

                selection.hideFlags = HideFlags.HideInHierarchy;
                dirty = true;
            }

            if(dirty)
            {
                Selection.selectionChanged();
            }
        }
    }
}
