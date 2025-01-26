using System;
using Ceres;
using Ceres.Editor;
using Ceres.Editor.Graph;
using Kurisu.NGDT.Editor;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Localization.Editor
{
    [CustomNodeView(typeof(LocalizedContentModule))]
    public class LocalizedContentModuleNode : ModuleNode
    {
        private LocalizedStringEditorField _editorField;
        
        public LocalizedContentModuleNode(Type type, CeresGraphView graphView) : base(type, graphView)
        {
        }
        
        protected override void OnRestore()
        {
            UpdateEditor();
        }
        
        protected override void Initialize(Type nodeType, DialogueGraphView graphView)
        {
            base.Initialize(nodeType, graphView);
            var tableEntryField = (GetFieldResolver("tableEntry") as FieldResolver<SharedStringResolver.SharedStringField, SharedString>)?.BaseField;
            var stringEntryField = (GetFieldResolver("stringEntry") as FieldResolver<SharedStringResolver.SharedStringField, SharedString>)?.BaseField;
            tableEntryField.RegisterValueChangedCallback(x => UpdateEditor());
            stringEntryField.RegisterValueChangedCallback(x => UpdateEditor());
        }
        
        private void UpdateEditor()
        {
            var tableEntry = this.GetSharedStringValue("tableEntry");
            var stringEntry = this.GetSharedStringValue("stringEntry");
            if (_editorField != null) mainContainer.Remove(_editorField);
            _editorField = null;
            if (string.IsNullOrEmpty(stringEntry) || string.IsNullOrEmpty(tableEntry)) return;
            _editorField = new LocalizedStringEditorField(tableEntry, stringEntry);
            mainContainer.Add(_editorField);
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Create Entry", (a) =>
            {
                var tableEntry = this.GetSharedStringValue("tableEntry");
                var stringEntry = this.GetSharedStringValue("stringEntry");
                var collection = LocalizationEditorSettings.GetStringTableCollection(tableEntry);
                if (collection == null) return;
                var tables = collection.Tables;
                for (int i = 0; i < tables.Count; i++)
                {
                    var table = (StringTable)tables[i].asset;
                    table.SharedData.GetId(stringEntry, true);
                }
            }));
        }
    }
}
