using System.Reflection;
using System;
using Ceres;
using UnityEngine.UIElements;
namespace Ceres.Editor
{

    public class SharedBoolResolver : FieldResolver<SharedBoolField, SharedBool>
    {
        public SharedBoolResolver(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }
        protected override SharedBoolField CreateEditorField(FieldInfo fieldInfo)
        {
            return new SharedBoolField(fieldInfo.Name, null, fieldInfo.FieldType, fieldInfo);
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
