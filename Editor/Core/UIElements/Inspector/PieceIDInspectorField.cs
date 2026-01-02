using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace NextGenDialogue.Graph.Editor
{
    /// <summary>
    /// Inspector-specific PieceID selection control with dropdown and search button
    /// </summary>
    public class PieceIDInspectorField : VisualElement
    {
        private readonly DropdownField _dropdown;

        private readonly DialogueGraphView _graphView;
        
        private readonly Action<PieceID> _onValueChanged;
        
        private PieceID _value;

        public PieceID Value
        {
            get => _value;
            set
            {
                _value = value ?? new PieceID();
                UpdateDropdownValue();
            }
        }

        public PieceIDInspectorField(string label, DialogueGraphView graphView, PieceID initialValue, Action<PieceID> onValueChanged)
        {
            _graphView = graphView ?? throw new ArgumentNullException(nameof(graphView));
            _onValueChanged = onValueChanged;
            _value = initialValue ?? new PieceID();

            AddToClassList("piece-id-inspector-field");

            // Create horizontal container for dropdown and search button
            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    minWidth = 0,
                    flexShrink = 1
                }
            };

            // Create dropdown
            var choices = GetPieceIDList();
            var currentIndex = choices.IndexOf(_value.Name ?? string.Empty);
            _dropdown = new DropdownField(label, choices, currentIndex >= 0 ? currentIndex : 0);
            _dropdown.AddToClassList("piece-id-dropdown");
            _dropdown.style.flexGrow = 1;
            _dropdown.style.minWidth = 0;
            _dropdown.style.flexShrink = 1;

            // Update choices when mouse enters (to catch new PieceIDs)
            _dropdown.RegisterCallback<MouseEnterEvent>(_ =>
            {
                var newChoices = GetPieceIDList();
                _dropdown.choices = newChoices;
            });

            // Handle value change
            _dropdown.RegisterValueChangedCallback(evt =>
            {
                _value.Name = evt.newValue;
                _onValueChanged?.Invoke(_value);
            });

            container.Add(_dropdown);

            // Create search button
            var searchButton = new Button(OnSearchButtonClicked)
            {
                text = "..."
            };
            searchButton.AddToClassList("piece-id-search-button");
            searchButton.tooltip = "Search Pieces";

            container.Add(searchButton);

            Add(container);
        }

        /// <summary>
        /// Get list of all PieceIDs from the graph
        /// </summary>
        private List<string> GetPieceIDList()
        {
            if (_graphView?.SharedVariables == null)
            {
                return new List<string> { string.Empty };
            }

            var list = _graphView.SharedVariables
                .Where(x => x.GetType() == typeof(PieceID))
                .Select(v => v.Name)
                .ToList();

            // Add empty option at the beginning
            if (!list.Contains(string.Empty))
            {
                list.Insert(0, string.Empty);
            }

            return list;
        }

        /// <summary>
        /// Update dropdown value to match current PieceID
        /// </summary>
        private void UpdateDropdownValue()
        {
            if (_dropdown == null) return;

            var choices = GetPieceIDList();
            _dropdown.choices = choices;

            var index = choices.IndexOf(_value?.Name ?? string.Empty);
            if (index >= 0)
            {
                _dropdown.index = index;
            }
        }

        /// <summary>
        /// Handle search button click
        /// </summary>
        private void OnSearchButtonClicked()
        {
            PieceSearchWindow.ShowWindow(_graphView, _value?.Name, OnPieceSelected);
        }

        /// <summary>
        /// Handle piece selection from search window
        /// </summary>
        /// <param name="pieceId">Selected PieceID name</param>
        private void OnPieceSelected(string pieceId)
        {
            _value.Name = pieceId;
            UpdateDropdownValue();
            _onValueChanged?.Invoke(_value);
        }
    }
}
