using Kurisu.NGDS.AI;
using UnityEditor;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    [CustomEditor(typeof(AIEntry))]
    public class AIEntryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var typeProperty = serializedObject.FindProperty("llmType");
            var settingProperty = serializedObject.FindProperty("setting");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(typeProperty, new GUIContent("LLM Model Type"));
            EditorGUILayout.PropertyField(settingProperty);
            if (settingProperty.objectReferenceValue == null)
            {
                if (GUILayout.Button("Use Editor Setting"))
                {
                    settingProperty.objectReferenceValue = NextGenDialogueSetting.GetOrCreateSettings().AITurboSetting;
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
