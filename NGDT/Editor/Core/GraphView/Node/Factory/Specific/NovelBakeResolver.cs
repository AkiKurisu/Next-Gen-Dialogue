using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [Ordered]
    public class NovelBakeResolver : INodeResolver
    {
        public IDialogueNode CreateNodeInstance(Type type)
        {
            return new NovelBakeNode();
        }
        public static bool IsAcceptable(Type behaviorType) => behaviorType == typeof(NovelBakeModule);
        private class NovelBakeNode : EditorModuleNode
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
                        var node = MapTreeView.View.Query<Node>().ToList().OfType<IDialogueNode>().FirstOrDefault(x => x.GUID == selection);
                        if (node != null) MapTreeView.View.AddToSelection(node.View);
                    }
                }
            }
            private async void AutoGenerateFromSelection()
            {
                autoGenerate.SetEnabled(false);
                SaveCurrentSelection();
                await MapTreeView.AutoGenerateNovel();
                autoGenerate.SetEnabled(true);
            }
            private void SaveCurrentSelection()
            {
                var containers = MapTreeView.View.selection.OfType<ContainerNode>();
                lastSelection = JsonConvert.SerializeObject(containers.Select(x => x.GUID).ToArray());
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
}
