using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NextGenDialogue.Graph.Tests
{
    /// <summary>
    /// Example custom tracker implementation for logging dialogue execution
    /// </summary>
    public class DialogueGraphLoggerTracker : DialogueGraphTracker
    {
        private readonly string _logPrefix;

        public DialogueGraphLoggerTracker(string logPrefix = "[Dialogue]")
        {
            _logPrefix = logPrefix;
        }

        public override UniTask OnDialogueUpdate(Root root)
        {
            Debug.Log($"{_logPrefix} Dialogue system updated");
            return UniTask.CompletedTask;
        }

        public override UniTask OnPieceTransition(string pieceId)
        {
            Debug.Log($"{_logPrefix} Transitioning to piece: {pieceId}");
            return UniTask.CompletedTask;
        }

        public override UniTask OnNodeUpdate(DialogueNode node, Status status)
        {
            var statusText = status == Status.Success ? "✓" : "✗";
            Debug.Log($"{_logPrefix} {statusText} Node [{node.GetType().Name}] executed with status: {status}");
            return UniTask.CompletedTask;
        }
    }

    public class DialogueTrackerUsageExample : MonoBehaviour
    {
        private DialogueGraphTracker _customTracker;

        private void Start()
        {
            // Create and activate a custom tracker
            _customTracker = new DialogueGraphLoggerTracker("[Dialogue Tracker]");
            
            // Set it as the active tracker
            DialogueGraphTracker.SetActiveTracker(_customTracker);
        }

        private void OnDestroy()
        {
            // Clean up when done
            _customTracker?.Dispose();
        }
    }
}

