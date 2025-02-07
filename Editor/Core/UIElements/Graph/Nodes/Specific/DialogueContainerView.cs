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
    [CustomNodeView(typeof(Dialogue))]
    public sealed class DialogueContainerView : ContainerNodeView, IContainChildNode, ILayoutNode
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
                var bridge = new PieceBridgeView(GraphView, PortColor, string.Empty);
                AddElement(bridge);
            }));
        }
        
        public void AddChildElement(IDialogueNodeView node)
        {
            string pieceID = ((PieceContainerView)node).GetPieceID();
            var count = this.Query<PieceBridgeView>().ToList().Count;
            if (!string.IsNullOrEmpty(pieceID) && ((Dialogue)NodeBehavior).ResolvePieceID(count) == pieceID)
            {
                AddElement(new PieceBridgeView(GraphView, PortColor, pieceID));
                return;
            }
            var bridge = new PieceBridgeView(GraphView, PortColor, string.Empty);
            AddElement(bridge);
            var edge = PortHelper.ConnectPorts(bridge.Child, node.Parent);
            GraphView.Add(edge);
        }
        
        protected override void OnCommit(Stack<IDialogueNodeView> stack)
        {
            ((Dialogue)NodeBehavior).referencePieces = new List<string>();
        }
        
        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            if (element is ModuleNodeView moduleNode)
            {
                var nodeType = moduleNode.NodeType;
                var define = nodeType.GetCustomAttributes<ModuleOfAttribute>().FirstOrDefault(x => x.ContainerType == typeof(Dialogue));
                if (define == null) return false;
                if (define.AllowMulti) return true;
                return !TryGetModuleNode(nodeType, out _);
            }
            return element is PieceBridgeView or ParentBridgeView;
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Collect All Pieces", a =>
            {
                var pieces = GraphView.CollectNodes<PieceContainerView>();
                var currentPieces = this.Query<PieceBridgeView>().ToList();
                var addPieces = pieces.Where(x => !currentPieces.Any(p => !string.IsNullOrEmpty(p.PieceID) && p.PieceID == x.GetPieceID()));
                foreach (var piece in addPieces)
                {
                    AddElement(new PieceBridgeView(GraphView, PortColor, piece.GetPieceID()));
                }
            }));
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Auto Layout", a =>
            {
                new DialogueTreeLayoutConvertor(GraphView, this).Layout();
            }));
            base.BuildContextualMenu(evt);
        }

        protected override IEnumerable<Type> GetExceptModuleTypes()
        {
            return contentContainer.Query<ModuleNodeView>().ToList().Select(x => x.NodeType).Where(x =>
            {
                var define = x.GetCustomAttributes<ModuleOfAttribute>()
                    .FirstOrDefault(moduleOfAttribute => moduleOfAttribute.ContainerType == typeof(Dialogue));
                return define!.AllowMulti;
            });
        }
    }
   
}