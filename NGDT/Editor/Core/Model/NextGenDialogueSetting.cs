using System;
using Kurisu.NGDS.AI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [Serializable]
    internal class GraphEditorSetting
    {
        [AkiGroupSelector, Tooltip("Display type, filter AkiGroup according to this list, nodes without category will always be displayed")]
        public string[] ShowGroups = new string[0];
        [AkiGroupSelector, Tooltip("The type that is not displayed, filter the AkiGroup according to this list, and the nodes without categories will always be displayed")]
        public string[] NotShowGroups = new string[0];
        [Tooltip("You can customize the style of the Graph view")]
        public StyleSheet graphStyleSheet;
        [Tooltip("You can customize the style of the Inspector inspector")]
        public StyleSheet inspectorStyleSheet;
        [Tooltip("You can customize the style of Node nodes")]
        public StyleSheet nodeStyleSheet;
    }
    public class NextGenDialogueSetting : ScriptableObject
    {
        private const string k_NDGTSettingsPath = "Assets/Next Gen Dialogue Setting.asset";
        private const string k_AITurboSettingsPath = "Assets/AI Turbo Setting.asset";
        private const string GraphFallBackPath = "NGDT/Graph";
        private const string InspectorFallBackPath = "NGDT/Inspector";
        private const string NodeFallBackPath = "NGDT/Node";

        [SerializeField]
        private GraphEditorSetting graphEditorSetting;
        [SerializeField]
        private AITurboSetting aiTurboSetting;
        public AITurboSetting AITurboSetting => aiTurboSetting;
        [SerializeField, HideInInspector]
        private bool autoSave;
        [SerializeField, HideInInspector]
        private string lastPath;
        /// <summary>
        /// Cache last open folder path in editor
        /// </summary>
        /// <value></value>
        public string LastPath
        {
            get => lastPath;
            set => lastPath = value;

        }
        /// <summary>
        /// Cache user auto save option value
        /// </summary>
        /// <value></value>
        public bool AutoSave
        {
            get => autoSave;
            set => autoSave = value;
        }
        public static StyleSheet GetGraphStyle()
        {
            var setting = GetOrCreateSettings();
            return setting.graphEditorSetting.graphStyleSheet ?? Resources.Load<StyleSheet>(GraphFallBackPath);
        }
        public static StyleSheet GetInspectorStyle()
        {
            var setting = GetOrCreateSettings();
            return setting.graphEditorSetting.inspectorStyleSheet ?? Resources.Load<StyleSheet>(InspectorFallBackPath);
        }
        public static StyleSheet GetNodeStyle()
        {
            var setting = GetOrCreateSettings();
            return setting.graphEditorSetting.nodeStyleSheet ?? Resources.Load<StyleSheet>(NodeFallBackPath);
        }
        public static (string[], string[]) GetMask()
        {
            var setting = GetOrCreateSettings();
            var editorSetting = setting.graphEditorSetting;
            if (editorSetting == null) return (null, null);
            return (editorSetting.ShowGroups, editorSetting.NotShowGroups);
        }
        public static NextGenDialogueSetting GetOrCreateSettings()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(NextGenDialogueSetting)}");
            NextGenDialogueSetting setting = null;
            if (guids.Length == 0)
            {
                setting = CreateInstance<NextGenDialogueSetting>();
                Debug.Log($"Next Gen Dialogue Setting saving path : {k_NDGTSettingsPath}");
                AssetDatabase.CreateAsset(setting, k_NDGTSettingsPath);
                AssetDatabase.SaveAssets();
            }
            else setting = AssetDatabase.LoadAssetAtPath<NextGenDialogueSetting>(AssetDatabase.GUIDToAssetPath(guids[0]));
            if (setting.aiTurboSetting == null)
            {
                setting.aiTurboSetting = GetOrCreateAITurboSetting();
            }
            return setting;
        }
        private static AITurboSetting GetOrCreateAITurboSetting()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(AITurboSetting)}");
            AITurboSetting setting;
            if (guids.Length == 0)
            {
                setting = CreateInstance<AITurboSetting>();
                Debug.Log($"AI Turbo Setting saving path : {k_AITurboSettingsPath}");
                AssetDatabase.CreateAsset(setting, k_AITurboSettingsPath);
                AssetDatabase.SaveAssets();
            }
            else setting = AssetDatabase.LoadAssetAtPath<AITurboSetting>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return setting;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }

    internal class NGDTSettingsProvider : SettingsProvider
    {
        private SerializedObject m_Settings;
        private class Styles
        {
            public static GUIContent GraphEditorSettingStyle = new("Graph Editor Setting");
            public static GUIContent AITurboSettingStyle = new("AI Turbo Setting");
            public static GUIContent EnableReflectionStyle = new("Enable Runtime Reflection",
                     "Set this on to map shared variables on awake automatically." +
                     " However, reflection may decrease your loading speed" +
                     " since shared variables will be mapped when dialogue tree is first loaded"
                     );
        }
        public NGDTSettingsProvider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope) { }
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            m_Settings = NextGenDialogueSetting.GetSerializedSettings();
        }
        public override void OnGUI(string searchContext)
        {
            GUILayout.BeginVertical("Editor Settings", GUI.skin.box);
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.PropertyField(m_Settings.FindProperty("graphEditorSetting"), Styles.GraphEditorSettingStyle);
            GUILayout.EndVertical();
            GUILayout.BeginVertical("Runtime Settings", GUI.skin.box);
            var turboSetting = m_Settings.FindProperty("aiTurboSetting");
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.PropertyField(turboSetting, Styles.AITurboSettingStyle);
            if (turboSetting.objectReferenceValue != null)
            {
                var obj = new SerializedObject(turboSetting.objectReferenceValue);
                EditorGUI.BeginChangeCheck();
                obj.UpdateIfRequiredOrScript();
                SerializedProperty iterator = obj.GetIterator();
                bool enterChildren = true;
                while (iterator.NextVisible(enterChildren))
                {
                    using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                    {
                        if (!enterChildren) EditorGUILayout.PropertyField(iterator, true);
                    }
                    enterChildren = false;
                }
                obj.ApplyModifiedProperties();
                EditorGUI.EndChangeCheck();
            }
            GUILayout.EndVertical();
            m_Settings.ApplyModifiedPropertiesWithoutUndo();
        }
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {

            var provider = new NGDTSettingsProvider("Project/Next Gen Dialogue Setting", SettingsScope.Project)
            {
                keywords = GetSearchKeywordsFromGUIContentProperties<Styles>()
            };
            return provider;

        }
    }
}