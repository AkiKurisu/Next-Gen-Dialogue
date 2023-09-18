using Kurisu.Localization.Editor;
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
        private void UpdateEditor()
        {
            var tableEntry = this.GetSharedStringValue(mapTreeView, "tableEntry");
            var stringEntry = this.GetSharedStringValue(mapTreeView, "stringEntry");
            if (editorField != null) mainContainer.Remove(editorField);
            editorField = new LocalizedStringEditorField(tableEntry, stringEntry);
            mainContainer.Add(editorField);
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Refresh Preview", (a) =>
            {
                UpdateEditor();
            }));
        }
    }
}
