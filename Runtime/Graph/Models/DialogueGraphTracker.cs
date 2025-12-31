using System;
using Cysharp.Threading.Tasks;

namespace NextGenDialogue.Graph
{
    /// <summary>
    /// Tracker for dialogue graph execution, useful for debugging and monitoring dialogue flow
    /// </summary>
    public class DialogueGraphTracker : IDisposable
    {
        public readonly struct TrackerAutoScope : IDisposable
        {
            private readonly DialogueGraphTracker _tracker;
            private readonly DialogueGraphTracker _cachedTracker;

            internal TrackerAutoScope(DialogueGraphTracker tracker)
            {
                _cachedTracker = GetActiveTracker();
                _tracker = tracker;
                SetActiveTracker(tracker);
            }

            public void Dispose()
            {
                _tracker.Dispose();
                if (!_cachedTracker._isDisposed)
                {
                    SetActiveTracker(_cachedTracker);
                }
            }
        }

        private static readonly DialogueGraphTracker Empty = new();

        private static DialogueGraphTracker _defaultTracker = Empty;

        private static DialogueGraphTracker Default
        {
            get
            {
                if (_defaultTracker == null || _defaultTracker._isDisposed)
                {
                    _defaultTracker = null;
                    return Empty;
                }
                return _defaultTracker;
            }
            set => _defaultTracker = value;
        }

        private static DialogueGraphTracker _activeTracker;

        private bool _isDisposed;

        protected DialogueGraphTracker()
        {
        }

        public bool IsValid() => !_isDisposed;

        /// <summary>
        /// Creates a helper struct for the scoped using blocks.
        /// </summary>
        /// <returns>IDisposable struct which calls Begin and End automatically.</returns>
        public TrackerAutoScope Auto()
        {
            return new TrackerAutoScope(this);
        }

        /// <summary>
        /// Get current active <see cref="DialogueGraphTracker"/>
        /// </summary>
        /// <returns></returns>
        public static DialogueGraphTracker GetActiveTracker()
        {
            return _activeTracker ?? Default;
        }

        /// <summary>
        /// Set default <see cref="DialogueGraphTracker"/>
        /// </summary>
        /// <param name="tracker"></param>
        internal static void SetDefaultTracker(DialogueGraphTracker tracker)
        {
            Default = tracker;
        }

        /// <summary>
        /// Set current active <see cref="DialogueGraphTracker"/>
        /// </summary>
        /// <param name="tracker"></param>
        public static void SetActiveTracker(DialogueGraphTracker tracker)
        {
            _activeTracker = tracker;
        }

        /// <summary>
        /// Called when dialogue graph updates (every frame during dialogue execution)
        /// </summary>
        public virtual UniTask OnDialogueUpdate(Root root)
        {
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// Called when transitioning to the next dialogue piece
        /// </summary>
        /// <param name="pieceId">The ID of the next piece</param>
        public virtual UniTask OnPieceTransition(string pieceId)
        {
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// Called when a dialogue node updates
        /// </summary>
        /// <param name="node">The dialogue node being updated</param>
        /// <param name="status">The status result of the update</param>
        public virtual UniTask OnNodeUpdate(DialogueNode node, Status status)
        {
            return UniTask.CompletedTask;
        }

        public virtual void Dispose()
        {
            if (_activeTracker == this)
            {
                _activeTracker = null;
            }
            _isDisposed = true;
        }
    }
}

