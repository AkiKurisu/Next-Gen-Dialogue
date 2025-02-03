using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ceres.Annotations;
using Ceres.Editor;
using Ceres.Editor.Graph;
using Ceres.Utilities;
using NextGenDialogue;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Node = UnityEditor.Experimental.GraphView.Node;
namespace NextGenDialogue.Graph.Editor
{
    public abstract class ContainerNodeView : StackNode, IDialogueNodeView, ILayoutNode
    {
        protected ContainerNodeView(Type type, CeresGraphView graphView)
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
            var bridge = new ParentBridgeView(ParentPortType, PortCapacity);
            Parent = bridge.Parent;
            Parent.portColor = PortColor;
            AddElement(bridge);
            Initialize(type, (DialogueGraphView)graphView);
            styleSheets.Add(CeresGraphView.GetOrLoadStyleSheet(NextGenDialogueSettings.NodeStylePath));
        }
        
        public UnityEditor.Experimental.GraphView.Node NodeElement => this;

        protected virtual Port.Capacity PortCapacity => Port.Capacity.Single;

        protected abstract Color PortColor { get; }
        
        private readonly TextField _description;

        protected abstract Type ParentPortType { get; }
        
        public string Guid { get; private set; }
        
        protected DialogueNode NodeBehavior { get; private set;  }
        
        
        private readonly Label _titleLabel;
        
        
        private Type _nodeType;
        
        public Port Parent { get; }
        
        
        private readonly VisualElement _fieldContainer;
        
        
        private readonly FieldResolverFactory _fieldResolverFactory;
        
        
        private readonly List<IFieldResolver> _resolvers = new();
        
        
        private readonly List<FieldInfo> _fieldInfos = new();

        public DialogueGraphView Graph { get; private set; }
        
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
            _description.RegisterCallback<FocusInEvent>(_ => { Input.imeCompositionMode = IMECompositionMode.On; });
            _description.RegisterCallback<FocusOutEvent>(_ => { Input.imeCompositionMode = IMECompositionMode.Auto; });
            headerContainer.Add(_description);
        }
        
        public void SetNodeInstance(DialogueNode dialogueNode)
        {
            NodeBehavior = dialogueNode;
            _resolvers.ForEach(e => e.Restore(NodeBehavior));
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            _description.value = NodeBehavior.NodeData.description;
            Guid = string.IsNullOrEmpty(dialogueNode.Guid) ? System.Guid.NewGuid().ToString() : dialogueNode.Guid;
        }
        
        public void CopyFrom(IDialogueNodeView copyNode)
        {
            var node = (ContainerNodeView)copyNode;
            for (int i = 0; i < node._resolvers.Count; i++)
            {
                _resolvers[i].Copy(node._resolvers[i]);
            }
            _copyMap.Clear();
            node.contentContainer.Query<ModuleNodeView>()
            .ToList()
            .ForEach(
                x =>
                {
                    //Copy child module
                    var newNode = _copyMap[x.GetHashCode()] = Graph.DuplicateNode(x) as ModuleNodeView;
                    AddElement(newNode);
                }
            );
            node.contentContainer.Query<ChildBridgeView>()
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
            NodeBehavior = (DialogueNode)Activator.CreateInstance(copyNode.GetBehavior());
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            Guid = System.Guid.NewGuid().ToString();
        }
        
        private readonly Dictionary<int, UnityEditor.Experimental.GraphView.Node> _copyMap = new();
        
        internal IReadOnlyDictionary<int, UnityEditor.Experimental.GraphView.Node> GetCopyMap()
        {
            return _copyMap;
        }
        
        public DialogueNode Compile()
        {
            NodeBehavior = (DialogueNode)Activator.CreateInstance(GetBehavior());
            return NodeBehavior;
        }
        
        public Type GetBehavior()
        {
            return _nodeType;
        }

        public void Commit(Stack<IDialogueNodeView> stack)
        {
            OnCommit(stack);
            var nodes = contentContainer.Query<ModuleNodeView>().ToList();
            nodes.ForEach(x =>
            {
                ((ContainerNode)NodeBehavior).AddChild(x.Compile());
                stack.Push(x);
            });
            var bridges = contentContainer.Query<ChildBridgeView>().ToList();
            // Manually commit bridge node
            // Do not duplicate commit dialogue piece
            bridges.ForEach(bridge => bridge.Commit((ContainerNode)NodeBehavior, stack));
            _resolvers.ForEach(resolver => resolver.Commit(NodeBehavior));
            NodeBehavior.NodeData.description = _description.value;
            NodeBehavior.NodeData.graphPosition = GetPosition();
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            NodeBehavior.Guid = Guid;
        }
        
        protected virtual void OnCommit(Stack<IDialogueNodeView> stack) { }
        
        public bool Validate(Stack<IDialogueNodeView> stack)
        {
            contentContainer.Query<ModuleNodeView>()
                            .ForEach(x => stack.Push(x));
            contentContainer.Query<ChildBridgeView>()
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
        
        protected virtual void Initialize(Type nodeType, DialogueGraphView graphView)
        {
            Assert.IsNotNull(nodeType);
            Assert.IsNotNull(graphView);
            Graph = graphView;
            _nodeType = nodeType;

            var defaultValue = (DialogueNode)Activator.CreateInstance(nodeType);
            nodeType.GetGraphEditorPropertyFields().ForEach(p =>
                {
                    var fieldResolver = _fieldResolverFactory.Create(p);
                    fieldResolver.Restore(defaultValue);
                    _fieldContainer.Add(fieldResolver.GetField(Graph));
                    _resolvers.Add(fieldResolver);
                    _fieldInfos.Add(p);
                });
            _titleLabel.text = CeresLabel.GetLabel(nodeType);
        }
        
        private void MarkAsExecuted(Status status)
        {
            style.backgroundColor = status switch
            {
                Status.Failure => Color.red,
                Status.Success => Color.green,
                _ => style.backgroundColor
            };
        }
        
        public void ClearStyle()
        {
            style.backgroundColor = new StyleColor(StyleKeyword.Null);
            contentContainer.Query<ModuleNodeView>()
                            .ForEach(x => x.ClearStyle());
            contentContainer.Query<ChildBridgeView>()
                            .ForEach(x => x.ClearStyle());
        }
        
        protected override void OnSeparatorContextualMenuEvent(ContextualMenuPopulateEvent evt, int separatorIndex)
        {
            evt.menu.MenuItems().Clear();
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Add Module", a =>
            {
                var provider = ScriptableObject.CreateInstance<ModuleSearchWindowProvider>();
                provider.Init(this, Graph, NextGenDialogueSettings.GetNodeSearchContext(), GetExceptModuleTypes());
                SearchWindow.Open(new SearchWindowContext(a.eventInfo.localMousePosition), provider);
            }));
        }
        
        public void RemoveModule<T>() where T : Module
        {
            if (TryGetModuleNode<T>(out var node))
            {
                RemoveElement(node);
            }
        }
        
        protected abstract IEnumerable<Type> GetExceptModuleTypes();
        
        public bool TryGetModuleNode<T>(out ModuleNodeView moduleNodeView) where T : Module
        {
            moduleNodeView = contentContainer.Query<ModuleNodeView>().ToList().FirstOrDefault(x => x.GetBehavior() == typeof(T));
            return moduleNodeView != null;
        }
        
        public bool TryGetModuleNode(Type type, out ModuleNodeView moduleNodeView)
        {
            moduleNodeView = contentContainer.Query<ModuleNodeView>().ToList().FirstOrDefault(x => x.GetBehavior() == type);
            return moduleNodeView != null;
        }
        
        public ModuleNodeView GetModuleNode<T>(int index) where T : Module
        {
            var nodes = GetModuleNodes<T>();
            return nodes[index];
        }
        
        public ModuleNodeView[] GetModuleNodes<T>() where T : Module
        {
            return contentContainer.Query<ModuleNodeView>().ToList().Where(x => x.GetBehavior() == typeof(T)).ToArray();
        }
        
        public ModuleNodeView AddModuleNode<T>(T module) where T : Module, new()
        {
            var moduleNode = (ModuleNodeView)NodeViewFactory.Get().CreateInstance(typeof(T), Graph);
            moduleNode.SetNodeInstance(module);
            Graph.AddNodeView(moduleNode);
            AddElement(moduleNode);
            return moduleNode;
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Duplicate", a =>
            {
                Graph.DuplicateNode(this);
            }));
            Graph.ContextualMenuRegistry.BuildContextualMenu(ContextualMenuType.Node, evt, GetBehavior());
        }
        
        public Rect GetWorldPosition()
        {
            return GetPosition();
        }
        
        public IReadOnlyList<ILayoutNode> GetLayoutChildren()
        {
            var list = new List<ILayoutNode>();
            var nodes = contentContainer
                 .Query<UnityEditor.Experimental.GraphView.Node>()
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
    
    [CustomNodeView(typeof(Dialogue))]
    public sealed class DialogueContainerView : ContainerNodeView, IContainChild, ILayoutNode
    {
        protected override Type ParentPortType => typeof(DialoguePort);
        protected override Color PortColor => new(97 / 255f, 95 / 255f, 212 / 255f, 0.91f);

        VisualElement ILayoutNode.View => this;
        
        public DialogueContainerView(Type type, CeresGraphView graphView): base(type, graphView)
        {
            name = nameof(DialogueContainerView);
        }

        protected override void OnSeparatorContextualMenuEvent(ContextualMenuPopulateEvent evt, int separatorIndex)
        {
            base.OnSeparatorContextualMenuEvent(evt, separatorIndex);
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Add Piece", a =>
            {
                var bridge = new PieceBridgeView(Graph, PortColor, string.Empty);
                AddElement(bridge);
            }));
        }
        
        public void AddChildElement(IDialogueNodeView node, DialogueGraphView graphView)
        {
            string pieceID = ((PieceContainerView)node).GetPieceID();
            var count = this.Query<PieceBridgeView>().ToList().Count;
            if (!string.IsNullOrEmpty(pieceID) && ((Dialogue)NodeBehavior).ResolvePieceID(count) == pieceID)
            {
                AddElement(new PieceBridgeView(graphView, PortColor, pieceID));
                return;
            }
            var bridge = new PieceBridgeView(graphView, PortColor, string.Empty);
            AddElement(bridge);
            var edge = PortHelper.ConnectPorts(bridge.Child, node.Parent);
            graphView.Add(edge);
        }
        
        protected override void OnCommit(Stack<IDialogueNodeView> stack)
        {
            ((Dialogue)NodeBehavior).referencePieces = new List<string>();
        }
        
        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            if (element is ModuleNodeView moduleNode)
            {
                var behaviorType = moduleNode.GetBehavior();
                var define = behaviorType.GetCustomAttributes<ModuleOfAttribute>().FirstOrDefault(x => x.ContainerType == typeof(Dialogue));
                if (define == null) return false;
                if (define.AllowMulti) return true;
                return !TryGetModuleNode(behaviorType, out _);
            }
            return element is PieceBridgeView or ParentBridgeView;
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Collect All Pieces", a =>
            {
                var pieces = Graph.CollectNodes<PieceContainerView>();
                var currentPieces = this.Query<PieceBridgeView>().ToList();
                var addPieces = pieces.Where(x => !currentPieces.Any(p => !string.IsNullOrEmpty(p.PieceID) && p.PieceID == x.GetPieceID()));
                foreach (var piece in addPieces)
                {
                    AddElement(new PieceBridgeView(Graph, PortColor, piece.GetPieceID()));
                }
            }));
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Auto Layout", a =>
            {
                new DialogueTreeLayoutConvertor(Graph, this).Layout();
            }));
            base.BuildContextualMenu(evt);
        }

        protected override IEnumerable<Type> GetExceptModuleTypes()
        {
            return contentContainer.Query<ModuleNodeView>().ToList().Select(x => x.GetBehavior()).Where(x =>
            {
                var define = x.GetCustomAttributes<ModuleOfAttribute>()
                    .FirstOrDefault(moduleOfAttribute => moduleOfAttribute.ContainerType == typeof(Dialogue));
                return define!.AllowMulti;
            });
        }
    }
    
    [CustomNodeView(typeof(Piece))]
    public sealed class PieceContainerView : ContainerNodeView, IContainChild
    {
        public string GetPieceID()
        {
            return mainContainer.Q<PieceIDField>().value.Name;
        }
        
        public Piece GetPiece()
        {
            return (Piece)NodeBehavior;
        }

        protected override Port.Capacity PortCapacity => Port.Capacity.Multi;

        protected override Type ParentPortType => typeof(PiecePort);

        protected override Color PortColor => new(60 / 255f, 140 / 255f, 171 / 255f, 0.91f);
        
        public PieceContainerView(Type type, CeresGraphView graphView): base(type, graphView)
        {
            name = nameof(PieceContainerView);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            Graph.Blackboard.RemoveVariable(mainContainer.Q<PieceIDField>().BindVariable, true);
        }

        protected override void OnSeparatorContextualMenuEvent(ContextualMenuPopulateEvent evt, int separatorIndex)
        {
            base.OnSeparatorContextualMenuEvent(evt, separatorIndex);
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Add Option", a =>
            {
                var bridge = new OptionBridgeView("Option", PortColor);
                AddElement(bridge);
            }));
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Edit PieceID", a =>
            {
                Graph.Blackboard.EditVariable(GetPieceID());
            }));
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Auto Layout", a =>
            {
                new DialogueTreeLayoutConvertor(Graph, this).Layout();
            }));
            if (Application.isPlaying)
            {
                evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Jump to this Piece", a =>
                {
                    DialogueSystem.Get().PlayDialoguePiece(GetPiece().CastPiece().ID);
                },
                _ =>
                {
                    var ds = DialogueSystem.Get();
                    if (!ds.IsPlaying) return DropdownMenuAction.Status.Disabled;
                    // Whether is the container of this piece
                    var piece = GetPiece().CastPiece();
                    if (ds.GetCurrentDialogue()?.GetPiece(piece.ID) != piece) return DropdownMenuAction.Status.Disabled;
                    return DropdownMenuAction.Status.Normal;
                }));
            }
            base.BuildContextualMenu(evt);
        }
        
        public void AddChildElement(IDialogueNodeView node, DialogueGraphView graphView)
        {
            var bridge = new OptionBridgeView("Option", PortColor);
            AddElement(bridge);
            var edge = PortHelper.ConnectPorts(bridge.Child, node.Parent);
            graphView.Add(edge);
        }
        
        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            if (element is not ModuleNodeView moduleNode) return element is OptionBridgeView or ParentBridgeView;
            var behaviorType = moduleNode.GetBehavior();
            var define = behaviorType.GetCustomAttributes<ModuleOfAttribute>().FirstOrDefault(x => x.ContainerType == typeof(Piece));
            if (define == null) return false;
            if (define.AllowMulti) return true;
            return !TryGetModuleNode(behaviorType, out _);
        }
        
        public void GenerateNewPieceID()
        {
            var variable = new PieceID { Name = "New Piece" };
            Graph.Blackboard.AddVariable(variable, false);
            mainContainer.Q<PieceIDField>().value = new PieceID { Name = variable.Name };
        }
        
        protected override IEnumerable<Type> GetExceptModuleTypes()
        {
            return contentContainer.Query<ModuleNodeView>().ToList().Select(x => x.GetBehavior()).Where(x =>
            {
                var define = x.GetCustomAttributes<ModuleOfAttribute>()
                                            .First(attribute => attribute.ContainerType == typeof(Piece));
                return !define.AllowMulti;
            });
        }

        public OptionContainerView[] GetConnectedOptionContainers()
        {
            var bridges = contentContainer.Query<ChildBridgeView>().ToList();
            return bridges.Select(bridge => PortHelper.FindChildNode(bridge.Child)).OfType<OptionContainerView>().ToArray();
        }
    }
    
    [CustomNodeView(typeof(Option))]
    public sealed class OptionContainerView : ContainerNodeView
    {
        protected override Type ParentPortType => typeof(OptionPort);

        protected override Color PortColor => new(57 / 255f, 98 / 255f, 147 / 255f, 0.91f);
        
        public OptionContainerView(Type type, CeresGraphView graphView): base(type, graphView)
        {
            name = nameof(OptionContainerView);
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Auto Layout", a =>
            {
                new DialogueTreeLayoutConvertor(Graph, this).Layout();
            }));
            base.BuildContextualMenu(evt);
        }
        
        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            if (element is not ModuleNodeView moduleNode) return element is ParentBridgeView;
            var behaviorType = moduleNode.GetBehavior();
            if (behaviorType.GetCustomAttributes<ModuleOfAttribute>().All(x => x.ContainerType != typeof(Option))) return false;
            return !TryGetModuleNode(behaviorType, out _);
        }
        
        protected override IEnumerable<Type> GetExceptModuleTypes()
        {
            return contentContainer.Query<ModuleNodeView>().ToList().Select(x => x.GetBehavior()).Where(x =>
            {
                var define = x.GetCustomAttributes<ModuleOfAttribute>().First(attribute => attribute.ContainerType == typeof(Option));
                return !define.AllowMulti;
            });
        }

        public PieceContainerView GetConnectedPieceContainer()
        {
            return PortHelper.FindParentNode(Parent) as PieceContainerView;
        }
    }
}
