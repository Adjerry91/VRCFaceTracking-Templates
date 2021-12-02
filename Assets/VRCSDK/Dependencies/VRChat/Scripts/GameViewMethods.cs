using System;
using System.Reflection;

namespace VRC. SDKBase
{
    public static class GameViewMethods
    {
        private static readonly Type GameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        private static readonly Type PlayModeViewType = System.Type.GetType("UnityEditor.PlayModeView, UnityEditor");

        public static int GetSelectedSizeIndex()
        {
            return (int) GetSelectedSizeProperty().GetValue(GetPlayModeViewObject());
        }

        public static void SetSelectedSizeIndex(int value)
        {
            var selectedSizeIndexProp = GetSelectedSizeProperty();
            selectedSizeIndexProp.SetValue(GetPlayModeViewObject(), value, null);
        }
        
        // Set it to something else just to force a refresh
        public static void ResizeGameView()
        {
            int current = GetSelectedSizeIndex();
            SetSelectedSizeIndex(current == 0 ? 1 : 0);
        }

        private static PropertyInfo GetSelectedSizeProperty()
        {
            return GameViewType.GetProperty("selectedSizeIndex",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        private static Object GetPlayModeViewObject()
        {
            MethodInfo GetMainPlayModeView = PlayModeViewType.GetMethod("GetMainPlayModeView",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return GetMainPlayModeView.Invoke(null, null);
        }

        public static void Repaint()
        {
            MethodInfo RepaintAll = PlayModeViewType.GetMethod("RepaintAll", BindingFlags.NonPublic | BindingFlags.Static);
            RepaintAll.Invoke(GetPlayModeViewObject(), null);
        }

    }
}