using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public abstract class ContainerNode : StackNode, IDialogueNode
    {
        public ContainerNode() : base()
        {
            capabilities |= Capabilities.Groupable;
            fieldResolverFactory = FieldResolverFactory.Instance;
            fieldContainer = new VisualElement();
            GUID = Guid.NewGuid().ToString();
            Initialize();
            headerContainer.style.flexDirection = FlexDirection.Column;
            headerContainer.style.justifyContent = Justify.Center;
            titleLabel = new Label();
            headerContainer.Add(titleLabel);
            description = new TextField
            {
                multiline = true
            };
            description.style.whiteSpace = WhiteSpace.Normal;
            AddDescription();
            AddToClassList("DialogueStack");
            //Replace dark color of the place holder
            this.Q<VisualElement>(classes: "stack-node-placeholder")
                .Children()
                .First().style.color = new Color(1, 1, 1, 0.6f);
            var bridge = new ParentBridge(ParentPortType);
            Parent = bridge.Parent;
            Parent.portColor = PortColor;
            AddElement(bridge);
            InitializeSettings();
        }
        public abstract Color PortColor { get; }
        private readonly TextField description;
        public abstract Type ParentPortType { get; }
        public string GUID { get; private set; }
        protected NodeBehavior NodeBehavior { set; get; }
        private readonly Label titleLabel;
        private Type dirtyNodeBehaviorType;
        public Port Parent { get; }
        private readonly VisualElement fieldContainer;
        private readonly FieldResolverFactory fieldResolverFactory;
        private readonly List<IFieldResolver> resolvers = new();
        private readonly List<FieldInfo> fieldInfos = new();
        public Action<IDialogueNode> OnSelectAction { get; set; }
        protected IDialogueTreeView mapTreeView;
        //Setting
        private VisualElement settings;
        private NodeSettingsView settingsContainer;
        private Button settingButton;
        private bool settingsExpanded = false;
        public IFieldResolver GetFieldResolver(string fieldName)
        {
            int index = fieldInfos.FindIndex(x => x.Name == fieldName);
            if (index != -1) return resolvers[index];
            else return null;
        }
        private void InitializeSettings()
        {
            // Initialize settings button:
            settingsContainer = new NodeSettingsView(this)
            {
                visible = false
            };
            settings = new VisualElement();
            // Add Node type specific settings
            settings.Add(CreateSettingsView());
            settingsContainer.Add(settings);
            headerContainer.Add(settingsContainer);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            OnGeometryChanged(null);
        }
        private void CreateSettingButton()
        {
            settingButton = new Button(ToggleSettings) { name = "settings-button" };
            var image = new Image() { name = "icon", scaleMode = ScaleMode.ScaleToFit };
            image.style.backgroundImage = Resources.Load<Texture2D>("NGDT/SettingIcon");
            settingButton.Add(image);
            headerContainer.Add(settingButton);
        }
        protected virtual VisualElement CreateSettingsView() => new Label("Advanced Settings") { name = "header" };
        void ToggleSettings()
        {
            settingsExpanded = !settingsExpanded;
            if (settingsExpanded)
                OpenSettings();
            else
                CloseSettings();
        }
        public void OpenSettings()
        {
            if (settingsContainer != null)
            {
                settingButton.AddToClassList("clicked");
                settingsContainer.visible = true;
                settingsExpanded = true;
            }
        }

        public void CloseSettings()
        {
            if (settingsContainer != null)
            {
                settingButton.RemoveFromClassList("clicked");
                settingsContainer.visible = false;
                settingsExpanded = false;
            }
        }
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (settingButton != null)
            {
                var settingsButtonLayout = settingButton.ChangeCoordinatesTo(settingsContainer.parent, settingButton.layout);
                settingsContainer.style.top = settingsButtonLayout.yMax - 18f;
                settingsContainer.style.left = settingsButtonLayout.xMin - layout.width + 20f;
            }
        }
        private void Initialize()
        {
            mainContainer.Add(fieldContainer);
        }
        public override void OnSelected()
        {
            base.OnSelected();
            OnSelectAction?.Invoke(this);
        }
        private void AddDescription()
        {
            description.RegisterCallback<FocusInEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.On; });
            description.RegisterCallback<FocusOutEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.Auto; });
            headerContainer.Add(description);
        }
        public void Restore(NodeBehavior behavior)
        {
            NodeBehavior = behavior;
            resolvers.ForEach(e => e.Restore(NodeBehavior));
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            description.value = NodeBehavior.description;
            GUID = string.IsNullOrEmpty(behavior.GUID) ? Guid.NewGuid().ToString() : behavior.GUID;
        }
        public void CopyFrom(IDialogueNode copyNode)
        {
            var node = copyNode as ContainerNode;
            for (int i = 0; i < node.resolvers.Count; i++)
            {
                resolvers[i].Copy(node.resolvers[i]);
            }
            moduleCopyMapCache.Clear();
            node.contentContainer.Query<ModuleNode>()
            .ToList()
            .ForEach(
                x =>
                {
                    //Copy its child module
                    var newNode = mapTreeView.DuplicateNode(x) as ModuleNode;
                    moduleCopyMapCache[x.GetHashCode()] = newNode;
                    AddElement(newNode);
                }
            );
            description.value = node.description.value;
            NodeBehavior = (NodeBehavior)Activator.CreateInstance(copyNode.GetBehavior());
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            GUID = Guid.NewGuid().ToString();
        }
        private readonly Dictionary<int, ModuleNode> moduleCopyMapCache = new();
        internal IReadOnlyDictionary<int, ModuleNode> GetModuleCopyMap()
        {
            return moduleCopyMapCache;
        }
        public NodeBehavior ReplaceBehavior()
        {
            NodeBehavior = (NodeBehavior)Activator.CreateInstance(GetBehavior());
            return NodeBehavior;
        }
        public Type GetBehavior()
        {
            return dirtyNodeBehaviorType;
        }

        public void Commit(Stack<IDialogueNode> stack)
        {
            OnCommit(stack);
            var nodes = contentContainer.Query<ModuleNode>().ToList();
            nodes.ForEach(x =>
            {
                (NodeBehavior as Container).AddChild(x.ReplaceBehavior());
                stack.Push(x);
            });
            var bridges = contentContainer.Query<ChildBridge>().ToList();
            //Manually commit bridge node
            //Do not duplicate commit dialogue piece
            bridges.ForEach(x => x.Commit(NodeBehavior as Container, stack));
            resolvers.ForEach(r => r.Commit(NodeBehavior));
            NodeBehavior.description = description.value;
            NodeBehavior.graphPosition = GetPosition();
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            NodeBehavior.GUID = GUID;
        }
        protected virtual void OnCommit(Stack<IDialogueNode> stack) { }
        public bool Validate(Stack<IDialogueNode> stack)
        {
            contentContainer.Query<ModuleNode>()
                            .ForEach(x => stack.Push(x));
            contentContainer.Query<ChildBridge>()
                            .ForEach(x => x.Validate(stack));
            var valid = GetBehavior() != null;
            if (valid)
            {
                style.backgroundColor = new StyleColor(StyleKeyword.Null);
            }
            else
            {
                style.backgroundColor = Color.red;
            }
            return valid;
        }
        public void SetBehavior(Type nodeBehavior, IDialogueTreeView ownerTreeView = null)
        {
            if (ownerTreeView != null) mapTreeView = ownerTreeView;
            if (dirtyNodeBehaviorType != null)
            {
                dirtyNodeBehaviorType = null;
                fieldContainer.Clear();
                resolvers.Clear();
                fieldInfos.Clear();
            }
            dirtyNodeBehaviorType = nodeBehavior;

            var defaultValue = (NodeBehavior)Activator.CreateInstance(nodeBehavior);
            bool haveSetting = false;
            nodeBehavior
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(field => field.GetCustomAttribute<HideInEditorWindow>() == null)
                .Concat(GetAllFields(nodeBehavior))
                .Where(field => field.IsInitOnly == false)
                .ToList().ForEach((p) =>
                {
                    var fieldResolver = fieldResolverFactory.Create(p);
                    fieldResolver.Restore(defaultValue);
                    if (p.GetCustomAttribute<SettingAttribute>() != null)
                    {
                        settingsContainer.Add(fieldResolver.GetEditorField(mapTreeView));
                        haveSetting = true;
                    }
                    else
                    {
                        fieldContainer.Add(fieldResolver.GetEditorField(mapTreeView));
                    }
                    resolvers.Add(fieldResolver);
                    fieldInfos.Add(p);
                });
            var label = nodeBehavior.GetCustomAttribute(typeof(AkiLabelAttribute), false) as AkiLabelAttribute;
            titleLabel.text = label?.Title ?? nodeBehavior.Name;
            if (settingButton != null) headerContainer.Remove(settingButton);
            if (haveSetting)
            {
                CreateSettingButton();
            }
        }
        private static IEnumerable<FieldInfo> GetAllFields(Type t)
        {
            if (t == null)
                return Enumerable.Empty<FieldInfo>();

            return t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => field.GetCustomAttribute<SerializeField>() != null)
                .Where(field => field.GetCustomAttribute<HideInEditorWindow>() == null).Concat(GetAllFields(t.BaseType));
        }
        private void MarkAsExecuted(Status status)
        {
            switch (status)
            {
                case Status.Failure:
                    {
                        style.backgroundColor = Color.red;
                        break;
                    }
                case Status.Success:
                    {
                        style.backgroundColor = Color.green;
                        break;
                    }
            }
        }
        public void ClearStyle()
        {
            style.backgroundColor = new StyleColor(StyleKeyword.Null);
            contentContainer.Query<ModuleNode>()
                            .ForEach(x => x.ClearStyle());
            contentContainer.Query<ChildBridge>()
                            .ForEach(x => x.ClearStyle());
        }
        protected override void OnSeparatorContextualMenuEvent(ContextualMenuPopulateEvent evt, int separatorIndex)
        {
            evt.menu.MenuItems().Clear();
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Add Module", (a) =>
            {
                var provider = ScriptableObject.CreateInstance<ModuleSearchWindowProvider>();
                provider.Init(this, mapTreeView, NextGenDialogueSetting.GetMask(), GetModuleTypes());
                SearchWindow.Open(new SearchWindowContext(a.eventInfo.localMousePosition), provider);
            }));
        }
        private IEnumerable<Type> GetModuleTypes()
        {
            return contentContainer.Query<ModuleNode>().ToList().Select(x => x.GetBehavior());
        }
        public bool TryGetModuleNode<T>(out ModuleNode moduleNode) where T : Module
        {
            moduleNode = contentContainer.Query<ModuleNode>().ToList().FirstOrDefault(x => x.GetBehavior() == typeof(T));
            return moduleNode != null;
        }
        public bool TryGetModuleNode(Type type, out ModuleNode moduleNode)
        {
            moduleNode = contentContainer.Query<ModuleNode>().ToList().FirstOrDefault(x => x.GetBehavior() == type);
            return moduleNode != null;
        }
        public ModuleNode AddModuleNode<T>(T module) where T : Module
        {
            var type = typeof(T);
            var moduleNode = NodeResolverFactory.Instance.Create(type, mapTreeView) as ModuleNode;
            moduleNode.Restore(module);
            AddElement(moduleNode);
            moduleNode.OnSelectAction = OnSelectAction;
            return moduleNode;
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Duplicate", (a) =>
            {
                mapTreeView.DuplicateNode(this);
            }));
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Select Group", (a) =>
            {
                mapTreeView.SelectGroup(this);
            }));
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("UnSelect Group", (a) =>
            {
                mapTreeView.UnSelectGroup();
            }));
        }
    }
    public class DialogueContainer : ContainerNode, IContainChild
    {
        public sealed override Type ParentPortType => typeof(DialoguePort);

        public override Color PortColor => new(97 / 255f, 95 / 255f, 212 / 255f, 0.91f);

        public DialogueContainer() : base()
        {
            name = "DialogueContainer";
        }

        protected override void OnSeparatorContextualMenuEvent(ContextualMenuPopulateEvent evt, int separatorIndex)
        {
            base.OnSeparatorContextualMenuEvent(evt, separatorIndex);
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Add Piece", (a) =>
            {
                var bridge = new PieceBridge(mapTreeView, typeof(PiecePort), PortColor, string.Empty);
                AddElement(bridge);
            }));
        }
        public void AddChildElement(IDialogueNode node, IDialogueTreeView treeView)
        {
            string pieceID = ((PieceContainer)node).GetPieceID();
            var count = this.Query<PieceBridge>().ToList().Count;
            if (!string.IsNullOrEmpty(pieceID) && ((Dialogue)NodeBehavior).ResolvePieceID(count) == pieceID)
            {
                AddElement(new PieceBridge(treeView, typeof(PiecePort), PortColor, pieceID));
                return;
            }
            var bridge = new PieceBridge(treeView, typeof(PiecePort), PortColor, string.Empty);
            AddElement(bridge);
            var edge = PortHelper.ConnectPorts(bridge.Child, node.Parent);
            treeView.GraphView.Add(edge);
        }
        protected sealed override void OnCommit(Stack<IDialogueNode> stack)
        {
            ((Dialogue)NodeBehavior).referencePieces = new List<string>();
        }
        protected sealed override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            if (element is ModuleNode moduleNode)
            {
                var behaviorType = moduleNode.GetBehavior();
                if (!behaviorType.GetCustomAttributes<ModuleOfAttribute>().Any(x => x.ContainerType == typeof(Dialogue))) return false;
                return !TryGetModuleNode(behaviorType, out _);
            }
            return element is PieceBridge or ParentBridge;
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Collect All Pieces", (a) =>
            {
                var pieces = mapTreeView.CollectNodes<PieceContainer>();
                var currentPieces = this.Query<PieceBridge>().ToList();
                var addPieces = pieces.Where(x => !currentPieces.Any(p => !string.IsNullOrEmpty(p.PieceID) && p.PieceID == x.GetPieceID()));
                foreach (var piece in addPieces)
                {
                    AddElement(new PieceBridge(mapTreeView, typeof(PiecePort), PortColor, piece.GetPieceID()));
                }
            }));
            base.BuildContextualMenu(evt);
        }
    }
    public class PieceContainer : ContainerNode, IContainChild
    {
        public string GetPieceID()
        {
            return mainContainer.Q<PieceIDField>().value.Name;
        }
        public Piece GetPiece()
        {
            return (Piece)NodeBehavior;
        }
        public sealed override Type ParentPortType => typeof(PiecePort);
        public override Color PortColor => new(60 / 255f, 140 / 255f, 171 / 255f, 0.91f);
        public PieceContainer() : base()
        {
            name = "PieceContainer";
        }
        protected override void OnSeparatorContextualMenuEvent(ContextualMenuPopulateEvent evt, int separatorIndex)
        {
            base.OnSeparatorContextualMenuEvent(evt, separatorIndex);
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Add Option", (a) =>
            {
                var bridge = new OptionBridge("Option", typeof(OptionPort), PortColor);
                AddElement(bridge);
            }));
        }
        public sealed override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (TryGetModuleNode<AIBakeModule>(out _))
                evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Bake Dialogue", (a) =>
               {
                   mapTreeView.BakeDialogue();
               }));
            base.BuildContextualMenu(evt);
        }
        public void AddChildElement(IDialogueNode node, IDialogueTreeView treeView)
        {
            var bridge = new OptionBridge("Option", typeof(OptionPort), PortColor);
            AddElement(bridge);
            var edge = PortHelper.ConnectPorts(bridge.Child, node.Parent);
            treeView.GraphView.Add(edge);
        }
        protected sealed override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            if (element is ModuleNode moduleNode)
            {
                var behaviorType = moduleNode.GetBehavior();
                if (!behaviorType.GetCustomAttributes<ModuleOfAttribute>().Any(x => x.ContainerType == typeof(Piece))) return false;
                return !TryGetModuleNode(behaviorType, out _);
            }
            return element is OptionBridge or ParentBridge;
        }
    }
    public class OptionContainer : ContainerNode
    {
        public sealed override Type ParentPortType => typeof(OptionPort);
        public override Color PortColor => new(57 / 255f, 98 / 255f, 147 / 255f, 0.91f);
        public OptionContainer() : base()
        {
            name = "OptionContainer";
        }
        public sealed override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (TryGetModuleNode<AIBakeModule>(out _))
                evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Bake Dialogue", (a) =>
               {
                   mapTreeView.BakeDialogue();
               }));
            base.BuildContextualMenu(evt);
        }
        protected sealed override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            if (element is ModuleNode moduleNode)
            {
                var behaviorType = moduleNode.GetBehavior();
                if (!behaviorType.GetCustomAttributes<ModuleOfAttribute>().Any(x => x.ContainerType == typeof(Option))) return false;
                return !TryGetModuleNode(behaviorType, out _);
            }
            return element is ParentBridge;
        }
    }
}
