using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NextGenDialogue.Graph.Editor
{
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
                new DialogueTreeLayoutConvertor(GraphView, this).Layout();
            }));
            base.BuildContextualMenu(evt);
        }
        
        protected override void OnSeparatorContextualMenuEvent(ContextualMenuPopulateEvent evt, int separatorIndex)
        {
            base.OnSeparatorContextualMenuEvent(evt, separatorIndex);
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Add Piece", a =>
            {
                // Create piece
                var piece = (PieceContainerView)GraphView.CreateNextContainer(this);
                piece.AddModuleNode(new ContentModule());
                // Link to bridge node
                this.ConnectContainerNodes(piece);
            }));
        }
        
        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            if (element is not ModuleNodeView moduleNode) return element is ParentBridgeView;
            var behaviorType = moduleNode.NodeType;
            if (behaviorType.GetCustomAttributes<ModuleOfAttribute>().All(x => x.ContainerType != typeof(Option))) return false;
            return !TryGetModuleNode(behaviorType, out _);
        }
        
        protected override IEnumerable<Type> GetExceptModuleTypes()
        {
            return contentContainer.Query<ModuleNodeView>().ToList().Select(x => x.NodeType).Where(x =>
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