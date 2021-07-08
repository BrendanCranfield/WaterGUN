using UnityEngine;
using UnityEditor;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(FpsGraphicsSettings), true)]
    public class FpsGraphicsSettingsEditor : SettingsContextEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.HelpBox("Resolution, fullscreen and vsync settings are initialised on first run based on the Unity player settings.", MessageType.None);
            EditorGUILayout.Space();

            base.OnInspectorGUIInternal();
        }
    }
}
