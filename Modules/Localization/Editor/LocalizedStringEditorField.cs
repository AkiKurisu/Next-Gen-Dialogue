using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;
using UnityEditor;
namespace Kurisu.NGDT.Localization.Editor
{
    public class LocalizedStringEditorField : VisualElement
    {
        private static Color AkiBlue = new(140 / 255f, 160 / 255f, 250 / 255f);
        public LocalizedStringEditorField(string tableEntry, string keyEntry, bool autoAddNewEntry = false)
        {
            var toggleGroup = new ToggleGroup();
            var buttonGroup = new VisualElement();
            buttonGroup.style.flexDirection = FlexDirection.Row;
            Add(buttonGroup);
            Add(toggleGroup);
            var collection = LocalizationEditorSettings.GetStringTableCollection(tableEntry);
            if (collection == null) return;
            var tables = collection.Tables;
            for (int i = 0; i < tables.Count; i++)
            {
                var table = tables[i].asset as StringTable;
                //If autoAddNewEntry is true, it will automatically generate entry id for new content
                var id = table.SharedData.GetId(keyEntry, autoAddNewEntry);
                if (id == SharedTableData.EmptyId) continue;
                var editorField = new TextField
                {
                    multiline = true
                };
                editorField.style.whiteSpace = WhiteSpace.Normal;
                if (!table.ContainsKey(id))
                {
                    table.AddEntry(id, string.Empty);
                    EditorUtility.SetDirty(table.SharedData);
                    EditorUtility.SetDirty(table);
                }
                editorField.value = table.GetEntry(id).LocalizedValue;
                editorField.RegisterValueChangedCallback(x =>
                {
                    table.GetEntry(id).Value = x.newValue;
                    EditorUtility.SetDirty(table);
                    EditorUtility.SetDirty(table.SharedData);
                    EditorUtility.SetDirty(collection);
                });
                editorField.multiline = true;
                toggleGroup.AddToggleElement(editorField);
                int k = i;
                var button = GetButton(table.LocaleIdentifier.Code, () => toggleGroup.Toggle(k), Color.grey);
                toggleGroup.OnToggle += (index) => { button.style.backgroundColor = index == k ? AkiBlue : Color.grey; };
                buttonGroup.Add(button);
            }
            toggleGroup.Toggle(0);
            EditorUtility.SetDirty(collection);
        }
        private static Button GetButton(string label, System.Action clickEvent, Color color)
        {
            var button = new Button(clickEvent)
            {
                text = label
            };
            button.style.fontSize = 14;
            button.style.color = Color.white;
            button.style.backgroundColor = color;
            return button;
        }
    }
}
