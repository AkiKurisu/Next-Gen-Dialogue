using System;
using System.Reflection;
using Kurisu.NGDT.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Localization.Editor
{
    public class LocalizedStringResolver : FieldResolver<LocalizedStringField, LocalizedString>
    {
        public LocalizedStringResolver(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }
        private LocalizedStringField editorField;
        protected override LocalizedStringField CreateEditorField(FieldInfo fieldInfo)
        {
            editorField = new LocalizedStringField(fieldInfo.Name, null);
            return editorField;
        }
        public static bool IsAcceptable(Type infoType, FieldInfo _) => infoType == typeof(LocalizedString);

    }
    public class LocalizedStringField : BaseField<LocalizedString>
    {
        LocalizedStringInstance m_Instance;
        private LocalizedStringInstance Instance => m_Instance != null ? m_Instance : GetInstance();
        SerializedObject m_SerializedObject;
        SerializedProperty m_SerializedProperty;
        public LocalizedStringField(string label, VisualElement visualInput) : base(label, visualInput)
        {
            var element = CreateLocalizedStringNode();
            element.style.minWidth = 350;
            Add(element);
        }
        private LocalizedStringInstance GetInstance()
        {
            m_Instance = ScriptableObject.CreateInstance<LocalizedStringInstance>();
            m_Instance.localizedString = value;
            m_SerializedObject = new SerializedObject(m_Instance);
            m_SerializedProperty = m_SerializedObject.FindProperty("localizedString");
            return m_Instance;
        }
        VisualElement CreateLocalizedStringNode()
        {
            return new IMGUIContainer(() =>
            {
                m_SerializedObject.Update();
                EditorGUILayout.PropertyField(m_SerializedProperty);
                if (m_SerializedObject.ApplyModifiedProperties())
                {
                    base.value = Instance.localizedString;
                }
            });
        }
        public sealed override LocalizedString value
        {
            get => base.value; set
            {
                if (value == null)
                {
                    Instance.localizedString = new LocalizedString();
                }
                else
                {
                    Instance.localizedString = ReflectionHelper.DeepCopy(value);
                }
                base.value = Instance.localizedString;
            }
        }
    }
}
