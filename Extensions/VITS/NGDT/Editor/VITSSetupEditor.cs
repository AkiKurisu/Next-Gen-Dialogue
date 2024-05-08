using Kurisu.NGDS.VITS;
using Kurisu.NGDT.Editor;
using UnityEditor;
using UnityEngine;
namespace Kurisu.NGDT.VITS.Editor
{
    [CustomEditor(typeof(VITSSetup))]
    public class VITSSetupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var settingProperty = serializedObject.FindProperty("setting");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("llmType"), new GUIContent("LLM Model Type"));
            EditorGUILayout.PropertyField(settingProperty);
            if (settingProperty.objectReferenceValue == null)
            {
                if (GUILayout.Button("Use Editor Setting"))
                {
                    settingProperty.objectReferenceValue = NextGenDialogueSetting.GetOrCreateSettings().AITurboSetting;
                }
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("audioSource"), new GUIContent("VITS Audio Source"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            DrawAudio();
        }
        private void DrawAudio()
        {
            var clip = ((VITSSetup)target).AudioClipCache;
            EditorGUILayout.BeginVertical();
            if (clip != null)
            {
                GUILayout.Label($"Audio Clip Cached : {clip.name}");
                GUILayout.Label($"Length : {clip.length}");
            }
            else
            {
                GUILayout.Label($"No Audio Clip Cached");
            }
            GUILayout.EndVertical();
            GUI.enabled = clip != null;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Preview", GUILayout.MinHeight(25)))
            {
                AudioUtil.PlayClip(clip);
            }
            if (GUILayout.Button("Stop", GUILayout.MinHeight(25)))
            {
                AudioUtil.StopClip(clip);
            }
            GUILayout.EndHorizontal();
            var orgColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(140 / 255f, 160 / 255f, 250 / 255f);
            if (GUILayout.Button("Save Audio", GUILayout.MinHeight(25)))
            {
                Save(clip);
            }
            GUI.backgroundColor = orgColor;
            GUI.enabled = true;
        }
        private void Save(AudioClip audioClip)
        {
            string folderPath = EditorPrefs.GetString(AudioUtil.PrefKey, Application.dataPath);
            string path = EditorUtility.OpenFolderPanel("Select save path", folderPath, "");
            if (string.IsNullOrEmpty(path)) return;
            EditorPrefs.SetString(AudioUtil.PrefKey, path);
            string outPutPath = $"{path}/{audioClip.name}";
            WavUtil.Save(outPutPath, audioClip);
            Debug.Log($"Audio saved succeed! Audio path:{outPutPath}");
        }
    }
}