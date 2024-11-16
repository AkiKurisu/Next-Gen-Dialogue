using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.IO;
using Ceres.Editor;
using UnityEditor.Callbacks;
namespace Kurisu.NGDT.Editor
{
    public class DialogueEditorWindow : EditorWindow, IHasCustomMenu
    {
        // GraphView window per UObject
        private static readonly Dictionary<int, DialogueEditorWindow> cache = new();
        private DialogueTreeView graphView;
        public DialogueTreeView GraphView => graphView;
        private UnityEngine.Object Key { get; set; }
        private InfoView infoView;
        private const string TreeName = "Dialogue Tree";
        private const string InfoText = "Welcome to Next-Gen Dialogue Tree Editor!";
        private static NextGenDialogueSetting setting;
        private string bakeGenerateText;
        private Vector2 scrollPosition;
        private IMGUIContainer previewContainer;
        private static NextGenDialogueSetting Setting
        {
            get
            {
                if (setting == null) setting = NextGenDialogueSetting.GetOrCreateSettings();
                return setting;
            }
        }
#pragma warning disable IDE0051
        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceId, int _)
        {
            if (cache.ContainsKey(instanceId))
            {
                cache[instanceId].Show();
                cache[instanceId].Focus();
                return true;
            }
            var asset = EditorUtility.InstanceIDToObject(instanceId);
            if (asset.GetType() == typeof(NextGenDialogueAsset))
            {
                Show((IDialogueContainer)asset);
                return true;
            }
            return false;
        }
#pragma warning restore IDE0051
        [MenuItem("Tools/Next Gen Dialogue/Next Gen Dialogue Editor")]
        private static void ShowEditorWindow()
        {
            string path = EditorUtility.SaveFilePanel("Select DialogueTreeSO save path", Application.dataPath, "DialogueTreeSO", "asset");
            if (string.IsNullOrEmpty(path)) return;
            path = path.Replace(Application.dataPath, string.Empty);
            var treeSO = CreateInstance<NextGenDialogueAsset>();
            AssetDatabase.CreateAsset(treeSO, $"Assets/{path}");
            AssetDatabase.SaveAssets();
            Show(treeSO);
        }
        public static bool ContainsEditorWindow(int windowInstanceID)
        {
            foreach (var value in cache.Values)
            {
                if (value.GetInstanceID() == windowInstanceID) return true;
            }
            return false;
        }
        public static void Show(IDialogueContainer bt)
        {
            var window = Create<DialogueEditorWindow>(bt);
            window.Show();
            window.Focus();
        }
        private DialogueTreeView CreateView(IDialogueContainer behaviorTree)
        {
            return new DialogueTreeView(behaviorTree, this);
        }
        private static T Create<T>(IDialogueContainer bt) where T : DialogueEditorWindow
        {

            var key = bt.Object.GetHashCode();
            if (cache.ContainsKey(key))
            {
                return (T)cache[key];
            }
            var window = CreateInstance<T>();
            StructGraphView(window, bt);
            window.titleContent = new GUIContent($"NGDT ({bt.Object.name})");
            window.Key = bt.Object;
            cache[key] = window;
            return window;
        }
        private static void StructGraphView(DialogueEditorWindow window, IDialogueContainer behaviorTree)
        {
            window.rootVisualElement.Clear();
            window.graphView = window.CreateView(behaviorTree);
            window.infoView = new InfoView(InfoText);
            window.infoView.styleSheets.Add(Resources.Load<StyleSheet>("NGDT/Info"));
            window.graphView.Add(window.infoView);
            window.graphView.OnSelectAction = window.OnNodeSelectionChange;
            GenerateBlackBoard(window.graphView);
            window.graphView.Restore();
            window.rootVisualElement.Add(window.CreateToolBar(window.graphView));
            window.rootVisualElement.Add(window.graphView);
            window.rootVisualElement.Add(window.CreateBakePreview());
        }

        private static void GenerateBlackBoard(DialogueTreeView graphView)
        {
            var blackboard = new DialogueBlackboard(graphView, graphView);
            blackboard.SetPosition(new Rect(10, 100, 300, 400));
            graphView.Add(blackboard);
            graphView.Blackboard = blackboard;
        }
        private void SaveDataToAsset(string path)
        {
            var treeAsset = CreateInstance<NextGenDialogueAsset>();
            if (!graphView.Validate())
            {
                Debug.LogWarning($"<color=#ff2f2f>NGDT</color> : Save failed, ScriptableObject wasn't created !\n{DateTime.Now}");
                return;
            }
            graphView.Commit(treeAsset);
            AssetDatabase.CreateAsset(treeAsset, $"Assets/{path}/{Key.name}.asset");
            AssetDatabase.SaveAssets();
            Debug.Log($"<color=#3aff48>NGDT</color> : Save succeed, ScriptableObject created path : {path}/{Key.name}.asset\n{DateTime.Now}");
        }

        private void OnDestroy()
        {
            int code;
            if (Key != null && cache.ContainsKey(code = Key.GetHashCode()))
            {
                if (Setting.AutoSave && !Application.isPlaying)
                {
                    if (!cache[code].graphView.Save())
                    {
                        var msg = "Auto save failed, do you want to discard change ?";
                        if (EditorUtility.DisplayDialog("Warning", msg, "Cancel", "Discard"))
                        {
                            var newWindow = cache[code] = Clone();
                            newWindow.Show();
                        }
                        else
                        {
                            cache.Remove(code);
                        }
                        return;
                    }
                    Debug.Log($"<color=#3aff48>NGDT</color>[{graphView.DialogueTree.Object.name}] saved succeed ! {DateTime.Now}");
                }
                cache.Remove(code);
            }
        }
        private DialogueEditorWindow Clone()
        {
            var newWindow = Instantiate(this);
            newWindow.rootVisualElement.Clear();
            newWindow.rootVisualElement.Add(newWindow.CreateToolBar(graphView));
            newWindow.graphView = graphView;
            newWindow.rootVisualElement.Add(graphView);
            newWindow.rootVisualElement.Add(newWindow.CreateBakePreview());
            graphView.OnSelectAction = newWindow.OnNodeSelectionChange;
            graphView.EditorWindow = newWindow;
            newWindow.Key = Key;
            return newWindow;
        }
        private void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            switch (playModeStateChange)
            {
                case PlayModeStateChange.EnteredEditMode:
                    Reload();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    Reload();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playModeStateChange), playModeStateChange, null);
            }
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Reload();
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void Reload()
        {
            if (Key != null)
            {
                if (Key is GameObject) StructGraphView(this, (Key as GameObject).GetComponent<IDialogueContainer>());
                else StructGraphView(this, Key as IDialogueContainer);
                Repaint();
            }
        }
        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Reload"), false, Reload);
        }
        private VisualElement CreateToolBar(DialogueTreeView graphView)
        {
            return new IMGUIContainer(
                () =>
                {
                    GUILayout.BeginHorizontal(EditorStyles.toolbar);

                    GUI.enabled = !Application.isPlaying;
                    if (GUILayout.Button($"Save {TreeName}", EditorStyles.toolbarButton))
                    {
                        var guiContent = new GUIContent();
                        if (graphView.Save())
                        {
                            guiContent.text = $"Update {TreeName} Succeed !";
                            ShowNotification(guiContent);
                        }
                        else
                        {
                            guiContent.text = $"Invalid {TreeName}, please check the node connection for errors !";
                            ShowNotification(guiContent);
                        }
                    }
                    GUI.enabled = true;
                    bool newValue = GUILayout.Toggle(Setting.AutoSave, "Auto Save", EditorStyles.toolbarButton);
                    if (newValue != Setting.AutoSave)
                    {
                        Setting.AutoSave = newValue;
                        EditorUtility.SetDirty(setting);
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
                            EditorUtility.SetDirty(setting);
                            AssetDatabase.SaveAssets();
                            ShowNotification(new GUIContent("Data dropped succeed !"));
                            graphView.CopyFromOtherTree(data, new Vector3(400, 300));
                        }
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Save to Json", EditorStyles.toolbarButton))
                    {
                        string path = EditorUtility.SaveFilePanel("Select json file save path", Setting.LastPath, graphView.DialogueTree.Object.name, "json");
                        if (!string.IsNullOrEmpty(path))
                        {
                            var serializedData = graphView.SerializeTreeToJson();
                            FileInfo info = new(path);
                            Setting.LastPath = info.Directory.FullName;
                            EditorUtility.SetDirty(setting);
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
                            Setting.LastPath = info.Directory.FullName;
                            EditorUtility.SetDirty(setting);
                            AssetDatabase.SaveAssets();
                            var data = File.ReadAllText(path);
                            if (graphView.CopyFromJson(data, new Vector3(400, 300)))
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
            previewContainer = new IMGUIContainer(() =>
            {
                if (graphView.selection.Count > 0 && !string.IsNullOrEmpty(bakeGenerateText))
                {
                    GUILayout.Label("Bake Preview");
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                    EditorGUILayout.TextArea(
                        bakeGenerateText,
                        new GUIStyle("TextField") { wordWrap = true, richText = true }
                    );
                    GUILayout.EndScrollView();
                }
            });
            previewContainer.style.maxHeight = 100;
            return previewContainer;
        }
        private bool TryBake(out string generateText)
        {
            var containers = graphView.selection.OfType<ContainerNode>().ToList();
            generateText = null;
            if (containers.Count < 2) return false;
            var bakeContainer = containers.Last();
            if (bakeContainer.TryGetModuleNode<AIBakeModule>(out ModuleNode _))
            {
                containers.Remove(bakeContainer);
                generateText = new DialogueBaker().Preview(containers, bakeContainer);
                return true;
            }
            if (bakeContainer.TryGetModuleNode<NovelBakeModule>(out ModuleNode novelBakeModule))
            {
                generateText = new NovelBaker().Preview(containers, novelBakeModule, bakeContainer);
                return true;
            }
            return false;
        }
        private IDialogueContainer LoadDataFromFile(string path)
        {
            try
            {
                return AssetDatabase.LoadAssetAtPath<NextGenDialogueAsset>($"Assets/{path}");

            }
            catch
            {
                ShowNotification(new GUIContent($"Invalid Path: Assets/{path}, asset type must be inherited from NextGenDialogueTreeSO !"));
                return null;
            }
        }
        private void OnNodeSelectionChange(IDialogueNode node)
        {
            infoView.UpdateSelection(node);
            if (TryBake(out string content))
            {
                bakeGenerateText = content;
            }
            else
            {
                bakeGenerateText = null;
            }
        }
    }
}