using System.Reflection;
#if !UNITY_2022_1_OR_NEWER
using UnityEditor.UIElements;
#endif
using System;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public class SharedIntResolver : FieldResolver<SharedIntField, SharedInt>
    {
        public SharedIntResolver(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }
        protected override void SetTree(IDialogueTreeView ownerTreeView)
        {
            editorField.Init(ownerTreeView);
        }
        private SharedIntField editorField;
        protected override SharedIntField CreateEditorField(FieldInfo fieldInfo)
        {
            editorField = new SharedIntField(fieldInfo.Name, null, fieldInfo.FieldType, fieldInfo);
            return editorField;
        }
        public static bool IsAcceptable(Type infoType, FieldInfo _) => infoType == typeof(SharedInt);

    }
    public class SharedIntField : SharedVariableField<SharedInt, int>
    {

        public SharedIntField(string label, VisualElement visualInput, Type objectType, FieldInfo fieldInfo) : base(label, visualInput, objectType, fieldInfo)
        {
        }
        protected override BaseField<int> CreateValueField() => new IntegerField();
    }
}