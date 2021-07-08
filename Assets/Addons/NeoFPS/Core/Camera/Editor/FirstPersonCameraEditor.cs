using UnityEngine;
using UnityEditor;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(FirstPersonCamera))]
    public class FirstPersonCameraEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Camera"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AudioListener"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimTransform"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviousCameraAction"));

            // FoV
            var fovProp = serializedObject.FindProperty("m_DefaultFoV");
            EditorGUILayout.PropertyField(fovProp, new GUIContent("Default FoV", fovProp.tooltip));

            float verticalOld = fovProp.floatValue / 0.5625f;
            float vertical = Mathf.Clamp(EditorGUILayout.DelayedFloatField("Horizontal 16:9", verticalOld), 40f, 160f);
            if (!Mathf.Approximately(vertical, verticalOld))
                fovProp.floatValue = vertical * 0.5625f;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OffsetTransform"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimPositionEffectMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimRotationEffectMultiplier"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
