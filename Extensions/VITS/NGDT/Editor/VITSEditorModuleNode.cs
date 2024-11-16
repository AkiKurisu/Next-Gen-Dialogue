using Ceres.Editor;
using Kurisu.NGDT.Editor;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.VITS.Editor
{
    [CustomNodeEditor(typeof(VITSEditorModule))]
    public class VITSEditorModuleNode : EditorModuleNode
    {
        private readonly Button generateAll;
        private Toggle skipContainedAudioClip;
        private Toggle skipSharedAudioClip;
        public VITSEditorModuleNode()
        {
            mainContainer.Add(new Button(AttachAllPieces) { text = "Attach VITS Module to All Pieces" });
            mainContainer.Add(new Button(AttachAllOptions) { text = "Attach VITS Module to All Options" });
            mainContainer.Add(generateAll = new Button(GenerateAll) { text = "Generate All", tooltip = "Generate all VITS Modules with no audio baked" });
        }
        protected override void OnBehaviorSet()
        {
            skipContainedAudioClip = (GetFieldResolver("skipContainedAudioClip") as BoolResolver).EditorField;
            skipSharedAudioClip = (GetFieldResolver("skipSharedAudioClip") as BoolResolver).EditorField;
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
                    var vitsModule = node as VITSModuleNode;
                    if (skipContainedAudioClip.value && vitsModule.ContainsAudioClip()) continue;
                    if (skipSharedAudioClip.value && vitsModule.IsSharedMode()) continue;
                    if (!await vitsModule.BakeAudio()) break;
                }
            }
            generateAll.SetEnabled(true);
        }
    }
}