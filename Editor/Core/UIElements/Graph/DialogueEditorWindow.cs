using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.IO;
using Ceres.Editor.Graph;
using UnityEditor.Callbacks;
namespace NextGenDialogue.Graph.Editor
{
    public class DialogueEditorWindow : CeresGraphEditorWindow<IDialogueGraphContainer, DialogueEditorWindow>
    {
        private DialogueGraphView _graphView;
        
        private static NextGenDialogueSettings _setting;
        
        private string _bakeGenerateText;
        
        private Vector2 _scrollPosition;
        
        private IMGUIContainer _previewContainer;
        
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
        
        [MenuItem("Tools/Next Gen Dialogue/Next Gen Dialogue Editor")]
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
                    titleContent = new GUIContent($"NGDT ({Identifier.boundObject.name})");
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
            if (_graphView.IsDirty())
            {
                titleContent.text = $"NGDT ({Identifier.boundObject.name})*";
            }
            else
            {
                titleContent.text = $"NGDT ({Identifier.boundObject.name})";
            }
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
            _graphView.Restore();
            rootVisualElement.Add(CreateToolBar(_graphView));
            rootVisualElement.Add(_graphView);
            rootVisualElement.Add(CreateBakePreview());
        }
        private void SaveDataToAsset(string path)
        {
            var treeAsset = CreateInstance<NextGenDialogueGraphAsset>();
            if (!_graphView.Validate())
            {
                Debug.LogWarning($"<color=#ff2f2f>NGDT</color>: Save failed, ScriptableObject wasn't created!\n{DateTime.Now}");
                return;
            }
            _graphView.Commit(treeAsset);
            AssetDatabase.CreateAsset(treeAsset, $"Assets/{path}/{Identifier.boundObject.name}.asset");
            AssetDatabase.SaveAssets();
            Debug.Log($"<color=#3aff48>NGDT</color>: Save succeed, ScriptableObject created path: {path}/{Identifier.boundObject.name}.asset\n{DateTime.Now}");
        }

        protected override void OnDisable()
        {
            if (!Identifier.IsValid()) return;
            
            if (Setting.AutoSave && !Application.isPlaying && _graphView.IsDirty())
            {
                if (!_graphView.Save())
                {
                    const string msg = "Auto save failed, do you want to discard change?";
                    if (EditorUtility.DisplayDialog("Warning", msg, "Cancel", "Discard"))
                    {
                        var newWindow = Clone();
                        EditorWindowRegistry.Register(GetContainer(), newWindow);
                        newWindow.Show();
                    }
                    return;
                }
                Debug.Log($"<color=#3aff48>NGDT</color>[{_graphView.DialogueGraphContainer.Object.name}] saved succeed, {DateTime.Now}");
            }
            
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

                    GUI.enabled = !Application.isPlaying;
                    var image  = EditorGUIUtility.IconContent("SaveAs@2x").image;
                    if (GUILayout.Button(new GUIContent(image,$"Save {Identifier.boundObject.name}"), EditorStyles.toolbarButton))
                    {
                        var guiContent = new GUIContent();
                        if (graphView.Save())
                        {
                            _graphView.ClearDirty();
                            guiContent.text = $"Update {Identifier.boundObject.name} succeed !";
                            ShowNotification(guiContent);
                        }
                        else
                        {
                            guiContent.text = $"Failed to save {Identifier.boundObject.name}, please check the node connection for errors!";
                            ShowNotification(guiContent);
                        }
                    }
                    GUI.enabled = true;
                    bool newValue = GUILayout.Toggle(Setting.AutoSave, "Auto Save", EditorStyles.toolbarButton);
                    if (newValue != Setting.AutoSave)
                    {
                        Setting.AutoSave = newValue;
                        EditorUtility.SetDirty(_setting);
                        AssetDatabase.SaveAssets();
                    }
                    GUI.enabled = !Application.isPlaying;
                    if (GUILayout.Button("Save to Asset", EditorStyles.toolbarButton))
                    {
                        string path = EditorUtility.OpenFolderPanel("Select ScriptableObject save path", Setting.LastPath, "");
                        if (!string.IsNullOrEmpty(path))
                        {
                            Setting.LastPath = path;
                            SaveDataToAsset(path.Replace(Application.dataPath, string.Empty));
                        }

                    }
                    if (GUILayout.Button("Copy from Asset", EditorStyles.toolbarButton))
                    {
                        string path = EditorUtility.OpenFilePanel("Select ScriptableObject to copy", Setting.LastPath, "asset");
                        var data = LoadDataFromFile(path.Replace(Application.dataPath, string.Empty));
                        if (data != null)
                        {
                            Setting.LastPath = path;
                            EditorUtility.SetDirty(_setting);
                            AssetDatabase.SaveAssets();
                            ShowNotification(new GUIContent("Data dropped succeed !"));
                            graphView.DeserializeGraph(data.GetDialogueGraph(), new Vector3(400, 300));
                        }
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Save to Json", EditorStyles.toolbarButton))
                    {
                        string path = EditorUtility.SaveFilePanel("Select json file save path", Setting.LastPath, graphView.DialogueGraphContainer.Object.name, "json");
                        if (!string.IsNullOrEmpty(path))
                        {
                            var serializedData = graphView.SerializeGraph();
                            FileInfo info = new(path);
                            Setting.LastPath = info.Directory!.FullName;
                            EditorUtility.SetDirty(_setting);
                            File.WriteAllText(path, serializedData);
                            Debug.Log($"<color=#3aff48>NGDT</color>:Save json file succeed !");
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                        GUIUtility.ExitGUI();
                    }
                    if (GUILayout.Button("Copy from Json", EditorStyles.toolbarButton))
                    {
                        string path = EditorUtility.OpenFilePanel("Select json file to copy", Setting.LastPath, "json");
                        if (!string.IsNullOrEmpty(path))
                        {
                            FileInfo info = new(path);
                            Setting.LastPath = info.Directory!.FullName;
                            EditorUtility.SetDirty(_setting);
                            AssetDatabase.SaveAssets();
                            var data = File.ReadAllText(path);
                            if (graphView.DeserializeGraph(data, new Vector3(400, 300)))
                                ShowNotification(new GUIContent("Json file read Succeed !"));
                            else
                                ShowNotification(new GUIContent("Json file is in wrong format !"));
                        }
                        GUIUtility.ExitGUI();
                    }
                    GUI.enabled = true;
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
                        new GUIStyle("TextField") { wordWrap = true, richText = true }
                    );
                    GUILayout.EndScrollView();
                }
            });
            _previewContainer.style.maxHeight = 100;
            return _previewContainer;
        }
        
        private bool TryBake(out string generateText)
        {
            var containers = _graphView.selection.OfType<ContainerNodeView>().ToList();
            generateText = null;
            if (containers.Count < 2) return false;
            var bakeContainer = containers.Last();
            if (bakeContainer.TryGetModuleNode<AIBakeModule>(out ModuleNodeView _))
            {
                containers.Remove(bakeContainer);
                generateText = new DialogueBaker().Preview(containers, bakeContainer);
                return true;
            }
            if (bakeContainer.TryGetModuleNode<NovelBakeModule>(out ModuleNodeView novelBakeModule))
            {
                generateText = new NovelBaker().Preview(containers, novelBakeModule, bakeContainer);
                return true;
            }
            return false;
        }
        
        private IDialogueGraphContainer LoadDataFromFile(string path)
        {
            try
            {
                return AssetDatabase.LoadAssetAtPath<NextGenDialogueGraphAsset>($"Assets/{path}");

            }
            catch
            {
                ShowNotification(new GUIContent($"Invalid Path: Assets/{path}, asset type must be inherited from NextGenDialogueTreeSO !"));
                return null;
            }
        }
        
        private void OnNodeSelectionChange(IDialogueNodeView node)
        {
            _bakeGenerateText = TryBake(out var content) ? content : null;
        }
    }
}