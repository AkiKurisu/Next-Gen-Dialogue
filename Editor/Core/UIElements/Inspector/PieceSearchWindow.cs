using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NextGenDialogue.Graph.Editor
{
    /// <summary>
    /// Search window for finding and selecting Piece nodes
    /// </summary>
    public class PieceSearchWindow : EditorWindow
    {
        private DialogueGraphView _graphView;
        
        private Action<string> _onPieceSelected;
        
        private string _currentSelection;
        
        private TextField _searchField;
        
        private ListView _listView;
        
        private List<string> _allPieceIds;
        
        private List<string> _filteredPieceIds;

        /// <summary>
        /// Show the search window
        /// </summary>
        /// <param name="graphView">Graph view to search in</param>
        /// <param name="currentSelection">Currently selected PieceID</param>
        /// <param name="onPieceSelected">Callback when a piece is selected</param>
        public static void ShowWindow(DialogueGraphView graphView, string currentSelection, Action<string> onPieceSelected)
        {
            var window = GetWindow<PieceSearchWindow>(true, "Search Pieces", true);
            window._graphView = graphView;
            window._currentSelection = currentSelection;
            window._onPieceSelected = onPieceSelected;
            window.minSize = new Vector2(300, 400);
            window.maxSize = new Vector2(500, 600);
            window.RefreshPieceList();
            window.Show();
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.paddingTop = 10;
            root.style.paddingBottom = 10;
            root.style.paddingLeft = 10;
            root.style.paddingRight = 10;

            // Search field
            var searchContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginBottom = 10
                }
            };

            _searchField = new TextField
            {
                style = { flexGrow = 1 }
            };
            _searchField.RegisterValueChangedCallback(OnSearchTextChanged);
            searchContainer.Add(_searchField);

            root.Add(searchContainer);

            // List view
            _filteredPieceIds = new List<string>();
            _listView = new ListView(_filteredPieceIds, 20, MakeItem, BindItem)
            {
                selectionType = SelectionType.Single,
                style =
                {
                    flexGrow = 1,
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopColor = new Color(0.2f, 0.2f, 0.2f),
                    borderBottomColor = new Color(0.2f, 0.2f, 0.2f),
                    borderLeftColor = new Color(0.2f, 0.2f, 0.2f),
                    borderRightColor = new Color(0.2f, 0.2f, 0.2f)
                }
            };
            _listView.selectionChanged += OnSelectionChanged;
            _listView.itemsChosen += OnItemsChosen;

            root.Add(_listView);

            // Buttons container
            var buttonsContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexEnd,
                    marginTop = 10
                }
            };

            var locateButton = new Button(OnLocateClicked)
            {
                text = "Locate in Graph",
                style = { marginRight = 5 }
            };
            buttonsContainer.Add(locateButton);

            var selectButton = new Button(OnSelectClicked)
            {
                text = "Select"
            };
            buttonsContainer.Add(selectButton);

            root.Add(buttonsContainer);
        }

        /// <summary>
        /// Refresh the list of PieceIDs from the graph
        /// </summary>
        private void RefreshPieceList()
        {
            if (_graphView?.SharedVariables == null)
            {
                _allPieceIds = new List<string>();
            }
            else
            {
                _allPieceIds = _graphView.SharedVariables
                    .Where(x => x.GetType() == typeof(PieceID))
                    .Select(v => v.Name)
                    .OrderBy(x => x)
                    .ToList();
            }

            FilterList(string.Empty);
        }

        /// <summary>
        /// Filter the list based on search text
        /// </summary>
        /// <param name="searchText">Search text</param>
        private void FilterList(string searchText)
        {
            _filteredPieceIds.Clear();

            if (string.IsNullOrEmpty(searchText))
            {
                _filteredPieceIds.AddRange(_allPieceIds);
            }
            else
            {
                var lowerSearch = searchText.ToLowerInvariant();
                _filteredPieceIds.AddRange(_allPieceIds.Where(id => 
                    id.ToLowerInvariant().Contains(lowerSearch)));
            }

            _listView?.RefreshItems();

            // Select current selection if it exists in filtered list
            if (!string.IsNullOrEmpty(_currentSelection))
            {
                var index = _filteredPieceIds.IndexOf(_currentSelection);
                if (index >= 0)
                {
                    _listView?.SetSelection(index);
                }
            }
        }

        /// <summary>
        /// Create list item visual element
        /// </summary>
        private VisualElement MakeItem()
        {
            var label = new Label
            {
                style =
                {
                    paddingLeft = 5,
                    paddingTop = 2,
                    paddingBottom = 2,
                    unityTextAlign = TextAnchor.MiddleLeft
                }
            };
            return label;
        }

        /// <summary>
        /// Bind data to list item
        /// </summary>
        private void BindItem(VisualElement element, int index)
        {
            if (element is Label label && index >= 0 && index < _filteredPieceIds.Count)
            {
                label.text = _filteredPieceIds[index];
            }
        }

        /// <summary>
        /// Handle search text change
        /// </summary>
        private void OnSearchTextChanged(ChangeEvent<string> evt)
        {
            FilterList(evt.newValue);
        }

        /// <summary>
        /// Handle selection change in list
        /// </summary>
        private void OnSelectionChanged(IEnumerable<object> selectedItems)
        {
            var selected = selectedItems.FirstOrDefault() as string;
            if (!string.IsNullOrEmpty(selected))
            {
                _currentSelection = selected;
            }
        }

        /// <summary>
        /// Handle double-click on item (select and close)
        /// </summary>
        private void OnItemsChosen(IEnumerable<object> chosenItems)
        {
            var chosen = chosenItems.FirstOrDefault() as string;
            if (!string.IsNullOrEmpty(chosen))
            {
                _onPieceSelected?.Invoke(chosen);
                Close();
            }
        }

        /// <summary>
        /// Handle locate button click
        /// </summary>
        private void OnLocateClicked()
        {
            if (string.IsNullOrEmpty(_currentSelection) || _graphView == null) return;

            var piece = _graphView.nodes.OfType<PieceContainerView>()
                .FirstOrDefault(view => view.GetPieceID() == _currentSelection);
            
            if (piece != null)
            {
                _graphView.ClearSelection();
                _graphView.AddToSelection(piece);
                _graphView.FrameSelection();
            }
        }

        /// <summary>
        /// Handle select button click
        /// </summary>
        private void OnSelectClicked()
        {
            if (!string.IsNullOrEmpty(_currentSelection))
            {
                _onPieceSelected?.Invoke(_currentSelection);
                Close();
            }
        }
    }
}
