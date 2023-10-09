using System.Reflection;
#if !UNITY_2022_1_OR_NEWER
using UnityEditor.UIElements;
#endif
using System;
using UnityEngine.UIElements;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public class SharedVector3Resolver : FieldResolver<SharedVector3Field, SharedVector3>
    {
        public SharedVector3Resolver(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }
        protected override void SetTree(IDialogueTreeView ownerTreeView)
        {
            editorField.Init(ownerTreeView);
        }
        private SharedVector3Field editorField;
        protected override SharedVector3Field CreateEditorField(FieldInfo fieldInfo)
        {
            editorField = new SharedVector3Field(fieldInfo.Name, null, fieldInfo.FieldType, fieldInfo);
            return editorField;
        }
        public static bool IsAcceptable(Type infoType, FieldInfo _) => infoType == typeof(SharedVector3);
    }
    public class SharedVector3Field : SharedVariableField<SharedVector3, Vector3>
    {
        public SharedVector3Field(string label, VisualElement visualInput, Type objectType, FieldInfo fieldInfo) : base(label, visualInput, objectType, fieldInfo)
        {
        }
        protected override BaseField<Vector3> CreateValueField() => new Vector3Field();
    }
}
