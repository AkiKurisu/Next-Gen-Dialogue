using Kurisu.NGDT.Editor;
using System;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Localization.Editor
{
    [Ordered]
    public class LocalizedContentResolver : INodeResolver
    {
        public IDialogueNode CreateNodeInstance(Type type)
        {
            return new LocalizedContentNode();
        }
        public static bool IsAcceptable(Type behaviorType) => behaviorType == typeof(LocalizedContentModule);
    }
    public class LocalizedContentNode : ModuleNode
    {
        private LocalizedStringEditorField editorField;
        protected override void OnRestore()
        {
            UpdateEditor();
        }
        protected override void OnBehaviorSet(Type newType)
        {
            var tableEntryField = (GetFieldResolver("tableEntry") as FieldResolver<SharedStringField, SharedString>).EditorField;
            var stringEntryField = (GetFieldResolver("stringEntry") as FieldResolver<SharedStringField, SharedString>).EditorField;
            tableEntryField.RegisterValueChangedCallback(x => UpdateEditor());
            stringEntryField.RegisterValueChangedCallback(x => UpdateEditor());
        }
        private void UpdateEditor()
        {
            var tableEntry = this.GetSharedStringValue(mapTreeView, "tableEntry");
            var stringEntry = this.GetSharedStringValue(mapTreeView, "stringEntry");
            if (editorField != null) mainContainer.Remove(editorField);
            editorField = null;
            if (string.IsNullOrEmpty(stringEntry) || string.IsNullOrEmpty(tableEntry)) return;
            editorField = new LocalizedStringEditorField(tableEntry, stringEntry);
            mainContainer.Add(editorField);
        }
    }
}
