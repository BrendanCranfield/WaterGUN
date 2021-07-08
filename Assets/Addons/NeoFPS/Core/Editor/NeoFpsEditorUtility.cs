using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace NeoFPSEditor
{
    public static class NeoFpsEditorUtility
    {
        public static bool CheckAnimatorKeyValid(Animator animator, string key, AnimatorControllerParameterType parameterType)
        {
            // Do nothing in play mode
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return true;

            // Empty keys are always fine
            if (string.IsNullOrEmpty(key))
                return true;

            // Check animator is set
            if (animator == false)
                return false;

            SerializedObject animatorSO = new SerializedObject(animator);
            var controller = animatorSO.FindProperty("m_Controller").objectReferenceValue as AnimatorController;
            if (controller == null)
                return false;

            // Check through parameters for correct name and type
            var parameters = controller.parameters;
            for (int i = 0; i < parameters.Length; ++i)
            {
                var p = parameters[i];
                if (p.type == parameterType && p.name == key)
                    return true;
            }

            // None found
            return false;
        }

        public static Texture2D GetColourTexture(Color c)
        {
            c = c.linear;
            var tex = new Texture2D(4, 4);
            for (int i = 0; i < 16; ++i)
            {
                int row = i / 4;
                int column = i - (row * 4);
                tex.SetPixel(row, column, c);
            }
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }

        public static void ClampFloatProperty(SerializedProperty prop, float min, float max)
        {
            prop.floatValue = Mathf.Clamp(prop.floatValue, min, max);
        }

        public static void ClampMinFloatProperty(SerializedProperty prop, float min)
        {
            if (prop.floatValue < min)
                prop.floatValue = min;
        }

        public static void ClampMaxFloatProperty(SerializedProperty prop, float max)
        {
            if (prop.floatValue > max)
                prop.floatValue = max;
        }

        public static void ClampIntProperty(SerializedProperty prop, int min, int max)
        {
            prop.intValue = Mathf.Clamp(prop.intValue, min, max);
        }

        public static void ClampMinIntProperty(SerializedProperty prop, int min)
        {
            if (prop.intValue < min)
                prop.intValue = min;
        }

        public static void ClampMaxIntProperty(SerializedProperty prop, int max)
        {
            if (prop.intValue > max)
                prop.intValue = max;
        }
    }
}
