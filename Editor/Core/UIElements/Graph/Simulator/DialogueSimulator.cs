using System;
using System.Collections.Generic;
using System.Linq;

namespace NextGenDialogue.Graph.Editor
{
    /// <summary>
    /// Represents the current state of the dialogue simulator
    /// </summary>
    public enum SimulatorState
    {
        NotStarted,
        Playing,
        WaitingForSelection,
        Ended,
        Error
    }

    /// <summary>
    /// Represents a dialogue option in the simulator
    /// </summary>
    public class SimulatorOption
    {
        public string Content { get; set; }
        
        public string TargetPieceID { get; set; }
        
        public OptionContainerView OptionView { get; set; }
    }

    /// <summary>
    /// Core logic for simulating dialogue playback in the editor
    /// </summary>
    public class DialogueSimulator
    {
        private readonly DialogueGraphView _graphView;
        
        private PieceContainerView _currentPiece;
        
        private SimulatorState _state = SimulatorState.NotStarted;
        
        private string _errorMessage;
        
        private readonly List<string> _visitedPieceIDs = new();

        public event Action OnStateChanged;

        public DialogueSimulator(DialogueGraphView graphView)
        {
            _graphView = graphView;
        }

        public SimulatorState State => _state;
        
        public string ErrorMessage => _errorMessage;
        
        public PieceContainerView CurrentPiece => _currentPiece;
        
        public IReadOnlyList<string> VisitedPieceIDs => _visitedPieceIDs;

        /// <summary>
        /// Get the current piece ID
        /// </summary>
        public string CurrentPieceID => _currentPiece?.GetPieceID() ?? string.Empty;

        /// <summary>
        /// Get the current dialogue content from ContentModule
        /// </summary>
        public string GetCurrentContent()
        {
            if (_currentPiece == null) return string.Empty;

            var contentModules = _currentPiece.GetModuleNodes<ContentModule>();
            if (contentModules.Length == 0) return "[No dialogue content]";

            var contents = new List<string>();
            foreach (var moduleView in contentModules)
            {
                var content = moduleView.GetSharedStringValue("content");
                if (!string.IsNullOrEmpty(content))
                {
                    contents.Add(content);
                }
            }

            return contents.Count > 0 ? string.Join("\n", contents) : "[No dialogue content]";
        }

        /// <summary>
        /// Get the current options from connected Option containers
        /// </summary>
        public List<SimulatorOption> GetCurrentOptions()
        {
            var options = new List<SimulatorOption>();
            if (_currentPiece == null) return options;

            var optionContainers = _currentPiece.GetConnectedOptionContainers();
            foreach (var optionView in optionContainers)
            {
                var option = new SimulatorOption
                {
                    OptionView = optionView,
                    Content = GetOptionContent(optionView),
                    TargetPieceID = GetOptionTargetID(optionView)
                };
                options.Add(option);
            }

            return options;
        }

        private string GetOptionContent(OptionContainerView optionView)
        {
            var contentModules = optionView.GetModuleNodes<ContentModule>();
            if (contentModules.Length == 0) return "[No option content]";

            var content = contentModules[0].GetSharedStringValue("content");
            return string.IsNullOrEmpty(content) ? "[No option content]" : content;
        }

        private string GetOptionTargetID(OptionContainerView optionView)
        {
            if (!optionView.TryGetModuleNode<TargetIDModule>(out var targetIDModule))
            {
                return string.Empty;
            }

            var pieceID = targetIDModule.GetFieldValue<PieceID>("targetID");
            return pieceID?.Name ?? string.Empty;
        }

        /// <summary>
        /// Get the next piece ID from NextPieceModule (for auto-progression)
        /// </summary>
        public string GetNextPieceID()
        {
            if (_currentPiece == null) return string.Empty;

            if (!_currentPiece.TryGetModuleNode<NextPieceModule>(out var nextPieceModule))
            {
                return string.Empty;
            }

            var pieceID = nextPieceModule.GetFieldValue<PieceID>("nextID");
            return pieceID?.Name ?? string.Empty;
        }

        /// <summary>
        /// Check if current piece has options
        /// </summary>
        public bool HasOptions()
        {
            return GetCurrentOptions().Count > 0;
        }

        /// <summary>
        /// Check if current piece has a next piece (auto-progression)
        /// </summary>
        public bool HasNextPiece()
        {
            return !string.IsNullOrEmpty(GetNextPieceID());
        }

        /// <summary>
        /// Start simulation from the first piece in the dialogue
        /// </summary>
        public void StartFromBeginning()
        {
            Reset();

            var dialogueView = _graphView.CollectNodes<DialogueContainerView>().FirstOrDefault();
            if (dialogueView == null)
            {
                SetError("No Dialogue container found in the graph");
                return;
            }

            var pieces = _graphView.CollectNodes<PieceContainerView>();
            if (pieces.Count == 0)
            {
                SetError("No Piece containers found in the graph");
                return;
            }

            // Find the first piece (the one that is a direct child of Dialogue)
            var firstPiece = FindFirstPiece(pieces);
            if (firstPiece == null)
            {
                SetError("Could not determine the first Piece in the dialogue");
                return;
            }

            NavigateToPiece(firstPiece);
        }

        private PieceContainerView FindFirstPiece(List<PieceContainerView> pieces)
        {
            // Try to find a piece that is not targeted by any option
            var targetedPieceIDs = new HashSet<string>();
            foreach (var piece in pieces)
            {
                var options = piece.GetConnectedOptionContainers();
                foreach (var option in options)
                {
                    var targetID = GetOptionTargetID(option);
                    if (!string.IsNullOrEmpty(targetID))
                    {
                        targetedPieceIDs.Add(targetID);
                    }
                }
            }

            // Also check NextPieceModule targets
            foreach (var piece in pieces)
            {
                if (piece.TryGetModuleNode<NextPieceModule>(out var nextPieceModule))
                {
                    var pieceID = nextPieceModule.GetFieldValue<PieceID>("nextID");
                    if (pieceID != null && !string.IsNullOrEmpty(pieceID.Name))
                    {
                        targetedPieceIDs.Add(pieceID.Name);
                    }
                }
            }

            // Find pieces that are not targeted (likely starting pieces)
            var startingPieces = pieces.Where(p => !targetedPieceIDs.Contains(p.GetPieceID())).ToList();
            
            // Return the first non-targeted piece, or just the first piece if all are targeted
            return startingPieces.FirstOrDefault() ?? pieces.FirstOrDefault();
        }

        /// <summary>
        /// Start simulation from a specific piece
        /// </summary>
        public void StartFromPiece(PieceContainerView piece)
        {
            Reset();
            NavigateToPiece(piece);
        }

        /// <summary>
        /// Start simulation from a piece with the given ID
        /// </summary>
        public void StartFromPieceID(string pieceID)
        {
            Reset();

            var piece = _graphView.FindPiece(pieceID);
            if (piece == null)
            {
                SetError($"Piece with ID '{pieceID}' not found");
                return;
            }

            NavigateToPiece(piece);
        }

        /// <summary>
        /// Select an option and navigate to its target piece
        /// </summary>
        public void SelectOption(SimulatorOption option)
        {
            if (_state != SimulatorState.WaitingForSelection && _state != SimulatorState.Playing)
            {
                return;
            }

            if (string.IsNullOrEmpty(option.TargetPieceID))
            {
                SetError("Selected option has no target Piece ID");
                return;
            }

            var targetPiece = _graphView.FindPiece(option.TargetPieceID);
            if (targetPiece == null)
            {
                SetError($"Target Piece '{option.TargetPieceID}' not found");
                return;
            }

            NavigateToPiece(targetPiece);
        }

        /// <summary>
        /// Continue to the next piece (when there are no options)
        /// </summary>
        public void Continue()
        {
            if (_state != SimulatorState.Playing)
            {
                return;
            }

            var nextPieceID = GetNextPieceID();
            if (string.IsNullOrEmpty(nextPieceID))
            {
                // No next piece and no options - dialogue ends
                SetState(SimulatorState.Ended);
                return;
            }

            var nextPiece = _graphView.FindPiece(nextPieceID);
            if (nextPiece == null)
            {
                SetError($"Next Piece '{nextPieceID}' not found");
                return;
            }

            NavigateToPiece(nextPiece);
        }

        /// <summary>
        /// Stop the simulation
        /// </summary>
        public void Stop()
        {
            ClearHighlight();
            Reset();
        }

        /// <summary>
        /// Reset the simulator state
        /// </summary>
        public void Reset()
        {
            ClearHighlight();
            _currentPiece = null;
            _errorMessage = null;
            _visitedPieceIDs.Clear();
            SetState(SimulatorState.NotStarted);
        }

        private void NavigateToPiece(PieceContainerView piece)
        {
            ClearHighlight();
            _currentPiece = piece;
            _visitedPieceIDs.Add(piece.GetPieceID());
            HighlightCurrentPiece();

            // Determine state based on options
            if (HasOptions())
            {
                SetState(SimulatorState.WaitingForSelection);
            }
            else
            {
                SetState(SimulatorState.Playing);
            }
        }

        private void HighlightCurrentPiece()
        {
            if (_currentPiece == null) return;
            _currentPiece.NodeElement.AddToClassList("simulator_active");
        }

        private void ClearHighlight()
        {
            if (_currentPiece == null) return;
            _currentPiece.NodeElement.RemoveFromClassList("simulator_active");
        }

        /// <summary>
        /// Clear all simulator highlights from the graph
        /// </summary>
        public void ClearAllHighlights()
        {
            var pieces = _graphView.CollectNodes<PieceContainerView>();
            foreach (var piece in pieces)
            {
                piece.NodeElement.RemoveFromClassList("simulator_active");
            }
        }

        private void SetState(SimulatorState state)
        {
            _state = state;
            OnStateChanged?.Invoke();
        }

        private void SetError(string message)
        {
            _errorMessage = message;
            SetState(SimulatorState.Error);
        }

        /// <summary>
        /// Frame the current piece in the graph view
        /// </summary>
        public void FrameCurrentPiece()
        {
            if (_currentPiece == null) return;
            _graphView.FrameSelection();
            _graphView.ClearSelection();
            _graphView.AddToSelection(_currentPiece);
            _graphView.FrameSelection();
        }
    }
}
