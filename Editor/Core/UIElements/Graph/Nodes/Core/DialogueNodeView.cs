using System;
using System.Collections.Generic;
using System.Reflection;
using Ceres.Utilities;
using Ceres.Annotations;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace NextGenDialogue.Graph.Editor
{
    public interface IDialogueNodeView: ICeresNodeView
    {
        Port Parent { get; }
        
        DialogueGraphView GraphView { get; }
        
        Type NodeType { get; }
        
        void SetNodeInstance(DialogueNode dialogueNode);
        
        void Commit(Stack<IDialogueNodeView> stack);
        
        bool Validate(Stack<IDialogueNodeView> stack);
        
        void CopyFrom(IDialogueNodeView copyNode);
        
        DialogueNode Compile();
        
        void ClearStyle();
        
        IFieldResolver GetFieldResolver(string fieldName);
        
        Rect GetWorldPosition();
    }
    
    public abstract class DialogueNodeView : UnityEditor.Experimental.GraphView.Node, IDialogueNodeView
    {
        public string Guid { get; private set; }
        
        public DialogueGraphView GraphView { get; private set; }
        
        public Port Parent { private set; get; }
        
        public Type NodeType { get; private set; }
        
        protected DialogueNode NodeBehavior { set; get; }
        
        private VisualElement _fieldContainer;

        protected TextField DescriptionText { get; private set; }

        private FieldResolverFactory _fieldResolverFactory;
        
        private readonly List<IFieldResolver> _resolvers = new();

        private readonly List<FieldInfo> _fieldInfos = new();
        
        private readonly NodeSettingsView _nodeSettingsView;
        
        public UnityEditor.Experimental.GraphView.Node NodeElement => this;
        
        public IFieldResolver GetFieldResolver(string fieldName)
        {
            int index = _fieldInfos.FindIndex(x => x.Name == fieldName);
            if (index != -1) return _resolvers[index];
            return null;
        }

        protected DialogueNodeView(Type type, CeresGraphView graphView)
        {
            InitializeVisualElements();
            _nodeSettingsView = this.CreateSettingsView<NodeSettingsView>();
            Initialize(type, (DialogueGraphView)graphView);
        }
        
        
        /// <summary>
        /// Initialize dialogue node view visual elements
        /// </summary>
        protected virtual void InitializeVisualElements()
        {
            styleSheets.Add(CeresGraphView.GetOrLoadStyleSheet(NextGenDialogueSettings.NodeStylePath));
            _fieldResolverFactory = FieldResolverFactory.Get();
            _fieldContainer = new VisualElement();
            DescriptionText = new TextField();
            DescriptionText.RegisterCallback<FocusInEvent>(_ => { Input.imeCompositionMode = IMECompositionMode.On; });
            DescriptionText.RegisterCallback<FocusOutEvent>(_ => { Input.imeCompositionMode = IMECompositionMode.Auto; });
            mainContainer.Add(DescriptionText);
            mainContainer.Add(_fieldContainer);
            if(CanAddParent())
            {
                AddParent();
            }
        }
        
        public void SetNodeInstance(DialogueNode dialogueNode)
        {
            NodeBehavior = dialogueNode;
            _resolvers.ForEach(e => e.Restore(NodeBehavior));
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            DescriptionText.value = NodeBehavior.NodeData.description;
            Guid = string.IsNullOrEmpty(dialogueNode.Guid) ? System.Guid.NewGuid().ToString() : dialogueNode.Guid;
            OnRestore();
        }
        
        public void CopyFrom(IDialogueNodeView copyNode)
        {
            var node = (DialogueNodeView)copyNode;
            for (int i = 0; i < node._resolvers.Count; i++)
            {
                _resolvers[i].Copy(node._resolvers[i]);
            }
            DescriptionText.value = node.DescriptionText.value;
            NodeBehavior = (DialogueNode)Activator.CreateInstance(copyNode.NodeType);
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            Guid = System.Guid.NewGuid().ToString();
            OnRestore();
        }

        protected virtual void OnRestore()
        {

        }

        public DialogueNode Compile()
        {
            NodeBehavior = (DialogueNode)Activator.CreateInstance(NodeType);
            return NodeBehavior;
        }

        protected virtual bool CanAddParent()
        {
            return true;
        }

        private void AddParent()
        {
            Parent = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Port));
            Parent.portName = "Parent";
            inputContainer.Add(Parent);
        }

        protected static Port CreateChildPort()
        {
            var port = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(Port));
            port.portName = "Child";
            return port;
        }

        public void Commit(Stack<IDialogueNodeView> stack)
        {
            OnCommit(stack);
            _resolvers.ForEach(r => r.Commit(NodeBehavior));
            NodeBehavior.NodeData.description = DescriptionText.value;
            NodeBehavior.NodeData.graphPosition = GetPosition();
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            NodeBehavior.Guid = Guid;
        }
        protected abstract void OnCommit(Stack<IDialogueNodeView> stack);

        public bool Validate(Stack<IDialogueNodeView> stack)
        {
            var valid = NodeType != null && OnValidate(stack);
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

        protected abstract bool OnValidate(Stack<IDialogueNodeView> stack);

        protected virtual void Initialize(Type nodeType, DialogueGraphView graphView)
        {
            Assert.IsNotNull(nodeType);
            Assert.IsNotNull(graphView);
            GraphView = graphView;
            NodeType = nodeType;
            Guid = System.Guid.NewGuid().ToString();
            var defaultValue = (DialogueNode)Activator.CreateInstance(nodeType);
            var haveSetting = false;
            nodeType.GetGraphEditorPropertyFields()
                .ForEach((p) =>
                {
                    var fieldResolver = _fieldResolverFactory.Create(p);
                    fieldResolver.Restore(defaultValue);
                    if (p.GetCustomAttribute<SettingAttribute>() != null)
                    {
                        _nodeSettingsView.SettingsElement.Add(fieldResolver.GetField(GraphView));
                        haveSetting = true;
                    }
                    else
                    {
                        _fieldContainer.Add(fieldResolver.GetField(GraphView));
                    }
                    _resolvers.Add(fieldResolver);
                    _fieldInfos.Add(p);
                });
            title = CeresLabel.GetLabel(nodeType);
            if (!haveSetting) _nodeSettingsView.DisableSettings();
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
            OnClearStyle();
        }

        protected abstract void OnClearStyle();
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Duplicate", (a) =>
            {
                GraphView.DuplicateNode(this);
            }));
            GraphView.ContextualMenuRegistry.BuildContextualMenu(ContextualMenuType.Node, evt, NodeType);
        }
        
        public virtual Rect GetWorldPosition()
        {
            return GetPosition();
        }
    }
}