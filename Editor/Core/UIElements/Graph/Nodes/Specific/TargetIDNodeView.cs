using System;
using System.Collections.Generic;
using System.Linq;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NextGenDialogue.Graph.Editor
{
    [CustomNodeView(typeof(TargetIDModule))]
    public class TargetIDNodeView : ModuleNodeView, ILayoutNode
    {
        private Port _childPort;
        
        private static readonly Color PortColor = new(97 / 255f, 95 / 255f, 212 / 255f, 0.91f);
        
        private PieceIDField _targetIDField;
        
        private Toggle _useReferenceField;
        
        VisualElement ILayoutNode.View => this;
        
        public TargetIDNodeView(Type type, CeresGraphView graphView): base(type, graphView)
        {
        }

        private bool TryGetPiece(out PieceContainerView pieceContainerView)
        {
            if (_useReferenceField.value || !_childPort.connected)
            {
                pieceContainerView = null;
                return false;
            }
            pieceContainerView = PortHelper.FindChildNode(_childPort) as PieceContainerView;
            return pieceContainerView != null;
        }
        
        protected override void Initialize(Type nodeType, DialogueGraphView graphView)
        {
            base.Initialize(nodeType, graphView);
            _useReferenceField = ((BoolResolver)GetFieldResolver("useReference")).BaseField;
            _targetIDField = ((PieceIDResolver)GetFieldResolver("targetID")).BaseField;
            _childPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(PiecePort));
            _childPort.portName = string.Empty;
            _childPort.portColor = PortColor;
            outputContainer.Add(_childPort);
            _useReferenceField.RegisterValueChangedCallback(x => OnToggle(x.newValue));
            OnToggle(_useReferenceField.value);
        }
        
        protected sealed override void OnRestore()
        {
            if (_useReferenceField.value) return;
            _childPort.SetEnabled(true);
            _targetIDField.SetEnabled(false);
            // Connect after loaded
            schedule.Execute(FindTargetPieceAndConnect).ExecuteLater(100);
        }

        private void FindTargetPieceAndConnect()
        {
            var node = GraphView.FindPiece(_targetIDField.value.Name);
            if (node == null) return;
            var edge = PortHelper.ConnectPorts(_childPort, node.Parent);
            GraphView.Add(edge);
        }
        
        private void OnToggle(bool useReference)
        {
            if (useReference && _childPort.connected)
            {
                var edge = _childPort.connections.First();
                edge.output.Disconnect(edge);
                edge.input.Disconnect(edge);
                edge.RemoveFromHierarchy();
            }
            _childPort.SetEnabled(!useReference);
            _targetIDField.SetEnabled(useReference);
        }

        protected sealed override void OnSerialize()
        {
            if (_useReferenceField.value) return;
            if (!_childPort.connected) return;
            
            // Use weak reference instead of serialize reference
            var node = PortHelper.FindChildNode(_childPort) as PieceContainerView;
            _targetIDField.value.Name = node!.GetPieceID();
        }

        public IReadOnlyList<ILayoutNode> GetLayoutChildren()
        {
            var list = new List<ILayoutNode>();
            if (TryGetPiece(out var pieceContainer))
                list.Add(pieceContainer);
            return list;
        }
    }

}
