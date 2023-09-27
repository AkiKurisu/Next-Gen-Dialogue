using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System.IO;
namespace Kurisu.NGDT.Editor
{
    public class DialogueEditorWindow : EditorWindow
    {
        // GraphView window per GameObject
        private static readonly Dictionary<int, DialogueEditorWindow> cache = new();
        private DialogueTreeView graphView;
        public DialogueTreeView GraphView => graphView;
        private UnityEngine.Object Key { get; set; }
        private InfoView infoView;
        /// <summary>
        /// Identify current dialogue tree can be saved to ScriptableObject
        /// </summary>
        /// <returns></returns>
        private Type SOType => typeof(NextGenDialogueTreeSO);
        private const string TreeName = "Dialogue Tree";
        private const string InfoText = "Welcome to Next-Gen-Dialogue Node Editor!";
        private static NextGenDialogueSetting setting;
        private readonly AIDialogueBaker testBaker = new();
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
        [MenuItem("Tools/Next Gen Dialogue/Next Gen Dialogue Editor")]
        private static void ShowEditorWindow()
        {
            string path = EditorUtility.SaveFilePanel("Select DialogueTreeSO save path", Application.dataPath, "DialogueTreeSO", "asset");
            if (string.IsNullOrEmpty(path)) return;
            path = path.Replace(Application.dataPath, string.Empty);
            var treeSO = CreateInstance<NextGenDialogueTreeSO>();
            AssetDatabase.CreateAsset(treeSO, $"Assets/{path}");
            AssetDatabase.SaveAssets();
            Show(treeSO);
        }
        public static void Show(IDialogueTree bt)
        {
            var window = Create<DialogueEditorWindow>(bt);
            window.Show();
            window.Focus();
        }
        private DialogueTreeView CreateView(IDialogueTree behaviorTree)
        {
            return new DialogueTreeView(behaviorTree, this);
        }
        private static T Create<T>(IDialogueTree bt) where T : DialogueEditorWindow
        {

            var key = bt._Object.GetHashCode();
            if (cache.ContainsKey(key))
            {
                return (T)cache[key];
            }
            var window = CreateInstance<T>();
            StructGraphView(window, bt);
            window.titleContent = new GUIContent($"{window.graphView.TreeEditorName} ({bt._Object.name})");
            window.Key = bt._Object;
            cache[key] = window;
            return window;
        }
        private static void StructGraphView(DialogueEditorWindow window, IDialogueTree behaviorTree)
        {
            window.rootVisualElement.Clear();
            window.graphView = window.CreateView(behaviorTree);
            window.infoView = new InfoView(InfoText);
            window.infoView.styleSheets.Add(Resources.Load<StyleSheet>("NGDT/Info"));
            window.graphView.Add(window.infoView);
            window.graphView.OnSelectAction = window.OnNodeSelectionChange;//绑定委托
            GenerateBlackBoard(window.graphView);
            window.graphView.Restore();
            window.rootVisualElement.Add(window.CreateToolBar(window.graphView));
            window.rootVisualElement.Add(window.graphView);
            window.rootVisualElement.Add(window.CreateBakePreview());
        }

        private static void GenerateBlackBoard(DialogueTreeView _graphView)
        {
            var blackboard = new Blackboard(_graphView);
            blackboard.Add(new BlackboardSection { title = "Shared Variables" });
            blackboard.addItemRequested = _blackboard =>
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Int"), false, () => _graphView.AddExposedProperty(new SharedInt()));
                menu.AddItem(new GUIContent("Float"), false, () => _graphView.AddExposedProperty(new SharedFloat()));
                menu.AddItem(new GUIContent("Bool"), false, () => _graphView.AddExposedProperty(new SharedBool()));
                menu.AddItem(new GUIContent("Vector3"), false, () => _graphView.AddExposedProperty(new SharedVector3()));
                menu.AddItem(new GUIContent("String"), false, () => _graphView.AddExposedProperty(new SharedString()));
                menu.AddItem(new GUIContent("Object"), false, () => _graphView.AddExposedProperty(new SharedObject()));
                menu.ShowAsContext();
            };

            blackboard.editTextRequested = (_blackboard, element, newValue) =>
            {
                var oldPropertyName = ((BlackboardField)element).text;
                var index = _graphView.exposedProperties.FindIndex(x => x.Name == oldPropertyName);
                if (string.IsNullOrEmpty(newValue))
                {
                    blackboard.contentContainer.RemoveAt(index + 1);
                    _graphView.ExposedProperties.RemoveAt(index);
                    return;
                }
                if (_graphView.ExposedProperties.Any(x => x.Name == newValue))
                {
                    EditorUtility.DisplayDialog("Error", "A variable with the same name already exists !",
                        "OK");
                    return;
                }

                var targetIndex = _graphView.exposedProperties.FindIndex(x => x.Name == oldPropertyName);
                _graphView.ExposedProperties[targetIndex].Name = newValue;
                _graphView.NotifyEditSharedVariable(_graphView.ExposedProperties[targetIndex]);
                ((BlackboardField)element).text = newValue;
            };
            blackboard.SetPosition(new Rect(10, 100, 300, 400));
            _graphView.Add(blackboard);
            _graphView._blackboard = blackboard;
        }
        private void SaveDataToSO(string path)
        {
            var treeSO = CreateInstance(SOType);
            if (!graphView.Save())
            {
                Debug.LogWarning($"<color=#ff2f2f>{graphView.TreeEditorName}</color> : Save failed, ScriptableObject wasn't created !\n{System.DateTime.Now}");
                return;
            }
            graphView.Commit((IDialogueTree)treeSO);
            AssetDatabase.CreateAsset(treeSO, $"Assets/{path}/{Key.name}.asset");
            AssetDatabase.SaveAssets();
            Debug.Log($"<color=#3aff48>{graphView.TreeEditorName}</color> : Save succeed, ScriptableObject created path : {path}/{Key.name}.asset\n{System.DateTime.Now}");
        }

        private void OnDestroy()
        {
            int code = Key.GetHashCode();
            if (Key != null && cache.ContainsKey(code))
            {
                if (Setting.AutoSave)
                    cache[code].graphView.Save(true);
                cache.Remove(code);
            }
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
                if (Key is GameObject) StructGraphView(this, (Key as GameObject).GetComponent<IDialogueTree>());
                else StructGraphView(this, Key as IDialogueTree);
                Repaint();
            }
        }
        private VisualElement CreateToolBar(DialogueTreeView graphView)
        {
            return new IMGUIContainer(
                () =>
                {
                    GUILayout.BeginHorizontal(EditorStyles.toolbar);

                    if (!Application.isPlaying)
                    {
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
                        bool newValue = GUILayout.Toggle(Setting.AutoSave, "Auto Save", EditorStyles.toolbarButton);
                        if (newValue != Setting.AutoSave)
                        {
                            Setting.AutoSave = newValue;
                            EditorUtility.SetDirty(setting);
                            AssetDatabase.SaveAssets();
                        }
                        if (graphView.CanSaveToSO)
                        {
                            if (GUILayout.Button("Save To SO", EditorStyles.toolbarButton))
                            {
                                string path = EditorUtility.OpenFolderPanel("Select ScriptableObject save path", Setting.LastPath, "");
                                if (!string.IsNullOrEmpty(path))
                                {
                                    Setting.LastPath = path;
                                    SaveDataToSO(path.Replace(Application.dataPath, string.Empty));
                                }

                            }
                        }
                        if (GUILayout.Button("Copy From SO", EditorStyles.toolbarButton))
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
                        if (GUILayout.Button("Save To Json", EditorStyles.toolbarButton))
                        {
                            var serializedData = graphView.SerializeTreeToJson();
                            string path = EditorUtility.SaveFilePanel("Select json file save path", Setting.LastPath, graphView.BehaviorTree._Object.name, "json");
                            if (!string.IsNullOrEmpty(path))
                            {
                                FileInfo info = new(path);
                                Setting.LastPath = info.Directory.FullName;
                                EditorUtility.SetDirty(setting);
                                File.WriteAllText(path, serializedData);
                                Debug.Log($"<color=#3aff48>{GraphView.TreeEditorName}</color>:Save json file succeed !");
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
                            }
                        }
                        if (GUILayout.Button("Copy From Json", EditorStyles.toolbarButton))
                        {
                            string path = EditorUtility.OpenFilePanel("Select json file to copy", Setting.LastPath, "json");
                            if (!string.IsNullOrEmpty(path))
                            {
                                FileInfo info = new(path);
                                Setting.LastPath = info.Directory.FullName;
                                EditorUtility.SetDirty(setting);
                                AssetDatabase.SaveAssets();
                                var data = File.ReadAllText(path);
                                if (graphView.CopyFromJsonFile(data, new Vector3(400, 300)))
                                    ShowNotification(new GUIContent("Json file read Succeed !"));
                                else
                                    ShowNotification(new GUIContent("Json file is in wrong format !"));
                            }
                            GUIUtility.ExitGUI();
                        }
                        GUILayout.EndHorizontal();
                    }
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
            if (containers.Count == 0) return false;
            var bakeContainer = containers.Last();
            if (!bakeContainer.TryGetModuleNode<AIBakeModule>(out ModuleNode _)) return false;
            containers.Remove(bakeContainer);
            if (containers.Any(x => x.TryGetModuleNode<AIBakeModule>(out ModuleNode _))) return false;
            generateText = testBaker.TestBake(containers, bakeContainer);
            return true;
        }
        private IDialogueTree LoadDataFromFile(string path)
        {
            try
            {
                return AssetDatabase.LoadAssetAtPath<NextGenDialogueTreeSO>($"Assets/{path}");

            }
            catch
            {
                ShowNotification(new GUIContent($"Invalid Path:Assets/{path}, asset type must be inherited from NextGenDialogueTreeSO !"));
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