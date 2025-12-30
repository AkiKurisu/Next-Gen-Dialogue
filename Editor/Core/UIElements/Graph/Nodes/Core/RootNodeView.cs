using System;
using System.Collections.Generic;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NextGenDialogue.Graph.Editor
{
    [CustomNodeView(typeof(Root))]
    public sealed class RootNodeView : DialogueNodeView, ILayoutNode
    {
        public readonly Port Child;
        
        public VisualElement View => this;

        public RootNodeView(Type type, CeresGraphView graphView): base(type, graphView)
        {
            title = nameof(Root);
            Child = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialoguePort));
            Child.portName = "Child";
            Child.portColor = new Color(191 / 255f, 117 / 255f, 255 / 255f, 0.91f);
            outputContainer.Add(Child);
            capabilities &= ~Capabilities.Copiable;
            capabilities &= ~Capabilities.Deletable;
            capabilities &= ~Capabilities.Movable;
            RefreshExpandedState();
            RefreshPorts();
        }
        
        protected override bool CanAddParent()
        {
            return false;
        }

        protected override void InitializeVisualElements()
        {
            base.InitializeVisualElements();
            DescriptionText.RemoveFromHierarchy();
        }

        protected override void OnRestore()
        {
            ((Root)NodeInstance).UpdateEditor = ClearStyle;
        }
        
        protected override void OnSerialize()
        {
            ((Root)NodeInstance).UpdateEditor = ClearStyle;
        }
        
        protected override void OnClearStyle()
        {
            GraphView.CollectNodes<ContainerNodeView>().ForEach(view => view.ClearStyle());
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }
        
        public IReadOnlyList<ILayoutNode> GetLayoutChildren()
        {
            return new[] { (DialogueContainerView)PortHelper.FindChildNode(Child) };
        }
    }
}