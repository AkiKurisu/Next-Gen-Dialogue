using System.Collections.Generic;
using System.IO;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [CustomNodeView(typeof(PromptPresetModule))]
    public class PromptPresetNode : ModuleNode
    {
        private readonly TextField textField;
        private SharedTObjectField<TextAsset> promptField;
        public PromptPresetNode()
        {
            textField = new TextField("Edit Prompt");
            textField.style.minWidth = 200;
            textField.multiline = true;
            textField.style.maxWidth = 250;
            textField.style.whiteSpace = WhiteSpace.Normal;
            textField.AddToClassList("Multiline");
        }
        protected override void OnBehaviorSet()
        {
            promptField = ((SharedTObjectResolver<TextAsset>)GetFieldResolver("prompt")).BaseField;
            promptField.RegisterValueChangedCallback(x => UpdateTextField());
        }
        protected override void OnRestore()
        {
            UpdateTextField();
        }

        private void UpdateTextField()
        {
            var textAsset = this.GetSharedVariableValue<TextAsset>("prompt");
            textField.RemoveFromHierarchy();
            if (textAsset != null)
            {
                textField.value = textAsset.text;
                mainContainer.Add(textField);
            }
        }
        protected override void OnCommit(Stack<IDialogueNode> stack)
        {
            var textAsset = this.GetSharedVariableValue<TextAsset>("prompt");
            if (textAsset != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(textAsset);
                string assetFullPath = Application.dataPath + assetPath[6..];
                File.WriteAllText(assetFullPath, textField.text);
                EditorUtility.SetDirty(textAsset);
            }
        }
    }
}