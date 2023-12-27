using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        IDialogueTreeView MapTreeView { get; }
        Action<IDialogueNode> OnSelectAction { get; set; }
        void Restore(NodeBehavior behavior);
        void Commit(Stack<IDialogueNode> stack);
        bool Validate(Stack<IDialogueNode> stack);
        Type GetBehavior();
        void SetBehavior(Type nodeBehavior, IDialogueTreeView ownerTreeView = null);
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
        private Type dirtyNodeBehaviorType;
        public Port Parent { private set; get; }
        private readonly VisualElement fieldContainer;
        private readonly TextField description;
        private readonly FieldResolverFactory fieldResolverFactory;
        private readonly List<IFieldResolver> resolvers = new();
        protected readonly List<FieldInfo> fieldInfos = new();
        public Action<IDialogueNode> OnSelectAction { get; set; }
        public IDialogueTreeView MapTreeView { get; private set; }
        //Setting
        private VisualElement settings;
        protected NodeSettingsView settingsContainer;
        protected Button settingButton;
        private bool settingsExpanded = false;
        public Node View => this;
        public override void OnSelected()
        {
            base.OnSelected();
            OnSelectAction?.Invoke(this);
        }
        public IFieldResolver GetFieldResolver(string fieldName)
        {
            int index = fieldInfos.FindIndex(x => x.Name == fieldName);
            if (index != -1) return resolvers[index];
            else return null;
        }

        public DialogueTreeNode()
        {
            fieldResolverFactory = FieldResolverFactory.Instance;
            fieldContainer = new VisualElement();
            description = new TextField();
            GUID = Guid.NewGuid().ToString();
            Initialize();
            InitializeSettings();
        }


        private void InitializeSettings()
        {
            CreateSettingButton();
            settingsContainer = new NodeSettingsView(this)
            {
                visible = false
            };
            settings = new VisualElement();
            // Add Node type specific settings
            settings.Add(CreateSettingsView());
            settingsContainer.Add(settings);
            Add(settingsContainer);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            OnGeometryChanged(null);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            if (settingsContainer != null && settingsContainer.parent != null)
                settingsContainer.parent.Remove(settingsContainer);
        }

        private void CreateSettingButton()
        {
            settingButton = new Button(ToggleSettings) { name = "settings-button" };
            var image = new Image() { name = "icon", scaleMode = ScaleMode.ScaleToFit };
            image.style.backgroundImage = Resources.Load<Texture2D>("NGDT/SettingIcon");
            settingButton.Add(image);
            titleContainer.Add(settingButton);
        }
        protected virtual VisualElement CreateSettingsView() => new Label("Advanced Settings") { name = "header" };
        private void ToggleSettings()
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
                parent.Add(settingsContainer);
                OnGeometryChanged(null);
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
        protected virtual void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (settingButton != null && settingsContainer != null && settingsContainer.parent != null)
            {
                var settingsButtonLayout = settingButton.ChangeCoordinatesTo(settingsContainer.parent, settingButton.layout);
                settingsContainer.style.top = settingsButtonLayout.yMax - 20f;
                settingsContainer.style.left = settingsButtonLayout.xMin - layout.width + 20;
            }
        }
        private void Initialize()
        {
            AddDescription();
            mainContainer.Add(fieldContainer);
            AddParent();
        }

        protected virtual void AddDescription()
        {
            description.RegisterCallback<FocusInEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.On; });
            description.RegisterCallback<FocusOutEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.Auto; });
            mainContainer.Add(description);
        }
        public void Restore(NodeBehavior behavior)
        {
            NodeBehavior = behavior;
            resolvers.ForEach(e => e.Restore(NodeBehavior));
            NodeBehavior.NotifyEditor = MarkAsExecuted;
            description.value = NodeBehavior.description;
            GUID = string.IsNullOrEmpty(behavior.GUID) ? Guid.NewGuid().ToString() : behavior.GUID;
            OnRestore();
        }
        public void CopyFrom(IDialogueNode copyNode)
        {
            var node = copyNode as DialogueTreeNode;
            for (int i = 0; i < node.resolvers.Count; i++)
            {
                resolvers[i].Copy(node.resolvers[i]);
            }
            description.value = node.description.value;
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
            return dirtyNodeBehaviorType;
        }

        public void Commit(Stack<IDialogueNode> stack)
        {
            OnCommit(stack);
            resolvers.ForEach(r => r.Commit(NodeBehavior));
            NodeBehavior.description = description.value;
            NodeBehavior.graphPosition = GetPosition();
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
        public void SetBehavior(Type nodeBehavior, IDialogueTreeView ownerTreeView = null)
        {
            if (ownerTreeView != null)
            {
                MapTreeView = ownerTreeView;
            }
            //Clean up
            if (dirtyNodeBehaviorType != null)
            {
                dirtyNodeBehaviorType = null;
                settingsContainer.Clear();
                fieldContainer.Clear();
                resolvers.Clear();
                fieldInfos.Clear();
            }
            dirtyNodeBehaviorType = nodeBehavior ?? throw new ArgumentNullException(nameof(nodeBehavior));

            var defaultValue = Activator.CreateInstance(nodeBehavior) as NodeBehavior;
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
                        settingsContainer.Add(fieldResolver.GetEditorField(MapTreeView));
                        haveSetting = true;
                    }
                    else
                    {
                        fieldContainer.Add(fieldResolver.GetEditorField(MapTreeView));
                    }
                    resolvers.Add(fieldResolver);
                    fieldInfos.Add(p);
                });
            var label = nodeBehavior.GetCustomAttribute(typeof(AkiLabelAttribute), false) as AkiLabelAttribute;
            title = label?.Title ?? nodeBehavior.Name;
            if (!haveSetting) settingButton.visible = false;
            OnBehaviorSet();
        }
        protected virtual void OnBehaviorSet() { }

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
            OnClearStyle();
        }

        protected abstract void OnClearStyle();
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
        public virtual Rect GetWorldPosition()
        {
            return GetPosition();
        }
    }
}