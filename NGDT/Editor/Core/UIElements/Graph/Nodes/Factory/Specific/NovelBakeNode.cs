using System.Collections.Generic;
using System.Linq;
using Ceres.Editor;
using Newtonsoft.Json;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [CustomNodeView(typeof(NovelBakeModule))]
    public class NovelBakeNode : EditorModuleNode
    {
        private string lastSelection;
        private readonly Button autoGenerate;
        private readonly Button loadLast;
        public NovelBakeNode()
        {
            mainContainer.Add(autoGenerate = new Button(AutoGenerateFromSelection) { text = "Auto Generate Novel" });
            mainContainer.Add(loadLast = new Button(LoadLastBakeSelections) { text = "Load Last Bake Selections" });
            loadLast.SetEnabled(false);
        }
        private void LoadLastBakeSelections()
        {
            if (!string.IsNullOrEmpty(lastSelection))
            {
                var lastSelections = JsonConvert.DeserializeObject<string[]>(lastSelection);
                foreach (var selection in lastSelections)
                {
                    var node = GraphView.Query<Node>().ToList().OfType<IDialogueNode>().FirstOrDefault(x => x.Guid == selection);
                    if (node != null) GraphView.AddToSelection(node.NodeElement);
                }
            }
        }
        private async void AutoGenerateFromSelection()
        {
            autoGenerate.SetEnabled(false);
            SaveCurrentSelection();
            await NovelBaker.AutoGenerateNovel(GraphView);
            autoGenerate.SetEnabled(true);
        }
        private void SaveCurrentSelection()
        {
            var containers = GraphView.selection.OfType<ContainerNode>();
            lastSelection = JsonConvert.SerializeObject(containers.Select(x => x.Guid).ToArray());
            loadLast.SetEnabled(true);
        }
        protected override void OnRestore()
        {
            lastSelection = (NodeBehavior as NovelBakeModule).lastSelection;
            loadLast.SetEnabled(!string.IsNullOrEmpty(lastSelection));
        }
        protected override void OnCommit(Stack<IDialogueNode> stack)
        {
            (NodeBehavior as NovelBakeModule).lastSelection = lastSelection;
        }
    }
}