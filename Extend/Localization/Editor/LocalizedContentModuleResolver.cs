using Kurisu.NGDT.Editor;
using System;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Localization.Editor
{
    [Ordered]
    public class LocalizedContentModuleResolver : INodeResolver
    {
        public IDialogueNode CreateNodeInstance(Type type)
        {
            return new LocalizedContentModuleNode();
        }
        public static bool IsAcceptable(Type behaviorType) => behaviorType == typeof(LocalizedContentModule);
    }
    public class LocalizedContentModuleNode : ModuleNode
    {
        private LocalizedStringEditorField editorField;
        protected override void OnRestore()
        {
            UpdateEditor();
        }
        protected override void OnBehaviorSet()
        {
            var tableEntryField = (GetFieldResolver("tableEntry") as FieldResolver<SharedStringField, SharedString>).EditorField;
            var stringEntryField = (GetFieldResolver("stringEntry") as FieldResolver<SharedStringField, SharedString>).EditorField;
            tableEntryField.RegisterValueChangedCallback(x => UpdateEditor());
            stringEntryField.RegisterValueChangedCallback(x => UpdateEditor());
        }
        private void UpdateEditor()
        {
            var tableEntry = this.GetSharedStringValue("tableEntry");
            var stringEntry = this.GetSharedStringValue("stringEntry");
            if (editorField != null) mainContainer.Remove(editorField);
            editorField = null;
            if (string.IsNullOrEmpty(stringEntry) || string.IsNullOrEmpty(tableEntry)) return;
            editorField = new LocalizedStringEditorField(tableEntry, stringEntry);
            mainContainer.Add(editorField);
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Create Entry", (a) =>
            {
                var tableEntry = this.GetSharedStringValue("tableEntry");
                var stringEntry = this.GetSharedStringValue("stringEntry");
                var collection = LocalizationEditorSettings.GetStringTableCollection(tableEntry);
                if (collection == null) return;
                var tables = collection.Tables;
                for (int i = 0; i < tables.Count; i++)
                {
                    var table = tables[i].asset as StringTable;
                    table.SharedData.GetId(stringEntry, true);
                }
            }));
        }
    }
}
