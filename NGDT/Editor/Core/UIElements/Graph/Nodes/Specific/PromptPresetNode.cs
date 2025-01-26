using System;
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
        private readonly TextField _textField;
        
        private SharedTObjectField<TextAsset> _promptField;
        
        public PromptPresetNode(Type type, CeresGraphView graphView): base(type, graphView)
        {
            _textField = new TextField("Edit Prompt")
            {
                style =
                {
                    minWidth = 200,
                    maxWidth = 250,
                    whiteSpace = WhiteSpace.Normal
                },
                multiline = true
            };
            _textField.AddToClassList("Multiline");
        }
        
        protected override void Initialize(Type nodeType, DialogueGraphView graphView)
        {
            base.Initialize(nodeType, graphView);
            _promptField = ((SharedTObjectResolver<TextAsset>)GetFieldResolver("prompt")).BaseField;
            _promptField.RegisterValueChangedCallback(x => UpdateTextField());
        }
        
        protected override void OnRestore()
        {
            UpdateTextField();
        }

        private void UpdateTextField()
        {
            var textAsset = this.GetSharedVariableValue<TextAsset>("prompt");
            _textField.RemoveFromHierarchy();
            if (textAsset == null) return;
            _textField.value = textAsset.text;
            mainContainer.Add(_textField);
        }
        
        protected override void OnCommit(Stack<IDialogueNodeView> stack)
        {
            var textAsset = this.GetSharedVariableValue<TextAsset>("prompt");
            if (textAsset != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(textAsset);
                string assetFullPath = Application.dataPath + assetPath[6..];
                File.WriteAllText(assetFullPath, _textField.text);
                EditorUtility.SetDirty(textAsset);
            }
        }
    }
}