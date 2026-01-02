using System;
using System.Linq;
using Ceres.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace NextGenDialogue.Graph.Editor
{
    /// <summary>
    /// Inspector panel for Dialogue Graph Editor
    /// Displays information about selected container nodes and allows editing their module properties
    /// </summary>
    public class DialogueGraphInspectorPanel
    {
        private readonly DialogueEditorWindow _editorWindow;

        private VisualElement _container;

        private ScrollView _content;

        private DialogueNodeInspector _currentInspector;

        private ContainerNodeView _currentContainerView;

        private Label _positionLabel;

        private const int DefaultFontSize = 12;

        private const int SectionFontSize = 14;

        public DialogueGraphInspectorPanel(DialogueEditorWindow editorWindow)
        {
            _editorWindow = editorWindow;
        }

        /// <summary>
        /// Create inspector panel UI
        /// </summary>
        /// <returns>Inspector panel container</returns>
        public VisualElement CreatePanel()
        {
            _container = new VisualElement
            {
                name = "InspectorPanel"
            };

            // Inspector header
            var header = new VisualElement
            {
                name = "InspectorHeader"
            };
            var headerLabel = new Label("Inspector");
            header.Add(headerLabel);

            _container.Add(header);

            // Inspector content area (scrollable)
            _content = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "InspectorContent"
            };

            // Placeholder content
            var contentLabel = new Label("Select a container node to inspect");
            _content.Add(contentLabel);

            _container.Add(_content);

            return _container;
        }

        /// <summary>
        /// Attach selection listener for inspector panel updates
        /// </summary>
        /// <param name="rootElement">Root visual element to attach scheduler</param>
        public void AttachSelectionListener(VisualElement rootElement)
        {
            // Use schedule to periodically check for selection changes
            rootElement.schedule.Execute(CheckAndUpdateSelection).Every(100); // Check every 100ms
        }

        /// <summary>
        /// Check if selection changed and update inspector if needed
        /// </summary>
        private void CheckAndUpdateSelection()
        {
            if (_editorWindow.GetGraphView() is not DialogueGraphView graphView || _content == null) return;

            ContainerNodeView selectedContainerView = null;
            var selection = graphView.selection.OfType<ContainerNodeView>().ToArray();
            if (selection.Length == 1)
            {
                selectedContainerView = selection[0];
            }

            // Check if selection changed
            if (selectedContainerView != _currentContainerView)
            {
                BuildInspectorContent();
            }
            else if (_currentContainerView != null)
            {
                UpdateInspectorContent();
            }
        }

        /// <summary>
        /// Build inspector content based on current selection
        /// </summary>
        private void BuildInspectorContent()
        {
            var graphView = _editorWindow.GetGraphView() as DialogueGraphView;
            if (_content == null || graphView == null) return;

            DestroyCurrentInspector();

            var selection = graphView.selection.OfType<ContainerNodeView>().ToArray();
            if (selection.Length == 0)
            {
                // No selection - show placeholder
                var label = new Label("Select a container node to inspect");
                _content.Add(label);
                return;
            }

            if (selection.Length > 1)
            {
                // Multiple selection
                var label = new Label($"Multiple containers selected ({selection.Length})");
                _content.Add(label);
                return;
            }

            DrawContainerInspector(selection[0]);
        }

        /// <summary>
        /// Update dynamic information that changes frequently (e.g., position)
        /// </summary>
        private void UpdateInspectorContent()
        {
            if (_currentContainerView == null || _positionLabel == null) return;

            var position = _currentContainerView.GetPosition();
            _positionLabel.text = $"Position: ({position.x:F1}, {position.y:F1})";
        }

        /// <summary>
        /// Display a container view inspector
        /// </summary>
        /// <param name="containerView">Container view to inspect</param>
        private void DrawContainerInspector(ContainerNodeView containerView)
        {
            _currentContainerView = containerView;
            _currentInspector = new DialogueNodeInspector(containerView);

            // Container information section
            AddSectionTitle("Container Information");

            // Container type
            var containerTypeName = containerView.NodeType.Name;
            if (containerView.NodeType.IsGenericType)
            {
                containerTypeName = containerView.NodeType.GetGenericTypeDefinition().Name;
            }
            AddInfoLabel($"Type: {containerTypeName}");

            // Container GUID
            AddInfoLabel($"GUID: {containerView.Guid}", new Color(0.7f, 0.7f, 0.7f));

            // Container position
            var position = containerView.GetPosition();
            _positionLabel = new Label($"Position: ({position.x:F1}, {position.y:F1})")
            {
                style =
                {
                    marginLeft = 10,
                    marginTop = 5,
                    fontSize = DefaultFontSize,
                    whiteSpace = WhiteSpace.Normal
                }
            };
            _content.Add(_positionLabel);

            // Description section
            DrawDescriptionSection(containerView);

            // Modules section
            DrawModulesSection();
        }

        /// <summary>
        /// Display node description section
        /// </summary>
        /// <param name="containerView">Container view to get description from</param>
        private void DrawDescriptionSection(ContainerNodeView containerView)
        {
            var description = NodeInfo.GetInfo(containerView.NodeType);
            
            // Skip if description is just the type name (no custom description)
            var typeName = containerView.NodeType.Name;
            if (containerView.NodeType.IsGenericType)
            {
                typeName = containerView.NodeType.GetGenericTypeDefinition().Name;
            }
            if (string.IsNullOrEmpty(description) || description == typeName)
            {
                return;
            }

            AddSectionTitle("Description", 15);

            // Create description container with background
            var descriptionContainer = new VisualElement
            {
                name = "DescriptionContainer"
            };
            descriptionContainer.AddToClassList("description-container");

            var descriptionLabel = new Label(description)
            {
                name = "DescriptionLabel"
            };
            descriptionLabel.AddToClassList("description-label");

            descriptionContainer.Add(descriptionLabel);
            _content.Add(descriptionContainer);
        }

        /// <summary>
        /// Display module description section
        /// </summary>
        /// <param name="moduleInfo">Module info to get description from</param>
        private void DrawModuleDescriptionSection(DialogueNodeInspector.ModuleInfo moduleInfo)
        {
            var description = NodeInfo.GetInfo(moduleInfo.ModuleType);
            
            // Skip if description is just the type name (no custom description)
            var typeName = moduleInfo.ModuleType.Name;
            if (moduleInfo.ModuleType.IsGenericType)
            {
                typeName = moduleInfo.ModuleType.GetGenericTypeDefinition().Name;
            }
            if (string.IsNullOrEmpty(description) || description == typeName)
            {
                return;
            }

            // Create description container with background (smaller margin for module descriptions)
            var descriptionContainer = new VisualElement
            {
                name = "ModuleDescriptionContainer"
            };
            descriptionContainer.AddToClassList("description-container");
            descriptionContainer.style.marginTop = 5;
            descriptionContainer.style.marginLeft = 10;
            descriptionContainer.style.marginRight = 10;

            var descriptionLabel = new Label(description)
            {
                name = "ModuleDescriptionLabel"
            };
            descriptionLabel.AddToClassList("description-label");

            descriptionContainer.Add(descriptionLabel);
            _content.Add(descriptionContainer);
        }

        /// <summary>
        /// Display modules section with hybrid UIElement/IMGUI rendering
        /// PieceID fields are rendered with UIElements, other fields with IMGUI
        /// </summary>
        private void DrawModulesSection()
        {
            AddSectionTitle("Modules", 15);

            var graphView = GetGraphView();
            if (graphView == null) return;

            try
            {
                var moduleInfos = _currentInspector.GetModuleInfos();
                
                if (moduleInfos.Count == 0)
                {
                    var noModulesLabel = new Label("No modules in this container")
                    {
                        style =
                        {
                            marginLeft = 10,
                            marginTop = 5,
                            color = new Color(0.7f, 0.7f, 0.7f)
                        }
                    };
                    _content.Add(noModulesLabel);
                    return;
                }

                foreach (var moduleInfo in moduleInfos)
                {
                    // Module header (UIElement)
                    AddModuleHeader(moduleInfo);

                    // Module description section
                    DrawModuleDescriptionSection(moduleInfo);

                    // PieceID fields (UIElement)
                    foreach (var pieceIdInfo in moduleInfo.GetPieceIdFields())
                    {
                        var field = new PieceIDInspectorField(
                            pieceIdInfo.FieldName,
                            graphView,
                            pieceIdInfo.CurrentValue,
                            newValue => pieceIdInfo.SetValue(newValue)
                        );
                        _content.Add(field);
                    }

                    // Other fields (IMGUI)
                    if (moduleInfo.HasNonPieceIdFields())
                    {
                        var imguiContainer = new IMGUIContainer(() =>
                            _currentInspector.DrawModuleFieldsWithoutPieceId(moduleInfo))
                        {
                            style =
                            {
                                marginLeft = 10,
                                marginRight = 10,
                                marginTop = 5,
                                marginBottom = 5
                            }
                        };
                        _content.Add(imguiContainer);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLabel = new Label($"Error creating inspector: {ex.Message}")
                {
                    style =
                    {
                        marginLeft = 10,
                        color = new Color(1f, 0.3f, 0.3f)
                    }
                };
                _content.Add(errorLabel);
            }
        }

        /// <summary>
        /// Add module header label
        /// </summary>
        /// <param name="moduleInfo">Module info</param>
        private void AddModuleHeader(DialogueNodeInspector.ModuleInfo moduleInfo)
        {
            var moduleName = CeresLabel.GetLabel(moduleInfo.ModuleType);
            var headerLabel = new Label(moduleName)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = DefaultFontSize,
                    marginTop = 10,
                    marginLeft = 10,
                    marginBottom = 3
                }
            };
            _content.Add(headerLabel);
        }

        /// <summary>
        /// Add section title label
        /// </summary>
        /// <param name="title">Title text</param>
        /// <param name="topMargin">Top margin</param>
        private void AddSectionTitle(string title, int topMargin = 10)
        {
            var titleLabel = new Label(title)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = SectionFontSize,
                    marginTop = topMargin,
                    marginLeft = 10,
                    marginBottom = 5
                }
            };
            _content.Add(titleLabel);
        }

        /// <summary>
        /// Add info label
        /// </summary>
        /// <param name="text">Label text</param>
        /// <param name="color">Text color (optional)</param>
        private void AddInfoLabel(string text, Color? color = null)
        {
            var label = new Label(text)
            {
                style =
                {
                    marginLeft = 10,
                    marginTop = 5,
                    fontSize = DefaultFontSize,
                    whiteSpace = WhiteSpace.Normal
                }
            };

            if (color.HasValue)
            {
                label.style.color = color.Value;
            }

            _content.Add(label);
        }

        /// <summary>
        /// Destroy current inspector if exists
        /// </summary>
        private void DestroyCurrentInspector()
        {
            if (_currentInspector != null)
            {
                _currentInspector.Dispose();
                _currentInspector = null;
            }
            _currentContainerView = null;
            _positionLabel = null;
            _content.Clear();
        }

        /// <summary>
        /// Get the graph view from the editor window
        /// </summary>
        public DialogueGraphView GetGraphView()
        {
            return _editorWindow.GetGraphView() as DialogueGraphView;
        }
    }
}

