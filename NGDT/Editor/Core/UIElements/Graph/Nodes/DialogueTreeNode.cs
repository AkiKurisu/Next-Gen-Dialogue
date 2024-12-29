using System;
using System.Collections.Generic;
using System.Reflection;
using Ceres;
using Ceres.Annotations;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public interface IDialogueNode: ICeresNodeView
    {
        Port Parent { get; }
        
        DialogueGraphView MapGraphView { get; }
        
        void Restore(NodeBehavior behavior);
        
        void Commit(Stack<IDialogueNode> stack);
        
        bool Validate(Stack<IDialogueNode> stack);
        
        Type GetBehavior();
        
        void SetBehavior(Type nodeBehavior, DialogueGraphView ownerGraphView = null);
        
        void CopyFrom(IDialogueNode copyNode);
        
        NodeBehavior ReplaceBehavior();
        
        void ClearStyle();
        
        IFieldResolver GetFieldResolver(string fieldName);
        
        Rect GetWorldPosition();
    }
    
    public abstract class DialogueTreeNode : Node, IDialogueNode
    {
        public string Guid { get; private set; }
        
        protected NodeBehavior NodeBehavior { set; get; }
        
        private Type _dirtyNodeBehaviorType;
        
        public Port Parent { private set; get; }
        
        private readonly VisualElement _fieldContainer;
        
        private readonly TextField _description;
        
        private readonly FieldResolverFactory _fieldResolverFactory;
        
        private readonly List<IFieldResolver> _resolvers = new();

        private readonly List<FieldInfo> _fieldInfos = new();
        
        public DialogueGraphView MapGraphView { get; private set; }
        
        //Setting
        private VisualElement _settings;
        
        protected NodeSettingsView SettingsContainer;
        
        protected Button SettingButton;
        
        private bool _settingsExpanded;
        
        public Node NodeElement => this;
        
        public IFieldResolver GetFieldResolver(string fieldName)
        {
            int index = _fieldInfos.FindIndex(x => x.Name == fieldName);
            if (index != -1) return _resolvers[index];
            else return null;
        }

        public DialogueTreeNode()
        {
            _fieldResolverFactory = FieldResolverFactory.Get();
            _fieldContainer = new VisualElement();
            _description = new TextField();
            Guid = System.Guid.NewGuid().ToString();
            Initialize();
            InitializeSettings();
        }


        private void InitializeSettings()
        {
            CreateSettingButton();
            SettingsContainer = new NodeSettingsView(this)
            {
                visible = false
            };
            _settings = new VisualElement();
            // Add Node type specific settings
            _settings.Add(CreateSettingsView());
            SettingsContainer.Add(_settings);
            Add(SettingsContainer);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            OnGeometryChanged(null);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            if (SettingsContainer != null && SettingsContainer.parent != null)
                SettingsContainer.parent.Remove(SettingsContainer);
        }

        private void CreateSettingButton()
        {
            SettingButton = new Button(ToggleSettings) { name = "settings-button" };
            var image = new Image() { name = "icon", scaleMode = ScaleMode.ScaleToFit };
            image.style.backgroundImage = Resources.Load<Texture2D>("NGDT/SettingIcon");
            SettingButton.Add(image);
            titleContainer.Add(SettingButton);
        }
        
        protected virtual VisualElement CreateSettingsView() => new Label("Advanced Settings") { name = "header" };
        
        private void ToggleSettings()
        {
            _settingsExpanded = !_settingsExpanded;
            if (_settingsExpanded)
                OpenSettings();
            else
                CloseSettings();
        }

        public void OpenSettings()
        {
            if (SettingsContainer != null)
            {
                SettingButton.AddToClassList("clicked");
                SettingsContainer.visible = true;
                parent.Add(SettingsContainer);
                OnGeometryChanged(null);
                _settingsExpanded = true;
            }
        }

        public void CloseSettings()
        {
            if (SettingsContainer != null)
            {
                SettingButton.RemoveFromClassList("clicked");
                SettingsContainer.visible = false;
                _settingsExpanded = false;
            }
        }
        
        protected virtual void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (SettingButton != null && SettingsContainer != null && SettingsContainer.parent != null)
            {
                var settingsButtonLayout = SettingButton.ChangeCoordinatesTo(SettingsContainer.parent, SettingButton.layout);
                SettingsContainer.style.top = settingsButtonLayout.yMax - 20f;
                SettingsContainer.style.left = settingsButtonLayout.xMin - layout.width + 20;
            }
        }
        
        private void Initialize()
        {
            AddDescription();
            mainContainer.Add(_fieldContainer);
            AddParent();
        }

        protected virtual void AddDescription()
        {
            _description.RegisterCallback<FocusInEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.On; });
            _description.RegisterCallback<FocusOutEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.Auto; });
            mainContainer.Add(_description);
        }
        
        public void Restore(NodeBehavior behavior)
        {
            NodeBehavior = behavior;
            _resolvers.ForEach(e => e.Restore(NodeBehavior));
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            _description.value = NodeBehavior.NodeData.description;
            Guid = string.IsNullOrEmpty(behavior.Guid) ? System.Guid.NewGuid().ToString() : behavior.Guid;
            OnRestore();
        }
        
        public void CopyFrom(IDialogueNode copyNode)
        {
            var node = copyNode as DialogueTreeNode;
            for (int i = 0; i < node._resolvers.Count; i++)
            {
                _resolvers[i].Copy(node._resolvers[i]);
            }
            _description.value = node._description.value;
            NodeBehavior = (NodeBehavior)Activator.CreateInstance(copyNode.GetBehavior());
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            Guid = System.Guid.NewGuid().ToString();
            OnRestore();
        }

        protected virtual void OnRestore()
        {

        }

        public NodeBehavior ReplaceBehavior()
        {
            NodeBehavior = Activator.CreateInstance(GetBehavior()) as NodeBehavior;
            return NodeBehavior;
        }

        protected virtual void AddParent()
        {
            Parent = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Port));
            Parent.portName = "Parent";
            inputContainer.Add(Parent);
        }

        protected Port CreateChildPort()
        {
            var port = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(Port));
            port.portName = "Child";
            return port;
        }

        public Type GetBehavior()
        {
            return _dirtyNodeBehaviorType;
        }

        public void Commit(Stack<IDialogueNode> stack)
        {
            OnCommit(stack);
            _resolvers.ForEach(r => r.Commit(NodeBehavior));
            NodeBehavior.NodeData.description = _description.value;
            NodeBehavior.NodeData.graphPosition = GetPosition();
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            NodeBehavior.Guid = Guid;
        }
        protected abstract void OnCommit(Stack<IDialogueNode> stack);

        public bool Validate(Stack<IDialogueNode> stack)
        {
            var valid = GetBehavior() != null && OnValidate(stack);
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

        protected abstract bool OnValidate(Stack<IDialogueNode> stack);
        
        public void SetBehavior(Type nodeBehavior, DialogueGraphView ownerGraphView = null)
        {
            if (ownerGraphView != null)
            {
                MapGraphView = ownerGraphView;
            }
            //Clean up
            if (_dirtyNodeBehaviorType != null)
            {
                _dirtyNodeBehaviorType = null;
                SettingsContainer.Clear();
                _fieldContainer.Clear();
                _resolvers.Clear();
                _fieldInfos.Clear();
            }
            _dirtyNodeBehaviorType = nodeBehavior ?? throw new ArgumentNullException(nameof(nodeBehavior));

            var defaultValue = Activator.CreateInstance(nodeBehavior) as NodeBehavior;
            bool haveSetting = false;
            nodeBehavior.GetGraphEditorPropertyFields()
                .ForEach((p) =>
                {
                    var fieldResolver = _fieldResolverFactory.Create(p);
                    fieldResolver.Restore(defaultValue);
                    if (p.GetCustomAttribute<SettingAttribute>() != null)
                    {
                        SettingsContainer.Add(fieldResolver.GetField(MapGraphView));
                        haveSetting = true;
                    }
                    else
                    {
                        _fieldContainer.Add(fieldResolver.GetField(MapGraphView));
                    }
                    _resolvers.Add(fieldResolver);
                    _fieldInfos.Add(p);
                });
            title = CeresLabel.GetLabel(nodeBehavior);
            if (!haveSetting) SettingButton.visible = false;
            OnBehaviorSet();
        }
        protected virtual void OnBehaviorSet() { }

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
                MapGraphView.DuplicateNode(this);
            }));
            MapGraphView.ContextualMenuRegistry.BuildContextualMenu(ContextualMenuType.Node, evt, GetBehavior());
        }
        
        public virtual Rect GetWorldPosition()
        {
            return GetPosition();
        }
    }
}