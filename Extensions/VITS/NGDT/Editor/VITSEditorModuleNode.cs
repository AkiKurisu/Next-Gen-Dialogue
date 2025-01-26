using System;
using Ceres.Editor;
using Ceres.Editor.Graph;
using Kurisu.NGDT.Editor;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.VITS.Editor
{
    [CustomNodeView(typeof(VITSEditorModule))]
    public class VITSEditorModuleNode : EditorModuleNode
    {
        private readonly Button _generateAll;
        
        private Toggle _skipContainedAudioClip;
        
        private Toggle _skipSharedAudioClip;
        
        public VITSEditorModuleNode(Type type, CeresGraphView graphView): base(type, graphView)
        {
            mainContainer.Add(new Button(AttachAllPieces) { text = "Attach VITS Module to All Pieces" });
            mainContainer.Add(new Button(AttachAllOptions) { text = "Attach VITS Module to All Options" });
            mainContainer.Add(_generateAll = new Button(GenerateAll) { text = "Generate All", tooltip = "Generate all VITS Modules with no audio baked" });
        }
        
        protected override void Initialize(Type nodeType, DialogueGraphView graphView)
        {
            base.Initialize(nodeType, graphView);
            _skipContainedAudioClip = ((BoolResolver)GetFieldResolver("skipContainedAudioClip")).BaseField;
            _skipSharedAudioClip = ((BoolResolver)GetFieldResolver("skipSharedAudioClip")).BaseField;
        }
        
        private void AttachAllPieces()
        {
            Graph.CollectNodes<PieceContainer>().ForEach(x => x.AddModuleNode(new VITSModule()));
        }
        
        private void AttachAllOptions()
        {
            Graph.CollectNodes<OptionContainer>().ForEach(x => x.AddModuleNode(new VITSModule()));
        }
        
        private async void GenerateAll()
        {
            _generateAll.SetEnabled(false);
            foreach (var container in Graph.CollectNodes<ContainerNode>())
            {
                if (container.TryGetModuleNode<VITSModule>(out var node))
                {
                    var vitsModule = (VITSModuleNode)node;
                    if (_skipContainedAudioClip.value && vitsModule.ContainsAudioClip()) continue;
                    if (_skipSharedAudioClip.value && vitsModule.IsSharedMode()) continue;
                    if (!await vitsModule.BakeAudio()) break;
                }
            }
            _generateAll.SetEnabled(true);
        }
    }
}