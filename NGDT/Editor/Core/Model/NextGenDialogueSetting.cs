using System;
using System.Linq;
using Ceres.Editor;
using Kurisu.NGDS.AI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [Serializable]
    internal class GraphEditorSetting
    {
        [NodeGroupSelector(typeof(NodeBehavior)), Tooltip("Display type, filter AkiGroup according to this list, nodes without category will always be displayed")]
        public string[] ShowGroups = new string[0];
        [NodeGroupSelector(typeof(NodeBehavior)), Tooltip("The type that is not displayed, filter the AkiGroup according to this list, and the nodes without categories will always be displayed")]
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
        public const string Version = "v2.0.0";
        
        private const string k_NDGTSettingsPath = "ProjectSettings/NextGenDialogueSetting.asset";
        
        private const string k_AITurboSettingsPath = "Assets/AI Turbo Setting.asset";
        
        private const string GraphFallBackPath = "NGDT/Graph";
        
        private const string InspectorFallBackPath = "NGDT/Inspector";
        
        private const string NodeFallBackPath = "NGDT/Node";

        private static NextGenDialogueSetting setting;
        
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
        private static readonly string[] internalNotShowGroups = new string[1] { "Hidden" };
        public static StyleSheet GetGraphStyle()
        {
            var setting = GetOrCreateSettings();
            return setting.graphEditorSetting.graphStyleSheet != null ? setting.graphEditorSetting.graphStyleSheet : Resources.Load<StyleSheet>(GraphFallBackPath);
        }
        public static StyleSheet GetInspectorStyle()
        {
            var setting = GetOrCreateSettings();
            return setting.graphEditorSetting.inspectorStyleSheet != null ? setting.graphEditorSetting.inspectorStyleSheet : Resources.Load<StyleSheet>(InspectorFallBackPath);
        }
        public static StyleSheet GetNodeStyle()
        {
            var setting = GetOrCreateSettings();
            return setting.graphEditorSetting.nodeStyleSheet != null ? setting.graphEditorSetting.nodeStyleSheet : Resources.Load<StyleSheet>(NodeFallBackPath);
        }
        public static (string[] showGroups, string[] notShowGroups) GetMask()
        {
            var setting = GetOrCreateSettings();
            var editorSetting = setting.graphEditorSetting;
            if (editorSetting == null) return (null, null);
            return (editorSetting.ShowGroups, editorSetting.NotShowGroups.Concat(internalNotShowGroups).ToArray());
        }
        
        public static NextGenDialogueSetting GetOrCreateSettings()
        {
            var arr = InternalEditorUtility.LoadSerializedFileAndForget(k_NDGTSettingsPath);
            setting = arr.Length > 0 ? arr[0] as NextGenDialogueSetting : setting ?? CreateInstance<NextGenDialogueSetting>();
            if (!setting.aiTurboSetting)
            {
                setting.aiTurboSetting = GetOrCreateAITurboSetting();
            }
            return setting;
        }
        
        private static AITurboSetting GetOrCreateAITurboSetting()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(AITurboSetting)}");
            AITurboSetting turboSetting;
            if (guids.Length == 0)
            {
                turboSetting = CreateInstance<AITurboSetting>();
                Debug.Log($"AI Turbo Setting saving path : {k_AITurboSettingsPath}");
                AssetDatabase.CreateAsset(turboSetting, k_AITurboSettingsPath);
                AssetDatabase.SaveAssets();
            }
            else turboSetting = AssetDatabase.LoadAssetAtPath<AITurboSetting>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return turboSetting;
        }
        
        public void Save(bool saveAsText = true)
        {
            InternalEditorUtility.SaveToSerializedFileAndForget(new[] { this }, k_NDGTSettingsPath, saveAsText);
        }
    }

    internal class NGDTSettingsProvider : SettingsProvider
    {
        private SerializedObject serializedObject;
        private NextGenDialogueSetting setting;
        private class Styles
        {
            public static GUIContent GraphEditorSettingStyle = new("Graph Editor Setting");
            public static GUIContent AITurboSettingStyle = new("AI Turbo Setting");
        }
        public NGDTSettingsProvider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope) { }
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            serializedObject = new SerializedObject(setting = NextGenDialogueSetting.GetOrCreateSettings());
        }
        public override void OnGUI(string searchContext)
        {
            GUILayout.BeginVertical("Editor Settings", GUI.skin.box);
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("graphEditorSetting"), Styles.GraphEditorSettingStyle);
            GUILayout.EndVertical();
            GUILayout.BeginVertical("Runtime Settings", GUI.skin.box);
            var turboSetting = serializedObject.FindProperty("aiTurboSetting");
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
            if (serializedObject.ApplyModifiedPropertiesWithoutUndo())
            {
                setting.Save();
            }
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