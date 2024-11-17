using System;
using System.Collections.Generic;
using System.Reflection;
using Ceres.Annotations;
using Ceres.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public interface IDialogueNode
    {
        Node View { get; }
        string GUID { get; }
        Port Parent { get; }
        DialogueTreeView MapTreeView { get; }
        Action<IDialogueNode> OnSelect { get; set; }
        void Restore(NodeBehavior behavior);
        void Commit(Stack<IDialogueNode> stack);
        bool Validate(Stack<IDialogueNode> stack);
        Type GetBehavior();
        void SetBehavior(Type nodeBehavior, DialogueTreeView ownerTreeView = null);
        void CopyFrom(IDialogueNode copyNode);
        NodeBehavior ReplaceBehavior();
        void ClearStyle();
        IFieldResolver GetFieldResolver(string fieldName);
        Rect GetWorldPosition();
    }
    public abstract class DialogueTreeNode : Node, IDialogueNode
    {
        public string GUID { get; private set; }
        
        protected NodeBehavior NodeBehavior { set; get; }
        
        private Type _dirtyNodeBehaviorType;
        
        public Port Parent { private set; get; }
        
        private readonly VisualElement _fieldContainer;
        
        private readonly TextField _description;
        
        private readonly FieldResolverFactory _fieldResolverFactory;
        
        private readonly List<IFieldResolver> _resolvers = new();
        
        protected readonly List<FieldInfo> FieldInfos = new();
        public Action<IDialogueNode> OnSelect { get; set; }
        
        public DialogueTreeView MapTreeView { get; private set; }
        //Setting
        private VisualElement _settings;
        
        protected NodeSettingsView SettingsContainer;
        
        protected Button SettingButton;
        
        private bool _settingsExpanded;
        
        public Node View => this;
        
        public override void OnSelected()
        {
            base.OnSelected();
            OnSelect?.Invoke(this);
        }
        
        public IFieldResolver GetFieldResolver(string fieldName)
        {
            int index = FieldInfos.FindIndex(x => x.Name == fieldName);
            if (index != -1) return _resolvers[index];
            else return null;
        }

        public DialogueTreeNode()
        {
            _fieldResolverFactory = FieldResolverFactory.Instance;
            _fieldContainer = new VisualElement();
            _description = new TextField();
            GUID = Guid.NewGuid().ToString();
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
            _description.value = NodeBehavior.nodeData.description;
            GUID = string.IsNullOrEmpty(behavior.GUID) ? Guid.NewGuid().ToString() : behavior.GUID;
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
            NodeBehavior = Activator.CreateInstance(copyNode.GetBehavior()) as NodeBehavior;
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            GUID = Guid.NewGuid().ToString();
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
            NodeBehavior.nodeData.description = _description.value;
            NodeBehavior.nodeData.graphPosition = GetPosition();
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            NodeBehavior.GUID = GUID;
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
        public void SetBehavior(Type nodeBehavior, DialogueTreeView ownerTreeView = null)
        {
            if (ownerTreeView != null)
            {
                MapTreeView = ownerTreeView;
            }
            //Clean up
            if (_dirtyNodeBehaviorType != null)
            {
                _dirtyNodeBehaviorType = null;
                SettingsContainer.Clear();
                _fieldContainer.Clear();
                _resolvers.Clear();
                FieldInfos.Clear();
            }
            _dirtyNodeBehaviorType = nodeBehavior ?? throw new ArgumentNullException(nameof(nodeBehavior));

            var defaultValue = Activator.CreateInstance(nodeBehavior) as NodeBehavior;
            bool haveSetting = false;
            nodeBehavior.GetEditorWindowFields()
                .ForEach((p) =>
                {
                    var fieldResolver = _fieldResolverFactory.Create(p);
                    fieldResolver.Restore(defaultValue);
                    if (p.GetCustomAttribute<SettingAttribute>() != null)
                    {
                        SettingsContainer.Add(fieldResolver.GetEditorField(MapTreeView));
                        haveSetting = true;
                    }
                    else
                    {
                        _fieldContainer.Add(fieldResolver.GetEditorField(MapTreeView));
                    }
                    _resolvers.Add(fieldResolver);
                    FieldInfos.Add(p);
                });
            var label = nodeBehavior.GetCustomAttribute(typeof(NodeLabelAttribute), false) as NodeLabelAttribute;
            title = label?.Title ?? nodeBehavior.Name;
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
                MapTreeView.DuplicateNode(this);
            }));
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Select Group", (a) =>
            {
                MapTreeView.GroupBlockHandler.SelectGroup(this);
            }));
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("UnSelect Group", (a) =>
            {
                MapTreeView.GroupBlockHandler.UnselectGroup();
            }));
            MapTreeView.ContextualMenuRegistry.BuildContextualMenu(ContextualMenuType.Node, evt, GetBehavior());
        }
        public virtual Rect GetWorldPosition()
        {
            return GetPosition();
        }
    }
}