using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using Ceres.Editor.Graph;
using UnityEditor.Callbacks;

namespace NextGenDialogue.Graph.Editor
{
    public class DialogueEditorWindow : CeresGraphEditorWindow<IDialogueGraphContainer, DialogueEditorWindow>
    {
        private DialogueGraphView _graphView;
        
        private DialogueGraphInspectorPanel _inspectorPanel;
        
        private static NextGenDialogueSettings _setting;
        
        private string _bakeGenerateText;
        
        private Vector2 _scrollPosition;
        
        private IMGUIContainer _previewContainer;
        
        private DialogueSimulatorPanel _simulatorPanel;
        
        private bool _showSimulator;
        
        private const string SplitViewWidthPrefsKey = "NextGenDialogue_DialogueEditorWindow_SplitViewWidth";
        
        private const float DefaultSplitViewWidth = 400f;
        
        private static NextGenDialogueSettings Setting
        {
            get
            {
                if (!_setting) _setting = NextGenDialogueSettings.Get();
                return _setting;
            }
        }
        
#pragma warning disable IDE0051
        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceId, int _)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceId);
            if (asset is not NextGenDialogueGraphAsset dialogueAsset) return false;
            
            var window = GetOrCreateEditorWindow(dialogueAsset);
            window.Focus();
            window.Show();
            return false;
        }
#pragma warning restore IDE0051
        
        [MenuItem("Tools/Next-Gen Dialogue/Dialogue Editor")]
        private static void ShowEditorWindow()
        {
            string path = EditorUtility.SaveFilePanel("Select DialogueAsset save path", Application.dataPath, "DialogueTreeSO", "asset");
            if (string.IsNullOrEmpty(path)) return;
            path = path.Replace(Application.dataPath, string.Empty);
            var asset = CreateInstance<NextGenDialogueGraphAsset>();
            AssetDatabase.CreateAsset(asset, $"Assets/{path}");
            AssetDatabase.SaveAssets();
            Show(asset);
        }

        protected override void OnInitialize()
        {
            try
            {
                DisplayProgressBar("Initialize field factory", 0f);
                {
                    FieldResolverFactory.Get();
                }
                DisplayProgressBar("Initialize node view factory", 0.3f);
                {
                    NodeViewFactory.Get();
                }
                DisplayProgressBar("Construct graph view", 0.6f);
                {
                    StructGraphView();
                    titleContent = new GUIContent($"Dialogue ({Identifier.boundObject.name})");
                }
            }
            finally
            {
                ClearProgressBar();
            }
        }
        
        private void OnGUI()
        {
            if (_graphView == null) return;
            titleContent.text = _graphView.IsDirty() ? $"Dialogue ({Identifier.boundObject.name})*" : $"Dialogue ({Identifier.boundObject.name})";
        }
        
        private static void DisplayProgressBar(string stepTitle, float progress)
        {
            EditorUtility.DisplayProgressBar("Initialize Dialogue Graph Editor", stepTitle, progress);
        }
        
        private static void ClearProgressBar()
        {
            EditorUtility.ClearProgressBar();
        }
        
        private void StructGraphView()
        {
            rootVisualElement.Clear();
            _graphView = new DialogueGraphView(this)
            {
                OnSelectNode = OnNodeSelectionChange
            };
            _graphView.RestoreGraph();
            rootVisualElement.Add(CreateToolBar(_graphView));
            
            rootVisualElement.styleSheets.Add(CeresGraphView.GetOrLoadStyleSheet("NGDT/DialogueInspector"));
            var savedWidth = EditorPrefs.GetFloat(SplitViewWidthPrefsKey, DefaultSplitViewWidth);
            var splitView = new TwoPaneSplitView(1, savedWidth, TwoPaneSplitViewOrientation.Horizontal)
            {
                name = "GraphSplitView"
            };
            
            splitView.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                var currentWidth = splitView.fixedPane.resolvedStyle.width;
                if (currentWidth > 0)
                {
                    EditorPrefs.SetFloat(SplitViewWidthPrefsKey, currentWidth);
                }
            });

            var leftContainer = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Column
                }
            };
            leftContainer.Add(_graphView);
            
            // Create simulator panel (initially hidden)
            _simulatorPanel = new DialogueSimulatorPanel(_graphView)
            {
                style =
                {
                    display = _showSimulator ? DisplayStyle.Flex : DisplayStyle.None
                }
            };
            leftContainer.Add(_simulatorPanel);
            
            splitView.Add(leftContainer);
            
            // Right pane: Inspector panel
            _inspectorPanel = new DialogueGraphInspectorPanel(this);
            var inspectorContainer = _inspectorPanel.CreatePanel();
            splitView.Add(inspectorContainer);
            
            rootVisualElement.Add(splitView);
            rootVisualElement.Add(CreateBakePreview());
            
            // Setup selection listener for inspector
            _inspectorPanel.AttachSelectionListener(rootVisualElement);
        }

        protected override void OnDisable()
        {
            if (!Identifier.IsValid()) return;
            
            if (Setting.AutoSave && !Application.isPlaying && _graphView.IsDirty())
            {
                if (!_graphView.SerializeGraph())
                {
                    const string msg = "Auto save failed, do you want to discard change?";
                    if (EditorUtility.DisplayDialog("Warning", msg, "Cancel", "Discard"))
                    {
                        var newWindow = Clone();
                        EditorWindowRegistry.Register(Identifier, newWindow);
                        newWindow.Show();
                    }
                    return;
                }
                NextGenDialogueLogger.Log($"[{_graphView.DialogueGraphContainer.Object.name}] saved succeed, {DateTime.Now}");
            }
            
            // Dispose the simulator panel
            _simulatorPanel?.Dispose();
            
            // Dispose the editor tracker when window is closed
            _graphView?.DisposeTracker();
            
            base.OnDisable();
        }
        
        private DialogueEditorWindow Clone()
        {
            var newWindow = Instantiate(this);
            newWindow.rootVisualElement.Clear();
            newWindow.rootVisualElement.Add(newWindow.CreateToolBar(_graphView));
            newWindow._graphView = _graphView;
            newWindow.rootVisualElement.Add(_graphView);
            newWindow.rootVisualElement.Add(newWindow.CreateBakePreview());
            _graphView.OnSelectNode = newWindow.OnNodeSelectionChange;
            _graphView.EditorWindow = newWindow;
            newWindow.Identifier = Identifier;
            return newWindow;
        }

        protected override void OnReloadGraphView()
        {
            StructGraphView();
        }
        
        private VisualElement CreateToolBar(DialogueGraphView graphView)
        {
            return new IMGUIContainer(
                () =>
                {
                    GUILayout.BeginHorizontal(EditorStyles.toolbar);
                    
                    using (new EditorGUI.DisabledScope(Application.isPlaying))
                    {
                        var image = EditorGUIUtility.IconContent("SaveAs@2x").image;
                        if (GUILayout.Button(new GUIContent(image, $"Save {Identifier.boundObject.name}"),
                                EditorStyles.toolbarButton))
                        {
                            var guiContent = new GUIContent();
                            if (graphView.SerializeGraph())
                            {
                                _graphView.ClearDirty();
                                guiContent.text = $"Update {Identifier.boundObject.name} succeed !";
                                ShowNotification(guiContent);
                            }
                            else
                            {
                                guiContent.text =
                                    $"Failed to save {Identifier.boundObject.name}, please check the node connection for errors!";
                                ShowNotification(guiContent);
                            }
                        }
                    }

                    bool newValue = GUILayout.Toggle(Setting.AutoSave, "Auto Save", EditorStyles.toolbarButton);
                    if (newValue != Setting.AutoSave)
                    {
                        Setting.AutoSave = newValue;
                        EditorUtility.SetDirty(_setting);
                        AssetDatabase.SaveAssets();
                    }
                    
                    GUILayout.FlexibleSpace();
                    
                    // Simulator toggle button
                    var simulatorIcon = EditorGUIUtility.IconContent("d_Animation.Play").image;
                    bool newShowSimulator = GUILayout.Toggle(_showSimulator, new GUIContent(simulatorIcon, "Simulator"), EditorStyles.toolbarButton);
                    if (newShowSimulator != _showSimulator)
                    {
                        _showSimulator = newShowSimulator;
                        if (_simulatorPanel != null)
                        {
                            _simulatorPanel.style.display = _showSimulator ? DisplayStyle.Flex : DisplayStyle.None;
                        }
                    }
                    
                    GUILayout.EndHorizontal();
                }
            );
        }
        
        private VisualElement CreateBakePreview()
        {
            _previewContainer = new IMGUIContainer(() =>
            {
                if (_graphView.selection.Count > 0 && !string.IsNullOrEmpty(_bakeGenerateText))
                {
                    GUILayout.Label("Bake Preview");
                    _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
                    EditorGUILayout.TextArea(
                        _bakeGenerateText,
                        new GUIStyle("TextField")
                        {
                            wordWrap = true, 
                            richText = true
                        }
                    );
                    GUILayout.EndScrollView();
                }
            });
            _previewContainer.style.maxHeight = 100;
            return _previewContainer;
        }
        
        private bool TryGetGenerateContent(out string generateText)
        {
            var containers = _graphView.selection.OfType<ContainerNodeView>().ToList();
            generateText = null;
            if (containers.Count < 2) return false;
            var bakeContainer = containers.Last();
            if (bakeContainer.TryGetModuleNode<AIBakeModule>(out _))
            {
                containers.Remove(bakeContainer);
                generateText = new DialogueBaker().Preview(containers, bakeContainer);
                return true;
            }
            if (bakeContainer.TryGetModuleNode<NovelBakeModule>(out var novelBakeModule))
            {
                generateText = new NovelBaker().Preview(containers, novelBakeModule, bakeContainer);
                return true;
            }
            return false;
        }
        
        private void OnNodeSelectionChange(IDialogueNodeView node)
        {
            _bakeGenerateText = TryGetGenerateContent(out var content) ? content : null;
        }
        
        /// <summary>
        /// Start simulator preview from a specific piece node
        /// </summary>
        public void StartSimulatorFromPiece(PieceContainerView piece)
        {
            // Show simulator panel if hidden
            if (!_showSimulator)
            {
                _showSimulator = true;
                if (_simulatorPanel != null)
                {
                    _simulatorPanel.style.display = DisplayStyle.Flex;
                }
            }
            
            // Start simulation from the piece
            _simulatorPanel?.StartFromPiece(piece);
        }
        
        /// <summary>
        /// Get current editing graph view
        /// </summary>
        /// <returns>The dialogue graph view</returns>
        public CeresGraphView GetGraphView()
        {
            return _graphView;
        }
    }
}
