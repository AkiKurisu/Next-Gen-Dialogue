using System;
using Kurisu.NGDT.Editor;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.VITS.Editor
{
    [Ordered]
    public class VITSEditorModuleResolver : INodeResolver
    {
        public IDialogueNode CreateNodeInstance(Type type)
        {
            return new VITSEditorModuleNode();
        }
        public static bool IsAcceptable(Type behaviorType) => behaviorType == typeof(VITSEditorModule);
        private class VITSEditorModuleNode : EditorModuleNode
        {
            private readonly Button generateAll;
            public VITSEditorModuleNode()
            {
                mainContainer.Add(new Button(AttachAllPieces) { text = "Attach VITS Module to All Pieces" });
                mainContainer.Add(new Button(AttachAllOptions) { text = "Attach VITS Module to All Options" });
                mainContainer.Add(generateAll = new Button(GenerateAll) { text = "Generate All", tooltip = "Generate all VITS Modules with no audio baked" });
            }
            private void AttachAllPieces()
            {
                MapTreeView.CollectNodes<PieceContainer>().ForEach(x => x.AddModuleNode(new VITSModule()));
            }
            private void AttachAllOptions()
            {
                MapTreeView.CollectNodes<OptionContainer>().ForEach(x => x.AddModuleNode(new VITSModule()));
            }
            private async void GenerateAll()
            {
                generateAll.SetEnabled(false);
                foreach (var container in MapTreeView.CollectNodes<ContainerNode>())
                {
                    if (container.TryGetModuleNode<VITSModule>(out var node))
                    {
                        var vitsModule = node as VITSModuleResolver.VITSModuleNode;
                        if (!await vitsModule.BakeAudio()) break;
                    }
                }
                generateAll.SetEnabled(true);
            }
        }
    }
}