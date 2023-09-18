using System.Reflection;
#if !UNITY_2022_1_OR_NEWER
using UnityEditor.UIElements;
#endif
using System;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public class SharedFloatResolver : FieldResolver<SharedFloatField, SharedFloat>
    {
        public SharedFloatResolver(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }
        protected override void SetTree(IDialogueTreeView ownerTreeView)
        {
            editorField.Init(ownerTreeView);
        }
        private SharedFloatField editorField;
        protected override SharedFloatField CreateEditorField(FieldInfo fieldInfo)
        {
            editorField = new SharedFloatField(fieldInfo.Name, null, fieldInfo.FieldType, fieldInfo);
            return editorField;
        }
        public static bool IsAcceptable(Type infoType, FieldInfo _) => infoType == typeof(SharedFloat);

    }
    public class SharedFloatField : SharedVariableField<SharedFloat, float>
    {

        public SharedFloatField(string label, VisualElement visualInput, Type objectType, FieldInfo fieldInfo) : base(label, visualInput, objectType, fieldInfo)
        {

        }
        protected override BaseField<float> CreateValueField() => new FloatField();
    }
}