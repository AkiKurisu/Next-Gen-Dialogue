using NextGenDialogue.AI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NextGenDialogue.Graph.Editor
{
    [FilePath("ProjectSettings/NextGenDialogueSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class NextGenDialogueSettings : ScriptableSingleton<NextGenDialogueSettings>
    {
        private const string AITurboSettingsPath = "Assets/AI Turbo Setting.asset";
        
        public const string GraphStylePath = "NGDT/Graph";
        
        public const string InspectorStylePath = "NGDT/Inspector";
        
        public const string NodeStylePath = "NGDT/Node";

        private static NextGenDialogueSettings _setting;
        
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
        
        public static NextGenDialogueSettings Get()
        {
            _setting = instance;
            if (!_setting!.aiTurboSetting)
            {
                _setting.aiTurboSetting = GetOrCreateAITurboSetting();
            }
            return _setting;
        }
        
        public static void SaveSettings()
        {
            instance.Save(true);
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
    }

    internal class NextGenDialogueSettingsProvider : SettingsProvider
    {
        private SerializedObject _serializedObject;
        
        private class Styles
        {
            public static readonly GUIContent AITurboSettingStyle = new("Current Config");
        }

        private NextGenDialogueSettingsProvider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope) { }
        
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _serializedObject = new SerializedObject(NextGenDialogueSettings.Get());
        }
        
        public override void OnGUI(string searchContext)
        {
            GUILayout.BeginVertical("Editor AI Settings", GUI.skin.box);
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
                NextGenDialogueSettings.SaveSettings();
            }
        }
        
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            var provider = new NextGenDialogueSettingsProvider("Project/Ceres/Next Gen Dialogue Settings", SettingsScope.Project)
            {
                keywords = GetSearchKeywordsFromGUIContentProperties<Styles>()
            };
            
            return provider;
        }
    }
}