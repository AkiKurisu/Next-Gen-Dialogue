using System;
using System.Linq;
using Ceres.Annotations;
using Ceres.Editor;
using Ceres.Editor.Graph;
using Kurisu.NGDS.AI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
namespace Kurisu.NGDT.Editor
{
    public class NextGenDialogueSettings : ScriptableObject
    {
        [Serializable]
        public class GraphEditorSetting
        {
            [NodeGroupSelector(typeof(NodeBehavior))]
            [Tooltip("Display type, filter NodeGroup according to this list, nodes without category will always be displayed")]
            public string[] showGroups = Array.Empty<string>();
        
            [NodeGroupSelector(typeof(NodeBehavior))]
            [Tooltip("The type that is not displayed, filter NodeGroup according to this list, and the nodes without categories will always be displayed")]
            public string[] notShowGroups = Array.Empty<string>();
        
            [Tooltip("You can customize the style of the Graph view")]
            public StyleSheet graphStyleSheet;
        
            [Tooltip("You can customize the style of the Inspector inspector")]
            public StyleSheet inspectorStyleSheet;
        
            [Tooltip("You can customize the style of Node nodes")]
            public StyleSheet nodeStyleSheet;
        }
        
        public const string Version = "v2.0.0";
        
        private const string SettingsPath = "ProjectSettings/NextGenDialogueSetting.asset";
        
        private const string AITurboSettingsPath = "Assets/AI Turbo Setting.asset";
        
        private const string GraphFallBackPath = "NGDT/Graph";
        
        private const string InspectorFallBackPath = "NGDT/Inspector";
        
        private const string NodeFallBackPath = "NGDT/Node";

        private static NextGenDialogueSettings _setting;
        
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
        
        public static NodeSearchContext GetNodeSearchContext()
        {
            var dialogueSettings = GetOrCreateSettings();
            var editorSetting = dialogueSettings.graphEditorSetting;
            if (editorSetting == null) return NodeSearchContext.Default;
            return new NodeSearchContext()
            {
                ShowGroups = editorSetting.showGroups,
                HideGroups = editorSetting.notShowGroups.Concat(new[]{ NodeGroup.Hidden }).ToArray()
            };
        }
        
        public static NextGenDialogueSettings GetOrCreateSettings()
        {
            var arr = InternalEditorUtility.LoadSerializedFileAndForget(SettingsPath);
            if (arr.Length > 0)
            {
                _setting = arr[0] as NextGenDialogueSettings;
            }
            else
            {
                _setting = CreateInstance<NextGenDialogueSettings>();
                _setting.Save();
            }
            if (!_setting!.aiTurboSetting)
            {
                _setting.aiTurboSetting = GetOrCreateAITurboSetting();
            }
            return _setting;
        }
        
        private static AITurboSetting GetOrCreateAITurboSetting()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(AITurboSetting)}");
            AITurboSetting turboSetting;
            if (guids.Length == 0)
            {
                turboSetting = CreateInstance<AITurboSetting>();
                Debug.Log($"AI Turbo Setting saving path : {AITurboSettingsPath}");
                AssetDatabase.CreateAsset(turboSetting, AITurboSettingsPath);
                AssetDatabase.SaveAssets();
            }
            else turboSetting = AssetDatabase.LoadAssetAtPath<AITurboSetting>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return turboSetting;
        }
        
        public void Save(bool saveAsText = true)
        {
            InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] { this }, SettingsPath, saveAsText);
        }
    }

    internal class NGDTSettingsProvider : SettingsProvider
    {
        private SerializedObject _serializedObject;
        
        private NextGenDialogueSettings _setting;
        
        private class Styles
        {
            public static readonly GUIContent GraphEditorSettingStyle = new("Graph Editor Setting");
            
            public static readonly GUIContent AITurboSettingStyle = new("AI Turbo Setting");
        }
        
        public NGDTSettingsProvider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope) { }
        
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _serializedObject = new SerializedObject(_setting = NextGenDialogueSettings.GetOrCreateSettings());
        }
        
        public override void OnGUI(string searchContext)
        {
            GUILayout.BeginVertical("Editor Settings", GUI.skin.box);
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.PropertyField(_serializedObject.FindProperty("graphEditorSetting"), Styles.GraphEditorSettingStyle);
            GUILayout.EndVertical();
            GUILayout.BeginVertical("Runtime Settings", GUI.skin.box);
            var turboSetting = _serializedObject.FindProperty("aiTurboSetting");
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
            if (_serializedObject.ApplyModifiedPropertiesWithoutUndo())
            {
                _setting.Save();
            }
        }
        
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            var provider = new NGDTSettingsProvider("Project/Next Gen Dialogue Settings", SettingsScope.Project)
            {
                keywords = GetSearchKeywordsFromGUIContentProperties<Styles>()
            };
            
            return provider;
        }
    }
}