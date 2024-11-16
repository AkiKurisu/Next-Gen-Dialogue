using System.Reflection;
using System;
using Ceres;
using UnityEngine;
using UnityEngine.UIElements;
namespace Ceres.Editor
{
    public class SharedStringResolver : FieldResolver<SharedStringField, SharedString>
    {
        public SharedStringResolver(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }
        protected override SharedStringField CreateEditorField(FieldInfo fieldInfo)
        {
            return new SharedStringField(fieldInfo.Name, null, fieldInfo.FieldType, fieldInfo);
        }
        public static bool IsAcceptable(Type infoType, FieldInfo _) => infoType == typeof(SharedString);

    }
    public class SharedStringField : SharedVariableField<SharedString, string>
    {
        private readonly bool multiline;
        public SharedStringField(string label, VisualElement visualInput, Type objectType, FieldInfo fieldInfo) : base(label, visualInput, objectType, fieldInfo)
        {
            multiline = fieldInfo.GetCustomAttribute<MultilineAttribute>() != null;
        }
        protected override BaseField<string> CreateValueField()
        {
            TextField textField;
            textField = new TextField();
            if (multiline)
            {
                textField.multiline = true;
                textField.style.maxWidth = 250;
                textField.style.whiteSpace = WhiteSpace.Normal;
                textField.AddToClassList("Multiline");
            }
            return textField;
        }
    }
}