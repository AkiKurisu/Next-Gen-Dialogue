using System.Reflection;
using System;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{

    public class SharedBoolResolver : FieldResolver<SharedBoolField, SharedBool>
    {
        public SharedBoolResolver(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }
        protected override void SetTree(IDialogueTreeView ownerTreeView)
        {
            editorField.Init(ownerTreeView);
        }
        private SharedBoolField editorField;
        protected override SharedBoolField CreateEditorField(FieldInfo fieldInfo)
        {
            editorField = new SharedBoolField(fieldInfo.Name, null, fieldInfo.FieldType, fieldInfo);
            return editorField;
        }
        public static bool IsAcceptable(Type infoType, FieldInfo _) => infoType == typeof(SharedBool);

    }
    public class SharedBoolField : SharedVariableField<SharedBool, bool>
    {
        public SharedBoolField(string label, VisualElement visualInput, Type objectType, FieldInfo fieldInfo) : base(label, visualInput, objectType, fieldInfo)
        {


        }

        protected override BaseField<bool> CreateValueField() => new Toggle("Value");
    }
}
