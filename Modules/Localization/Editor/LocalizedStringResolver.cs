using System;
using System.Reflection;
using Kurisu.NGDT.Editor;
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
            return new LocalizedStringField(fieldInfo.Name);
        }
        public static bool IsAcceptable(Type infoType, FieldInfo _) => infoType == typeof(LocalizedString);

    }
    public class LocalizedStringField : WrapField<LocalizedString>
    {
        public LocalizedStringField(string label) : base(label)
        {
            contentContainer.Q<IMGUIContainer>().style.minWidth = 350;
        }
    }
}
