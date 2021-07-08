using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS;

namespace NeoFPSEditor
{
	[CustomEditor (typeof (ScenePoolHandler))]
	public class ScenePoolHandlerEditor : BasePoolInfoEditor
	{
        protected override SerializedProperty GetPoolInfoArrayProperty()
        {
            return serializedObject.FindProperty("m_ScenePools");
        }

        protected override int GetDefaultPoolSize()
        {
            return 100;
        }

        public override void OnInspectorGUI ()
		{
			serializedObject.Update ();

            EditorGUILayout.LabelField("Starting Pools", EditorStyles.boldLabel);
            DoLayoutPoolInfo();

			serializedObject.ApplyModifiedProperties ();
		}
	}
}