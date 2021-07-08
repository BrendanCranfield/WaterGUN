using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(ParticleSystemShellEject))]
    public class ParticleSystemShellEjectEditor : BaseFirearmModuleBehaviourEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            var delayTypeProp = serializedObject.FindProperty("m_DelayType");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ParticleSystems"));
            EditorGUILayout.PropertyField(delayTypeProp);
            if (delayTypeProp.enumValueIndex == 1)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Delay"));
                --EditorGUI.indentLevel;
            }
        }
    }
}