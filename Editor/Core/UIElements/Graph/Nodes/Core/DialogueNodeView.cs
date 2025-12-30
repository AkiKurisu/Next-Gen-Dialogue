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
using NodeElement = UnityEditor.Experimental.GraphView.Node;

namespace NextGenDialogue.Graph.Editor
{
    public interface IDialogueNodeView: ICeresNodeView
    {
        /// <summary>
        /// Parent node port
        /// </summary>
        Port Parent { get; }
        
        /// <summary>
        /// Source graph view
        /// </summary>
        DialogueGraphView GraphView { get; }
        
        /// <summary>
        /// Dialogue node type
        /// </summary>
        Type NodeType { get; }
        
        void SetNodeInstance(DialogueNode dialogueNode);
        
        void SerializeNode(IDialogueGraphSerializeVisitor visitor, DialogueNode dialogueNode);
        
        void ClearStyle();
        
        IFieldResolver GetFieldResolver(string fieldName);
    }
    
    public abstract class DialogueNodeView : NodeElement, IDialogueNodeView
    {
        public string Guid { get; private set; }
        
        public DialogueGraphView GraphView { get; private set; }
        
        public Port Parent { private set; get; }
        
        public Type NodeType { get; private set; }
        
        /// <summary>
        /// Cached dialogue node instance
        /// </summary>
        protected DialogueNode NodeInstance { get; private set;}
        
        private VisualElement _fieldContainer;

        protected TextField DescriptionText { get; private set; }

        private FieldResolverFactory _fieldResolverFactory;
        
        private readonly List<IFieldResolver> _resolvers = new();

        private readonly List<FieldInfo> _fieldInfos = new();
        
        private readonly NodeSettingsView _nodeSettingsView;
        
        public NodeElement NodeElement => this;
        
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
            if (CanAddParent())
            {
                AddParent();
            }
        }
        
        public void SetNodeInstance(DialogueNode dialogueNode)
        {
            NodeInstance = dialogueNode;
            _resolvers.ForEach(e => e.Restore(NodeInstance));
            NodeInstance.NotifyEditor = MarkAsExecuted;
            DescriptionText.value = NodeInstance.NodeData.description;
            Guid = string.IsNullOrEmpty(dialogueNode.Guid) ? System.Guid.NewGuid().ToString() : dialogueNode.Guid;
            OnRestore();
        }

        protected virtual void OnRestore()
        {

        }

        public DialogueNode CompileNode()
        {
            NodeInstance = (DialogueNode)Activator.CreateInstance(NodeType);
            return NodeInstance;
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

        public void SerializeNode(IDialogueGraphSerializeVisitor visitor, DialogueNode dialogueNode)
        {
            NodeInstance = dialogueNode;
            NodeInstance.NodeData.description = DescriptionText.value;
            NodeInstance.NodeData.graphPosition = GetPosition();
            NodeInstance.NotifyEditor = MarkAsExecuted;
            NodeInstance.Guid = Guid;
            OnSerialize();
            _resolvers.ForEach(r => r.Commit(NodeInstance));
        }
        
        protected virtual void OnSerialize() {}

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
            OnClearStyle();
        }

        protected abstract void OnClearStyle();
    }
}