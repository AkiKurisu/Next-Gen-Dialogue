using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ceres.Annotations;
using Ceres.Editor.Graph;
using Ceres.Utilities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using NodeElement = UnityEditor.Experimental.GraphView.Node;

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
        
        public NodeElement NodeElement => this;

        protected virtual Port.Capacity PortCapacity => Port.Capacity.Single;

        protected abstract Color PortColor { get; }
        
        private readonly TextField _description;

        protected abstract Type ParentPortType { get; }
        
        public string Guid { get; private set; }
        
        public DialogueNode NodeInstance { get; private set;}
        
        private readonly Label _titleLabel;

        public Type NodeType { get; private set; }
        
        public Port Parent { get; }
        
        
        private readonly VisualElement _fieldContainer;
        
        
        private readonly FieldResolverFactory _fieldResolverFactory;
        
        
        private readonly List<IFieldResolver> _resolvers = new();
        
        
        private readonly List<FieldInfo> _fieldInfos = new();

        public DialogueGraphView GraphView { get; private set; }
        
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
            NodeInstance = dialogueNode;
            _resolvers.ForEach(e => e.Restore(NodeInstance));
            NodeInstance.NotifyEditor = MarkAsExecuted;
            _description.value = NodeInstance.NodeData.description;
            Guid = string.IsNullOrEmpty(dialogueNode.Guid) ? System.Guid.NewGuid().ToString() : dialogueNode.Guid;
        }
        
        public DialogueNode CompileNode()
        {
            NodeInstance = (DialogueNode)Activator.CreateInstance(NodeType);
            return NodeInstance;
        }

        public void SerializeNode(IDialogueGraphSerializeVisitor visitor, DialogueNode dialogueNode)
        {
            NodeInstance = dialogueNode;
            NodeInstance.NodeData.description = _description.value;
            NodeInstance.NodeData.graphPosition = GetPosition();
            NodeInstance.NotifyEditor = MarkAsExecuted;
            NodeInstance.Guid = Guid;
            visitor.Visit(this, (ContainerNode)NodeInstance);
            _resolvers.ForEach(resolver => resolver.Commit(NodeInstance));
        }

        private void Initialize(Type nodeType, DialogueGraphView graphView)
        {
            Assert.IsNotNull(nodeType);
            Assert.IsNotNull(graphView);
            GraphView = graphView;
            NodeType = nodeType;

            var defaultValue = (DialogueNode)Activator.CreateInstance(nodeType);
            nodeType.GetGraphEditorPropertyFields().ForEach(p =>
                {
                    var fieldResolver = _fieldResolverFactory.Create(p);
                    fieldResolver.Restore(defaultValue);
                    _fieldContainer.Add(fieldResolver.GetField(GraphView));
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
                provider.Init(this, GraphView, NodeSearchContext.Default, GetExceptModuleTypes());
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
            moduleNodeView = contentContainer.Query<ModuleNodeView>().ToList().FirstOrDefault(x => x.NodeType == typeof(T));
            return moduleNodeView != null;
        }
        
        public bool TryGetModuleNode(Type type, out ModuleNodeView moduleNodeView)
        {
            moduleNodeView = contentContainer.Query<ModuleNodeView>().ToList().FirstOrDefault(x => x.NodeType == type);
            return moduleNodeView != null;
        }
        
        public ModuleNodeView GetModuleNode<T>(int index) where T : Module
        {
            var nodes = GetModuleNodes<T>();
            return nodes[index];
        }
        
        public ModuleNodeView[] GetModuleNodes<T>() where T : Module
        {
            return contentContainer.Query<ModuleNodeView>()
                .ToList()
                .Where(nodeView => nodeView.NodeType == typeof(T))
                .ToArray();
        }
        
        public ModuleNodeView AddModuleNode<T>(T module) where T : Module, new()
        {
            var moduleNode = (ModuleNodeView)NodeViewFactory.Get().CreateInstance(typeof(T), GraphView);
            moduleNode.SetNodeInstance(module);
            GraphView.AddNodeView(moduleNode);
            AddElement(moduleNode);
            return moduleNode;
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            GraphView.ContextualMenuRegistry.BuildContextualMenu(ContextualMenuType.Node, evt, NodeType);
        }
        
        public IReadOnlyList<ILayoutNode> GetLayoutChildren()
        {
            var list = new List<ILayoutNode>();
            var nodes = contentContainer
                 .Query<NodeElement>()
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
}
