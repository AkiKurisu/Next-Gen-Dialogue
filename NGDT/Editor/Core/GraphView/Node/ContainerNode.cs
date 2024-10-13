using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public abstract class ContainerNode : StackNode, IDialogueNode, ILayoutTreeNode
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
            var bridge = new ParentBridge(ParentPortType, PortCapacity);
            Parent = bridge.Parent;
            Parent.portColor = PortColor;
            AddElement(bridge);
        }
        public Node View => this;
        public virtual Port.Capacity PortCapacity => Port.Capacity.Single;
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
        public IDialogueTreeView MapTreeView { get; private set; }
        VisualElement ILayoutTreeNode.View => this;
        public IFieldResolver GetFieldResolver(string fieldName)
        {
            int index = fieldInfos.FindIndex(x => x.Name == fieldName);
            if (index != -1) return resolvers[index];
            else return null;
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
            copyMap.Clear();
            node.contentContainer.Query<ModuleNode>()
            .ToList()
            .ForEach(
                x =>
                {
                    //Copy child module
                    var newNode = copyMap[x.GetHashCode()] = MapTreeView.DuplicateNode(x) as ModuleNode;
                    AddElement(newNode);
                }
            );
            node.contentContainer.Query<ChildBridge>()
            .ToList()
            .ForEach(
                x =>
                {
                    //Copy child bridge
                    var newNode = copyMap[x.GetHashCode()] = x.Clone();
                    AddElement(newNode);
                }
            );
            description.value = node.description.value;
            NodeBehavior = (NodeBehavior)Activator.CreateInstance(copyNode.GetBehavior());
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            GUID = Guid.NewGuid().ToString();
        }
        private readonly Dictionary<int, Node> copyMap = new();
        internal IReadOnlyDictionary<int, Node> GetCopyMap()
        {
            return copyMap;
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
            if (ownerTreeView != null) MapTreeView = ownerTreeView;
            if (dirtyNodeBehaviorType != null)
            {
                dirtyNodeBehaviorType = null;
                fieldContainer.Clear();
                resolvers.Clear();
                fieldInfos.Clear();
            }
            dirtyNodeBehaviorType = nodeBehavior;

            var defaultValue = (NodeBehavior)Activator.CreateInstance(nodeBehavior);
            nodeBehavior.GetEditorWindowFields().ForEach((p) =>
                {
                    var fieldResolver = fieldResolverFactory.Create(p);
                    fieldResolver.Restore(defaultValue);
                    fieldContainer.Add(fieldResolver.GetEditorField(MapTreeView));
                    resolvers.Add(fieldResolver);
                    fieldInfos.Add(p);
                });
            var label = nodeBehavior.GetCustomAttribute(typeof(AkiLabelAttribute), false) as AkiLabelAttribute;
            titleLabel.text = label?.Title ?? nodeBehavior.Name;
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
                provider.Init(this, MapTreeView, NextGenDialogueSetting.GetMask(), GetExceptModuleTypes());
                SearchWindow.Open(new SearchWindowContext(a.eventInfo.localMousePosition), provider);
            }));
        }
        public void RemoveModule<T>() where T : Module
        {
            if (TryGetModuleNode<T>(out ModuleNode node))
            {
                RemoveElement(node);
            }
        }
        protected abstract IEnumerable<Type> GetExceptModuleTypes();
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
        public ModuleNode GetModuleNode<T>(int index) where T : Module
        {
            var nodes = GetModuleNodes<T>();
            return nodes[index];
        }
        public ModuleNode[] GetModuleNodes<T>() where T : Module
        {
            return contentContainer.Query<ModuleNode>().ToList().Where(x => x.GetBehavior() == typeof(T)).ToArray();
        }
        public ModuleNode AddModuleNode<T>(T module) where T : Module, new()
        {
            var type = typeof(T);
            var moduleNode = NodeResolverFactory.Instance.Create(type, MapTreeView) as ModuleNode;
            moduleNode.Restore(module);
            AddElement(moduleNode);
            moduleNode.OnSelectAction = OnSelectAction;
            return moduleNode;
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Duplicate", (a) =>
            {
                MapTreeView.DuplicateNode(this);
            }));
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Select Group", (a) =>
            {
                MapTreeView.GroupBlockController.SelectGroup(this);
            }));
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("UnSelect Group", (a) =>
            {
                MapTreeView.GroupBlockController.UnSelectGroup();
            }));
            MapTreeView.ContextualMenuController.BuildContextualMenu(ContextualMenuType.Node, evt, GetBehavior());
        }
        public Rect GetWorldPosition()
        {
            return GetPosition();
        }
        public IReadOnlyList<ILayoutTreeNode> GetLayoutTreeChildren()
        {
            var list = new List<ILayoutTreeNode>();
            var nodes = contentContainer
                 .Query<Node>()
                 .ToList();
            nodes.Reverse();
            foreach (var node in nodes)
            {
                if (node is ILayoutTreeNode layoutTreeNode)
                {
                    list.AddRange(layoutTreeNode.GetLayoutTreeChildren());
                }
            }
            return list;
        }
    }
    public class DialogueContainer : ContainerNode, IContainChild, ILayoutTreeNode
    {
        public sealed override Type ParentPortType => typeof(DialoguePort);
        public override Color PortColor => new(97 / 255f, 95 / 255f, 212 / 255f, 0.91f);

        VisualElement ILayoutTreeNode.View => this;
        public DialogueContainer() : base()
        {
            name = "DialogueContainer";
        }

        protected override void OnSeparatorContextualMenuEvent(ContextualMenuPopulateEvent evt, int separatorIndex)
        {
            base.OnSeparatorContextualMenuEvent(evt, separatorIndex);
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Add Piece", (a) =>
            {
                var bridge = new PieceBridge(MapTreeView, PortColor, string.Empty);
                AddElement(bridge);
            }));
        }
        public void AddChildElement(IDialogueNode node, IDialogueTreeView treeView)
        {
            string pieceID = ((PieceContainer)node).GetPieceID();
            var count = this.Query<PieceBridge>().ToList().Count;
            if (!string.IsNullOrEmpty(pieceID) && ((Dialogue)NodeBehavior).ResolvePieceID(count) == pieceID)
            {
                AddElement(new PieceBridge(treeView, PortColor, pieceID));
                return;
            }
            var bridge = new PieceBridge(treeView, PortColor, string.Empty);
            AddElement(bridge);
            var edge = PortHelper.ConnectPorts(bridge.Child, node.Parent);
            treeView.View.Add(edge);
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
                var define = behaviorType.GetCustomAttributes<ModuleOfAttribute>().FirstOrDefault(x => x.ContainerType == typeof(Dialogue));
                if (define == null) return false;
                if (define.AllowMulti) return true;
                return !TryGetModuleNode(behaviorType, out _);
            }
            return element is PieceBridge or ParentBridge;
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Collect All Pieces", (a) =>
            {
                var pieces = MapTreeView.CollectNodes<PieceContainer>();
                var currentPieces = this.Query<PieceBridge>().ToList();
                var addPieces = pieces.Where(x => !currentPieces.Any(p => !string.IsNullOrEmpty(p.PieceID) && p.PieceID == x.GetPieceID()));
                foreach (var piece in addPieces)
                {
                    AddElement(new PieceBridge(MapTreeView, PortColor, piece.GetPieceID()));
                }
            }));
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Auto Layout", (a) =>
            {
                NodeAutoLayoutHelper.Layout(new DialogueTreeLayoutConvertor(MapTreeView.View, this));
            }));
            base.BuildContextualMenu(evt);
        }

        protected override IEnumerable<Type> GetExceptModuleTypes()
        {
            return contentContainer.Query<ModuleNode>().ToList().Select(x => x.GetBehavior()).Where(x =>
            {
                var define = x.GetCustomAttributes<ModuleOfAttribute>().FirstOrDefault(x => x.ContainerType == typeof(Dialogue));
                return !define.AllowMulti;
            });
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
        public sealed override Port.Capacity PortCapacity => Port.Capacity.Multi;
        public sealed override Type ParentPortType => typeof(PiecePort);
        public override Color PortColor => new(60 / 255f, 140 / 255f, 171 / 255f, 0.91f);
        public PieceContainer() : base()
        {
            name = "PieceContainer";
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            MapTreeView.BlackBoard.RemoveVariable(mainContainer.Q<PieceIDField>().bindVariable, true);
        }

        protected override void OnSeparatorContextualMenuEvent(ContextualMenuPopulateEvent evt, int separatorIndex)
        {
            base.OnSeparatorContextualMenuEvent(evt, separatorIndex);
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Add Option", (a) =>
            {
                var bridge = new OptionBridge("Option", PortColor);
                AddElement(bridge);
            }));
        }
        public sealed override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Edit PieceID", (a) =>
            {
                MapTreeView.BlackBoard.EditVariable(GetPieceID());
            }));
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Auto Layout", (a) =>
            {
                NodeAutoLayoutHelper.Layout(new DialogueTreeLayoutConvertor(MapTreeView.View, this));
            }));
            if (Application.isPlaying)
            {
                evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Jump to this Piece", (a) =>
                {
                    NGDS.IOCContainer.Resolve<NGDS.IDialogueSystem>()
                                     .PlayDialoguePiece(GetPiece().CastPiece().PieceID);
                },
                (e) =>
                {
                    var ds = NGDS.IOCContainer.Resolve<NGDS.IDialogueSystem>();
                    if (ds == null) return DropdownMenuAction.Status.Hidden;
                    if (!ds.IsPlaying) return DropdownMenuAction.Status.Disabled;
                    //Whether is the container of this piece
                    var piece = GetPiece().CastPiece();
                    if (ds.GetCurrentDialogue()?.GetPiece(piece.PieceID) != piece) return DropdownMenuAction.Status.Disabled;
                    return DropdownMenuAction.Status.Normal;
                }));
            }
            base.BuildContextualMenu(evt);
        }
        public void AddChildElement(IDialogueNode node, IDialogueTreeView treeView)
        {
            var bridge = new OptionBridge("Option", PortColor);
            AddElement(bridge);
            var edge = PortHelper.ConnectPorts(bridge.Child, node.Parent);
            treeView.View.Add(edge);
        }
        protected sealed override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            if (element is ModuleNode moduleNode)
            {
                var behaviorType = moduleNode.GetBehavior();
                var define = behaviorType.GetCustomAttributes<ModuleOfAttribute>().FirstOrDefault(x => x.ContainerType == typeof(Piece));
                if (define == null) return false;
                if (define.AllowMulti) return true;
                return !TryGetModuleNode(behaviorType, out _);
            }
            return element is OptionBridge or ParentBridge;
        }
        public void GenerateNewPieceID()
        {
            var variable = new PieceID() { Name = "New Piece" };
            MapTreeView.BlackBoard.AddVariable(variable, false);
            mainContainer.Q<PieceIDField>().value = new PieceID() { Name = variable.Name };
        }
        protected override IEnumerable<Type> GetExceptModuleTypes()
        {
            return contentContainer.Query<ModuleNode>().ToList().Select(x => x.GetBehavior()).Where(x =>
            {
                var define = x.GetCustomAttributes<ModuleOfAttribute>().FirstOrDefault(x => x.ContainerType == typeof(Piece));
                return !define.AllowMulti;
            });
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
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Auto Layout", (a) =>
            {
                NodeAutoLayoutHelper.Layout(new DialogueTreeLayoutConvertor(MapTreeView.View, this));
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
        protected override IEnumerable<Type> GetExceptModuleTypes()
        {
            return contentContainer.Query<ModuleNode>().ToList().Select(x => x.GetBehavior()).Where(x =>
            {
                var define = x.GetCustomAttributes<ModuleOfAttribute>().FirstOrDefault(x => x.ContainerType == typeof(Option));
                return !define.AllowMulti;
            });
        }
    }
}
