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
        private LocalizedStringField editorField;
        protected override LocalizedStringField CreateEditorField(FieldInfo fieldInfo)
        {
            editorField = new LocalizedStringField(fieldInfo.Name);
            return editorField;
        }
        public static bool IsAcceptable(Type infoType, FieldInfo _) => infoType == typeof(LocalizedString);

    }
    public class LocalizedStringField : WrapObjectField<LocalizedString>
    {
        public LocalizedStringField(string label) : base(label)
        {
            contentContainer.Q<IMGUIContainer>().style.minWidth = 350;
        }
    }
}
