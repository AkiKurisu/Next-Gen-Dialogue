using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ceres.Annotations;
using Ceres.Editor;
using Ceres.Editor.Graph;
using Ceres.Utilities;
using Kurisu.NGDS;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Node = UnityEditor.Experimental.GraphView.Node;
namespace Kurisu.NGDT.Editor
{
    public abstract class ContainerNode : StackNode, IDialogueNodeView, ILayoutNode
    {
        protected ContainerNode(Type type, CeresGraphView graphView)
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
            Initialize(type, (DialogueGraphView)graphView);
            styleSheets.Add(CeresGraphView.GetOrLoadStyleSheet(NextGenDialogueSettings.NodeStylePath));
        }
        
        public Node NodeElement => this;

        protected virtual Port.Capacity PortCapacity => Port.Capacity.Single;

        protected abstract Color PortColor { get; }
        
        private readonly TextField _description;

        protected abstract Type ParentPortType { get; }
        
        public string Guid { get; private set; }
        
        protected NGDT.DialogueNode NodeBehavior { get; private set;  }
        
        
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
        
        public void Restore(NGDT.DialogueNode dialogueNode)
        {
            NodeBehavior = dialogueNode;
            _resolvers.ForEach(e => e.Restore(NodeBehavior));
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            _description.value = NodeBehavior.NodeData.description;
            Guid = string.IsNullOrEmpty(dialogueNode.Guid) ? System.Guid.NewGuid().ToString() : dialogueNode.Guid;
        }
        
        public void CopyFrom(IDialogueNodeView copyNode)
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
                    var newNode = _copyMap[x.GetHashCode()] = Graph.DuplicateNode(x) as ModuleNode;
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
            NodeBehavior = (NGDT.DialogueNode)Activator.CreateInstance(copyNode.GetBehavior());
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            Guid = System.Guid.NewGuid().ToString();
        }
        
        private readonly Dictionary<int, Node> _copyMap = new();
        
        internal IReadOnlyDictionary<int, Node> GetCopyMap()
        {
            return _copyMap;
        }
        
        public NGDT.DialogueNode Compile()
        {
            NodeBehavior = (NGDT.DialogueNode)Activator.CreateInstance(GetBehavior());
            return NodeBehavior;
        }
        
        public Type GetBehavior()
        {
            return _nodeType;
        }

        public void Commit(Stack<IDialogueNodeView> stack)
        {
            OnCommit(stack);
            var nodes = contentContainer.Query<ModuleNode>().ToList();
            nodes.ForEach(x =>
            {
                ((Container)NodeBehavior).AddChild(x.Compile());
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
        
        protected virtual void OnCommit(Stack<IDialogueNodeView> stack) { }
        
        public bool Validate(Stack<IDialogueNodeView> stack)
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
        
        protected virtual void Initialize(Type nodeType, DialogueGraphView graphView)
        {
            Assert.IsNotNull(nodeType);
            Assert.IsNotNull(graphView);
            Graph = graphView;
            _nodeType = nodeType;

            var defaultValue = (NGDT.DialogueNode)Activator.CreateInstance(nodeType);
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
            contentContainer.Query<ModuleNode>()
                            .ForEach(x => x.ClearStyle());
            contentContainer.Query<ChildBridge>()
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
            var moduleNode = (ModuleNode)NodeViewFactory.Get().CreateInstance(typeof(T), Graph);
            moduleNode.Restore(module);
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
    
    [CustomNodeView(typeof(Dialogue))]
    public sealed class DialogueContainer : ContainerNode, IContainChild, ILayoutNode
    {
        protected override Type ParentPortType => typeof(DialoguePort);
        protected override Color PortColor => new(97 / 255f, 95 / 255f, 212 / 255f, 0.91f);

        VisualElement ILayoutNode.View => this;
        
        public DialogueContainer(Type type, CeresGraphView graphView): base(type, graphView)
        {
            name = nameof(DialogueContainer);
        }

        protected override void OnSeparatorContextualMenuEvent(ContextualMenuPopulateEvent evt, int separatorIndex)
        {
            base.OnSeparatorContextualMenuEvent(evt, separatorIndex);
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Add Piece", a =>
            {
                var bridge = new PieceBridge(Graph, PortColor, string.Empty);
                AddElement(bridge);
            }));
        }
        
        public void AddChildElement(IDialogueNodeView node, DialogueGraphView graphView)
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
        
        protected override void OnCommit(Stack<IDialogueNodeView> stack)
        {
            ((Dialogue)NodeBehavior).referencePieces = new List<string>();
        }
        
        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
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
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Collect All Pieces", a =>
            {
                var pieces = Graph.CollectNodes<PieceContainer>();
                var currentPieces = this.Query<PieceBridge>().ToList();
                var addPieces = pieces.Where(x => !currentPieces.Any(p => !string.IsNullOrEmpty(p.PieceID) && p.PieceID == x.GetPieceID()));
                foreach (var piece in addPieces)
                {
                    AddElement(new PieceBridge(Graph, PortColor, piece.GetPieceID()));
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
            return contentContainer.Query<ModuleNode>().ToList().Select(x => x.GetBehavior()).Where(x =>
            {
                var define = x.GetCustomAttributes<ModuleOfAttribute>()
                    .FirstOrDefault(moduleOfAttribute => moduleOfAttribute.ContainerType == typeof(Dialogue));
                return define!.AllowMulti;
            });
        }
    }
    
    [CustomNodeView(typeof(Piece))]
    public sealed class PieceContainer : ContainerNode, IContainChild
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
        
        public PieceContainer(Type type, CeresGraphView graphView): base(type, graphView)
        {
            name = nameof(PieceContainer);
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
                var bridge = new OptionBridge("Option", PortColor);
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
            var bridge = new OptionBridge("Option", PortColor);
            AddElement(bridge);
            var edge = PortHelper.ConnectPorts(bridge.Child, node.Parent);
            graphView.Add(edge);
        }
        
        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            if (element is not ModuleNode moduleNode) return element is OptionBridge or ParentBridge;
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
            return contentContainer.Query<ModuleNode>().ToList().Select(x => x.GetBehavior()).Where(x =>
            {
                var define = x.GetCustomAttributes<ModuleOfAttribute>()
                                            .First(attribute => attribute.ContainerType == typeof(Piece));
                return !define.AllowMulti;
            });
        }
    }
    
    [CustomNodeView(typeof(Option))]
    public sealed class OptionContainer : ContainerNode
    {
        protected override Type ParentPortType => typeof(OptionPort);

        protected override Color PortColor => new(57 / 255f, 98 / 255f, 147 / 255f, 0.91f);
        
        public OptionContainer(Type type, CeresGraphView graphView): base(type, graphView)
        {
            name = nameof(OptionContainer);
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
