using System;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;
using NextGenDialogue.Graph.Editor;
namespace NextGenDialogue.Graph.Localization.Editor
{
    [CustomNodeView(typeof(LocalizedContentModule))]
    public class LocalizedContentModuleNodeView : ModuleNodeView
    {
        private LocalizedStringEditorField _editorField;
        
        public LocalizedContentModuleNodeView(Type type, CeresGraphView graphView) : base(type, graphView)
        {
        }
        
        protected override void OnRestore()
        {
            UpdateEditor();
        }
        
        protected override void Initialize(Type nodeType, DialogueGraphView graphView)
        {
            base.Initialize(nodeType, graphView);
            var tableEntryField = ((SharedStringResolver)GetFieldResolver("tableEntry")).BaseField;
            var stringEntryField = ((SharedStringResolver)GetFieldResolver("stringEntry")).BaseField;
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
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Create localized entry", (a) =>
            {
                var tableEntry = this.GetSharedStringValue("tableEntry");
                var stringEntry = this.GetSharedStringValue("stringEntry");
                var collection = LocalizationEditorSettings.GetStringTableCollection(tableEntry);
                if (collection == null) return;
                var tableReferences = collection.Tables;
                foreach (var reference in tableReferences)
                {
                    var table = (StringTable)reference.asset;
                    table.SharedData.GetId(stringEntry, true);
                }
            }));
        }
    }
}
