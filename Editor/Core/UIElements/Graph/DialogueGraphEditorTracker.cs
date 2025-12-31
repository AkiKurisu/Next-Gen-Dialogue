using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor;

namespace NextGenDialogue.Graph.Editor
{
    /// <summary>
    /// Editor tracker for visualizing dialogue execution state in the graph view
    /// </summary>
    public class DialogueGraphEditorTracker : DialogueGraphTracker
    {
        private readonly DialogueGraphView _graphView;

        public DialogueGraphEditorTracker(DialogueGraphView graphView)
        {
            _graphView = graphView;
        }

        public override UniTask OnDialogueUpdate(Root root)
        {
            if (!EditorApplication.isPlaying)
            {
                return UniTask.CompletedTask;
            }

            // Clear all node styles when dialogue updates
            EditorApplication.delayCall += ClearAllNodeStyles;

            return UniTask.CompletedTask;
        }

        public override UniTask OnPieceTransition(string pieceId)
        {
            if (!EditorApplication.isPlaying)
            {
                return UniTask.CompletedTask;
            }

            // Clear all node styles when transitioning to next piece
            EditorApplication.delayCall += ClearAllNodeStyles;

            return UniTask.CompletedTask;
        }

        public override UniTask OnNodeUpdate(DialogueNode node, Status status)
        {
            if (!EditorApplication.isPlaying)
            {
                return UniTask.CompletedTask;
            }

            // Mark node as executed with corresponding style class
            EditorApplication.delayCall += () =>
            {
                var nodeView = _graphView?.CollectViews<IDialogueNodeView>()
                    .FirstOrDefault(view => view.NodeInstance == node);

                if (nodeView != null)
                {
                    // Remove existing status classes
                    nodeView.NodeElement.RemoveFromClassList("status_success");
                    nodeView.NodeElement.RemoveFromClassList("status_failure");
                    
                    // Add status class based on execution status
                    switch (status)
                    {
                        case Status.Success:
                            nodeView.NodeElement.AddToClassList("status_success");
                            break;
                        case Status.Failure:
                            nodeView.NodeElement.AddToClassList("status_failure");
                            break;
                    }
                }
            };

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// Clear all node status styles from the graph view
        /// </summary>
        private void ClearAllNodeStyles()
        {
            _graphView?.CollectViews<IDialogueNodeView>()
                .ForEach(view =>
                {
                    view.NodeElement.RemoveFromClassList("status_success");
                    view.NodeElement.RemoveFromClassList("status_failure");
                });
        }
    }
}

