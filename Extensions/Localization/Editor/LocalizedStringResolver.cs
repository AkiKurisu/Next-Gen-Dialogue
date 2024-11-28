using System;
using System.Reflection;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEngine.Localization;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Localization.Editor
{
    [Ordered]
    public class LocalizedStringResolver : FieldResolver<LocalizedStringField, LocalizedString>
    {
        public LocalizedStringResolver(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }
        protected override LocalizedStringField CreateEditorField(FieldInfo fieldInfo)
        {
            return new LocalizedStringField(fieldInfo.Name, fieldInfo);
        }
        public static bool IsAcceptable(Type infoType, FieldInfo _) => infoType == typeof(LocalizedString);

    }
    public sealed class LocalizedStringField : WrapField<LocalizedString>
    {
        public LocalizedStringField(string label, FieldInfo fieldInfo) : base(label, fieldInfo)
        {
            contentContainer.Q<IMGUIContainer>().style.minWidth = 350;
        }
    }
}
