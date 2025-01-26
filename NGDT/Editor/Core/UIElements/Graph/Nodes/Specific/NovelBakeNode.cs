using System;
using System.Collections.Generic;
using System.Linq;
using Ceres.Editor;
using Ceres.Editor.Graph;
using Newtonsoft.Json;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [CustomNodeView(typeof(NovelBakeModule))]
    public class NovelBakeNode : EditorModuleNode
    {
        private string _lastSelection;
        
        private readonly Button _autoGenerate;
        
        private readonly Button _loadLast;
        
        public NovelBakeNode(Type type, CeresGraphView graphView): base(type, graphView)
        {
            mainContainer.Add(_autoGenerate = new Button(AutoGenerateFromSelection) { text = "Auto Generate Novel" });
            mainContainer.Add(_loadLast = new Button(LoadLastBakeSelections) { text = "Load Last Bake Selections" });
            _loadLast.SetEnabled(false);
        }
        private void LoadLastBakeSelections()
        {
            if (!string.IsNullOrEmpty(_lastSelection))
            {
                var lastSelections = JsonConvert.DeserializeObject<string[]>(_lastSelection);
                foreach (var selection in lastSelections)
                {
                    var node = Graph.Query<Node>().ToList().OfType<IDialogueNodeView>().FirstOrDefault(x => x.Guid == selection);
                    if (node != null) Graph.AddToSelection(node.NodeElement);
                }
            }
        }
        private async void AutoGenerateFromSelection()
        {
            _autoGenerate.SetEnabled(false);
            SaveCurrentSelection();
            await NovelBaker.AutoGenerateNovel(Graph);
            _autoGenerate.SetEnabled(true);
        }
        private void SaveCurrentSelection()
        {
            var containers = Graph.selection.OfType<ContainerNode>();
            _lastSelection = JsonConvert.SerializeObject(containers.Select(x => x.Guid).ToArray());
            _loadLast.SetEnabled(true);
        }
        protected override void OnRestore()
        {
            _lastSelection = (NodeBehavior as NovelBakeModule).lastSelection;
            _loadLast.SetEnabled(!string.IsNullOrEmpty(_lastSelection));
        }
        protected override void OnCommit(Stack<IDialogueNodeView> stack)
        {
            (NodeBehavior as NovelBakeModule).lastSelection = _lastSelection;
        }
    }
}