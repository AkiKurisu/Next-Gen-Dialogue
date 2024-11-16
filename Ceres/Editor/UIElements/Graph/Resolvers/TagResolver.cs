using System;
using System.Reflection;
using Ceres.Annotations;
using UnityEditor.UIElements;
namespace Ceres.Editor
{
    [Ordered]
    public class TagResolver : FieldResolver<TagField, string>
    {
        public TagResolver(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }
        protected override TagField CreateEditorField(FieldInfo fieldInfo)
        {
            return new TagField(fieldInfo.Name);
        }
        public static bool IsAcceptable(Type infoType, FieldInfo info) => infoType == typeof(string) && info.GetCustomAttribute<TagAttribute>() != null;
    }
}