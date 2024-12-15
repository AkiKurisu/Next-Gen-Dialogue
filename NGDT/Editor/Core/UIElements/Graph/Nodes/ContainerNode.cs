using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ceres;
using Ceres.Annotations;
using Ceres.Editor;
using Ceres.Editor.Graph;
using Chris;
using Kurisu.NGDS;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Node = UnityEditor.Experimental.GraphView.Node;
namespace Kurisu.NGDT.Editor
{
    public abstract class ContainerNode : StackNode, IDialogueNode, ILayoutNode
    {
        public ContainerNode()
        {
            capabilities |= Capabilities.Groupable;
            _fieldResolverFactory = FieldResolverFactory.Get();
            _fieldContainer = new VisualElement();
            Guid = System.Guid.NewGuid().ToString();
            Initialize();
            headerContainer.style.flexDirection = FlexDirection.Column;
            headerContainer.style.justifyContent = Justify.Center;
            _titleLabel = new Label();
            headerContainer.Add(_titleLabel);
            _description = new TextField
            {
                multiline = true,
                style =
                {
                    whiteSpace = WhiteSpace.Normal
                }
            };
            AddDescription();
            AddToClassList("DialogueStack");
            // Replace dark color of the placeholder
            this.Q<VisualElement>(classes: "stack-node-placeholder")
                .Children()
                .First().style.color = new Color(1, 1, 1, 0.6f);
            var bridge = new ParentBridge(ParentPortType, PortCapacity);
            Parent = bridge.Parent;
            Parent.portColor = PortColor;
            AddElement(bridge);
        }
        public Node NodeElement => this;
        
        public virtual Port.Capacity PortCapacity => Port.Capacity.Single;
        
        public abstract Color PortColor { get; }
        
        private readonly TextField _description;
        
        public abstract Type ParentPortType { get; }
        
        public string Guid { get; private set; }
        
        protected NodeBehavior NodeBehavior { set; get; }
        
        
        private readonly Label _titleLabel;
        
        
        private Type _dirtyNodeBehaviorType;
        
        public Port Parent { get; }
        
        
        private readonly VisualElement _fieldContainer;
        
        
        private readonly FieldResolverFactory _fieldResolverFactory;
        
        
        private readonly List<IFieldResolver> _resolvers = new();
        
        
        private readonly List<FieldInfo> _fieldInfos = new();

        public DialogueGraphView MapGraphView { get; private set; }
        
        VisualElement ILayoutNode.View => this;
        
        public IFieldResolver GetFieldResolver(string fieldName)
        {
            int index = _fieldInfos.FindIndex(x => x.Name == fieldName);
            if (index != -1) return _resolvers[index];
            return null;
        }
        
        private void Initialize()
        {
            mainContainer.Add(_fieldContainer);
        }
        
        private void AddDescription()
        {
            _description.RegisterCallback<FocusInEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.On; });
            _description.RegisterCallback<FocusOutEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.Auto; });
            headerContainer.Add(_description);
        }
        
        public void Restore(NodeBehavior behavior)
        {
            NodeBehavior = behavior;
            _resolvers.ForEach(e => e.Restore(NodeBehavior));
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            _description.value = NodeBehavior.NodeData.description;
            Guid = string.IsNullOrEmpty(behavior.Guid) ? System.Guid.NewGuid().ToString() : behavior.Guid;
        }
        
        public void CopyFrom(IDialogueNode copyNode)
        {
            var node = (ContainerNode)copyNode;
            for (int i = 0; i < node._resolvers.Count; i++)
            {
                _resolvers[i].Copy(node._resolvers[i]);
            }
            _copyMap.Clear();
            node.contentContainer.Query<ModuleNode>()
            .ToList()
            .ForEach(
                x =>
                {
                    //Copy child module
                    var newNode = _copyMap[x.GetHashCode()] = MapGraphView.DuplicateNode(x) as ModuleNode;
                    AddElement(newNode);
                }
            );
            node.contentContainer.Query<ChildBridge>()
            .ToList()
            .ForEach(
                x =>
                {
                    //Copy child bridge
                    var newNode = _copyMap[x.GetHashCode()] = x.Clone();
                    AddElement(newNode);
                }
            );
            _description.value = node._description.value;
            NodeBehavior = (NodeBehavior)Activator.CreateInstance(copyNode.GetBehavior());
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            Guid = System.Guid.NewGuid().ToString();
        }
        
        private readonly Dictionary<int, Node> _copyMap = new();
        
        internal IReadOnlyDictionary<int, Node> GetCopyMap()
        {
            return _copyMap;
        }
        
        public NodeBehavior ReplaceBehavior()
        {
            NodeBehavior = (NodeBehavior)Activator.CreateInstance(GetBehavior());
            return NodeBehavior;
        }
        
        public Type GetBehavior()
        {
            return _dirtyNodeBehaviorType;
        }

        public void Commit(Stack<IDialogueNode> stack)
        {
            OnCommit(stack);
            var nodes = contentContainer.Query<ModuleNode>().ToList();
            nodes.ForEach(x =>
            {
                ((Container)NodeBehavior).AddChild(x.ReplaceBehavior());
                stack.Push(x);
            });
            var bridges = contentContainer.Query<ChildBridge>().ToList();
            // Manually commit bridge node
            // Do not duplicate commit dialogue piece
            bridges.ForEach(x => x.Commit(NodeBehavior as Container, stack));
            _resolvers.ForEach(r => r.Commit(NodeBehavior));
            NodeBehavior.NodeData.description = _description.value;
            NodeBehavior.NodeData.graphPosition = GetPosition();
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            NodeBehavior.Guid = Guid;
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
        
        public void SetBehavior(Type nodeBehavior, DialogueGraphView ownerGraphView = null)
        {
            if (ownerGraphView != null) MapGraphView = ownerGraphView;
            if (_dirtyNodeBehaviorType != null)
            {
                _dirtyNodeBehaviorType = null;
                _fieldContainer.Clear();
                _resolvers.Clear();
                _fieldInfos.Clear();
            }
            _dirtyNodeBehaviorType = nodeBehavior;

            var defaultValue = (NodeBehavior)Activator.CreateInstance(nodeBehavior);
            nodeBehavior.GetGraphEditorPropertyFields().ForEach((p) =>
                {
                    var fieldResolver = _fieldResolverFactory.Create(p);
                    fieldResolver.Restore(defaultValue);
                    _fieldContainer.Add(fieldResolver.GetField(MapGraphView));
                    _resolvers.Add(fieldResolver);
                    _fieldInfos.Add(p);
                });
            var label = nodeBehavior.GetCustomAttribute(typeof(CeresLabelAttribute), false) as CeresLabelAttribute;
            _titleLabel.text = label?.Label ?? nodeBehavior.Name;
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
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Add Module", (a) =>
            {
                var provider = ScriptableObject.CreateInstance<ModuleSearchWindowProvider>();
                provider.Init(this, MapGraphView, NextGenDialogueSetting.GetNodeSearchContext(), GetExceptModuleTypes());
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
            var moduleNode = DialogueNodeFactory.Get().Create(type, MapGraphView) as ModuleNode;
            moduleNode!.Restore(module);
            MapGraphView.AddNodeView(moduleNode);
            AddElement(moduleNode);
            return moduleNode;
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Duplicate", (a) =>
            {
                MapGraphView.DuplicateNode(this);
            }));
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Select Group", (a) =>
            {
                MapGraphView.GroupBlockHandler.SelectGroup(this);
            }));
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("UnSelect Group", (a) =>
            {
                MapGraphView.GroupBlockHandler.UnselectGroup();
            }));
            MapGraphView.ContextualMenuRegistry.BuildContextualMenu(ContextualMenuType.Node, evt, GetBehavior());
        }
        
        public Rect GetWorldPosition()
        {
            return GetPosition();
        }
        
        public IReadOnlyList<ILayoutNode> GetLayoutChildren()
        {
            var list = new List<ILayoutNode>();
            var nodes = contentContainer
                 .Query<Node>()
                 .ToList();
            nodes.Reverse();
            foreach (var node in nodes)
            {
                if (node is ILayoutNode layoutTreeNode)
                {
                    list.AddRange(layoutTreeNode.GetLayoutChildren());
                }
            }
            return list;
        }
    }
    public class DialogueContainer : ContainerNode, IContainChild, ILayoutNode
    {
        public sealed override Type ParentPortType => typeof(DialoguePort);
        public override Color PortColor => new(97 / 255f, 95 / 255f, 212 / 255f, 0.91f);

        VisualElement ILayoutNode.View => this;
        public DialogueContainer() : base()
        {
            name = "DialogueContainer";
        }

        protected override void OnSeparatorContextualMenuEvent(ContextualMenuPopulateEvent evt, int separatorIndex)
        {
            base.OnSeparatorContextualMenuEvent(evt, separatorIndex);
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Add Piece", (a) =>
            {
                var bridge = new PieceBridge(MapGraphView, PortColor, string.Empty);
                AddElement(bridge);
            }));
        }
        public void AddChildElement(IDialogueNode node, DialogueGraphView graphView)
        {
            string pieceID = ((PieceContainer)node).GetPieceID();
            var count = this.Query<PieceBridge>().ToList().Count;
            if (!string.IsNullOrEmpty(pieceID) && ((Dialogue)NodeBehavior).ResolvePieceID(count) == pieceID)
            {
                AddElement(new PieceBridge(graphView, PortColor, pieceID));
                return;
            }
            var bridge = new PieceBridge(graphView, PortColor, string.Empty);
            AddElement(bridge);
            var edge = PortHelper.ConnectPorts(bridge.Child, node.Parent);
            graphView.Add(edge);
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
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Collect All Pieces", (a) =>
            {
                var pieces = MapGraphView.CollectNodes<PieceContainer>();
                var currentPieces = this.Query<PieceBridge>().ToList();
                var addPieces = pieces.Where(x => !currentPieces.Any(p => !string.IsNullOrEmpty(p.PieceID) && p.PieceID == x.GetPieceID()));
                foreach (var piece in addPieces)
                {
                    AddElement(new PieceBridge(MapGraphView, PortColor, piece.GetPieceID()));
                }
            }));
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Auto Layout", (a) =>
            {
                NodeAutoLayoutHelper.Layout(new DialogueTreeLayoutConvertor(MapGraphView, this));
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
        
        public PieceContainer()
        {
            name = "PieceContainer";
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            MapGraphView.Blackboard.RemoveVariable(mainContainer.Q<PieceIDField>().BindVariable, true);
        }

        protected override void OnSeparatorContextualMenuEvent(ContextualMenuPopulateEvent evt, int separatorIndex)
        {
            base.OnSeparatorContextualMenuEvent(evt, separatorIndex);
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Add Option", (a) =>
            {
                var bridge = new OptionBridge("Option", PortColor);
                AddElement(bridge);
            }));
        }
        public sealed override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Edit PieceID", (a) =>
            {
                MapGraphView.Blackboard.EditVariable(GetPieceID());
            }));
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Auto Layout", (a) =>
            {
                NodeAutoLayoutHelper.Layout(new DialogueTreeLayoutConvertor(MapGraphView, this));
            }));
            if (Application.isPlaying)
            {
                evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Jump to this Piece", (a) =>
                {
                    ContainerSubsystem.Get().Resolve<IDialogueSystem>()
                                     .PlayDialoguePiece(GetPiece().CastPiece().PieceID);
                },
                (_) =>
                {
                    var ds = ContainerSubsystem.Get().Resolve<IDialogueSystem>();
                    if (ds == null) return DropdownMenuAction.Status.Hidden;
                    if (!ds.IsPlaying) return DropdownMenuAction.Status.Disabled;
                    // Whether is the container of this piece
                    var piece = GetPiece().CastPiece();
                    if (ds.GetCurrentDialogue()?.GetPiece(piece.PieceID) != piece) return DropdownMenuAction.Status.Disabled;
                    return DropdownMenuAction.Status.Normal;
                }));
            }
            base.BuildContextualMenu(evt);
        }
        
        public void AddChildElement(IDialogueNode node, DialogueGraphView graphView)
        {
            var bridge = new OptionBridge("Option", PortColor);
            AddElement(bridge);
            var edge = PortHelper.ConnectPorts(bridge.Child, node.Parent);
            graphView.Add(edge);
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
            MapGraphView.Blackboard.AddVariable(variable, false);
            mainContainer.Q<PieceIDField>().value = new PieceID() { Name = variable.Name };
        }
        
        protected override IEnumerable<Type> GetExceptModuleTypes()
        {
            return contentContainer.Query<ModuleNode>().ToList().Select(x => x.GetBehavior()).Where(x =>
            {
                var define = x.GetCustomAttributes<ModuleOfAttribute>()
                                            .First(attribute => attribute.ContainerType == typeof(Piece));
                return !define.AllowMulti;
            });
        }
    }
    public class OptionContainer : ContainerNode
    {
        public sealed override Type ParentPortType => typeof(OptionPort);
        public override Color PortColor => new(57 / 255f, 98 / 255f, 147 / 255f, 0.91f);
        public OptionContainer()
        {
            name = "OptionContainer";
        }
        public sealed override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Auto Layout", (a) =>
            {
                NodeAutoLayoutHelper.Layout(new DialogueTreeLayoutConvertor(MapGraphView, this));
            }));
            base.BuildContextualMenu(evt);
        }
        protected sealed override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            if (element is ModuleNode moduleNode)
            {
                var behaviorType = moduleNode.GetBehavior();
                if (behaviorType.GetCustomAttributes<ModuleOfAttribute>().All(x => x.ContainerType != typeof(Option))) return false;
                return !TryGetModuleNode(behaviorType, out _);
            }
            return element is ParentBridge;
        }
        protected override IEnumerable<Type> GetExceptModuleTypes()
        {
            return contentContainer.Query<ModuleNode>().ToList().Select(x => x.GetBehavior()).Where(x =>
            {
                var define = x.GetCustomAttributes<ModuleOfAttribute>().First(attribute => attribute.ContainerType == typeof(Option));
                return !define.AllowMulti;
            });
        }
    }
}
