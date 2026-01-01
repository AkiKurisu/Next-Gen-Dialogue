using UnityEngine;
using UnityEngine.UIElements;

namespace NextGenDialogue.Graph.Editor
{
    /// <summary>
    /// UI panel for the dialogue simulator
    /// </summary>
    public class DialogueSimulatorPanel : VisualElement
    {
        private readonly DialogueSimulator _simulator;

        private readonly Label _statusLabel;
        
        private readonly Label _pieceIDLabel;
        
        private readonly Label _contentLabel;
        
        private readonly VisualElement _optionsContainer;
        
        private Button _playButton;
        
        private Button _stopButton;
        
        private Button _continueButton;

        private const string PanelStyleClass = "dialogue-simulator-panel";
        
        private const string HeaderStyleClass = "dialogue-simulator-header";
        
        private const string ContentAreaStyleClass = "dialogue-simulator-content-area";
        
        private const string OptionsAreaStyleClass = "dialogue-simulator-options-area";
        
        private const string ControlsStyleClass = "dialogue-simulator-controls";
        
        private const string OptionButtonStyleClass = "dialogue-simulator-option-button";
        
        private const string StatusLabelStyleClass = "dialogue-simulator-status";

        public DialogueSimulatorPanel(DialogueGraphView graphView)
        {
            _simulator = new DialogueSimulator(graphView);
            _simulator.OnStateChanged += OnSimulatorStateChanged;

            AddToClassList(PanelStyleClass);
            
            // Apply inline styles
            ApplyPanelStyles();

            // Header
            var header = CreateHeader();
            Add(header);

            // Status bar
            var statusBar = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceBetween,
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 4,
                    paddingBottom = 4,
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f)
                }
            };

            _statusLabel = new Label("Not Started");
            _statusLabel.AddToClassList(StatusLabelStyleClass);
            _statusLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            statusBar.Add(_statusLabel);

            _pieceIDLabel = new Label("Piece: -")
            {
                style =
                {
                    color = new Color(0.6f, 0.8f, 1f, 1f)
                }
            };
            statusBar.Add(_pieceIDLabel);

            Add(statusBar);

            // Content area with scroll view
            var contentScrollView = new ScrollView(ScrollViewMode.Vertical);
            contentScrollView.AddToClassList(ContentAreaStyleClass);
            contentScrollView.style.flexGrow = 1;
            contentScrollView.style.minHeight = 80;
            contentScrollView.style.maxHeight = 200;
            contentScrollView.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f, 1f);
            contentScrollView.style.marginLeft = 8;
            contentScrollView.style.marginRight = 8;
            contentScrollView.style.marginTop = 8;
            contentScrollView.style.marginBottom = 8;
            contentScrollView.style.paddingLeft = 8;
            contentScrollView.style.paddingRight = 8;
            contentScrollView.style.paddingTop = 8;
            contentScrollView.style.paddingBottom = 8;
            contentScrollView.style.borderTopLeftRadius = 4;
            contentScrollView.style.borderTopRightRadius = 4;
            contentScrollView.style.borderBottomLeftRadius = 4;
            contentScrollView.style.borderBottomRightRadius = 4;

            _contentLabel = new Label("Click 'Play' to start preview or right-click a Piece node and select 'Preview from here'")
                {
                    style =
                    {
                        whiteSpace = WhiteSpace.Normal,
                        color = new Color(0.8f, 0.8f, 0.8f, 1f),
                        fontSize = 13
                    }
                };
            contentScrollView.Add(_contentLabel);

            Add(contentScrollView);

            // Options area
            _optionsContainer = new VisualElement();
            _optionsContainer.AddToClassList(OptionsAreaStyleClass);
            _optionsContainer.style.marginLeft = 8;
            _optionsContainer.style.marginRight = 8;
            _optionsContainer.style.marginBottom = 8;
            Add(_optionsContainer);

            // Controls
            var controlsContainer = CreateControls();
            Add(controlsContainer);

            UpdateUI();
        }

        private void ApplyPanelStyles()
        {
            style.backgroundColor = new Color(0.22f, 0.22f, 0.22f, 1f);
            style.borderTopWidth = 1;
            style.borderTopColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            style.minHeight = 200;
        }

        private VisualElement CreateHeader()
        {
            var header = new VisualElement();
            header.AddToClassList(HeaderStyleClass);
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            header.style.paddingLeft = 8;
            header.style.paddingRight = 8;
            header.style.paddingTop = 6;
            header.style.paddingBottom = 6;
            header.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1f);

            var titleLabel = new Label("Dialogue Simulator")
            {
                style =
                {
                    fontSize = 14,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    color = new Color(0.9f, 0.9f, 0.9f, 1f)
                }
            };
            header.Add(titleLabel);

            var frameButton = new Button(() => _simulator.FrameCurrentPiece())
            {
                text = "Frame",
                style =
                {
                    width = 50,
                    height = 20
                }
            };
            header.Add(frameButton);

            return header;
        }

        private VisualElement CreateControls()
        {
            var controls = new VisualElement();
            controls.AddToClassList(ControlsStyleClass);
            controls.style.flexDirection = FlexDirection.Row;
            controls.style.justifyContent = Justify.Center;
            controls.style.paddingLeft = 8;
            controls.style.paddingRight = 8;
            controls.style.paddingTop = 8;
            controls.style.paddingBottom = 8;
            controls.style.borderTopWidth = 1;
            controls.style.borderTopColor = new Color(0.15f, 0.15f, 0.15f, 1f);

            _playButton = new Button(OnPlayClicked)
            {
                text = "Play",
                style =
                {
                    width = 60,
                    height = 24,
                    marginRight = 8
                }
            };
            controls.Add(_playButton);

            _stopButton = new Button(OnStopClicked)
            {
                text = "Stop",
                style =
                {
                    width = 60,
                    height = 24,
                    marginRight = 8
                }
            };
            controls.Add(_stopButton);

            _continueButton = new Button(OnContinueClicked)
            {
                text = "Continue",
                style =
                {
                    width = 70,
                    height = 24
                }
            };
            controls.Add(_continueButton);

            return controls;
        }

        private void OnPlayClicked()
        {
            _simulator.StartFromBeginning();
        }

        private void OnStopClicked()
        {
            _simulator.Stop();
        }

        private void OnContinueClicked()
        {
            _simulator.Continue();
        }

        private void OnSimulatorStateChanged()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            UpdateStatusLabel();
            UpdatePieceIDLabel();
            UpdateContentLabel();
            UpdateOptions();
            UpdateControlButtons();
        }

        private void UpdateStatusLabel()
        {
            switch (_simulator.State)
            {
                case SimulatorState.NotStarted:
                    _statusLabel.text = "Not Started";
                    _statusLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
                    break;
                case SimulatorState.Playing:
                    _statusLabel.text = "Playing";
                    _statusLabel.style.color = new Color(0.4f, 0.8f, 0.4f, 1f);
                    break;
                case SimulatorState.WaitingForSelection:
                    _statusLabel.text = "Select an Option";
                    _statusLabel.style.color = new Color(1f, 0.8f, 0.3f, 1f);
                    break;
                case SimulatorState.Ended:
                    _statusLabel.text = "Dialogue Ended";
                    _statusLabel.style.color = new Color(0.6f, 0.6f, 1f, 1f);
                    break;
                case SimulatorState.Error:
                    _statusLabel.text = "Error";
                    _statusLabel.style.color = new Color(1f, 0.4f, 0.4f, 1f);
                    break;
            }
        }

        private void UpdatePieceIDLabel()
        {
            var pieceID = _simulator.CurrentPieceID;
            _pieceIDLabel.text = string.IsNullOrEmpty(pieceID) ? "Piece: -" : $"Piece: {pieceID}";
        }

        private void UpdateContentLabel()
        {
            switch (_simulator.State)
            {
                case SimulatorState.NotStarted:
                    _contentLabel.text = "Click 'Play' to start preview or right-click a Piece node and select 'Preview from here'";
                    break;
                case SimulatorState.Playing:
                case SimulatorState.WaitingForSelection:
                    _contentLabel.text = _simulator.GetCurrentContent();
                    break;
                case SimulatorState.Ended:
                    _contentLabel.text = "The dialogue has ended.\n\nClick 'Play' to restart from the beginning.";
                    break;
                case SimulatorState.Error:
                    _contentLabel.text = $"Error: {_simulator.ErrorMessage}";
                    break;
            }
        }

        private void UpdateOptions()
        {
            _optionsContainer.Clear();

            if (_simulator.State != SimulatorState.WaitingForSelection && 
                _simulator.State != SimulatorState.Playing)
            {
                return;
            }

            var options = _simulator.GetCurrentOptions();
            if (options.Count == 0)
            {
                return;
            }

            for (int i = 0; i < options.Count; i++)
            {
                var option = options[i];
                var optionButton = CreateOptionButton(i + 1, option);
                _optionsContainer.Add(optionButton);
            }
        }

        private Button CreateOptionButton(int index, SimulatorOption option)
        {
            var button = new Button(() => OnOptionSelected(option))
            {
                text = $"{index}. {option.Content}"
            };
            button.AddToClassList(OptionButtonStyleClass);
            button.style.marginBottom = 4;
            button.style.paddingLeft = 8;
            button.style.paddingRight = 8;
            button.style.paddingTop = 6;
            button.style.paddingBottom = 6;
            button.style.unityTextAlign = TextAnchor.MiddleLeft;
            button.style.backgroundColor = new Color(0.3f, 0.3f, 0.35f, 1f);
            button.style.borderTopLeftRadius = 4;
            button.style.borderTopRightRadius = 4;
            button.style.borderBottomLeftRadius = 4;
            button.style.borderBottomRightRadius = 4;

            if (string.IsNullOrEmpty(option.TargetPieceID))
            {
                button.style.backgroundColor = new Color(0.4f, 0.3f, 0.3f, 1f);
                button.tooltip = "Warning: This option has no target Piece ID";
            }
            else
            {
                button.tooltip = $"Go to: {option.TargetPieceID}";
            }

            return button;
        }

        private void OnOptionSelected(SimulatorOption option)
        {
            _simulator.SelectOption(option);
        }

        private void UpdateControlButtons()
        {
            var isNotStartedOrEnded = _simulator.State == SimulatorState.NotStarted || 
                                       _simulator.State == SimulatorState.Ended ||
                                       _simulator.State == SimulatorState.Error;
            var isPlaying = _simulator.State == SimulatorState.Playing;
            var hasNoOptions = !_simulator.HasOptions();

            _playButton.SetEnabled(isNotStartedOrEnded);
            _stopButton.SetEnabled(!isNotStartedOrEnded);
            _continueButton.SetEnabled(isPlaying && hasNoOptions);
        }

        /// <summary>
        /// Start simulation from a specific piece (called from context menu)
        /// </summary>
        public void StartFromPiece(PieceContainerView piece)
        {
            _simulator.StartFromPiece(piece);
        }

        /// <summary>
        /// Clean up when panel is removed
        /// </summary>
        public void Dispose()
        {
            _simulator.OnStateChanged -= OnSimulatorStateChanged;
            _simulator.ClearAllHighlights();
        }
    }
}
