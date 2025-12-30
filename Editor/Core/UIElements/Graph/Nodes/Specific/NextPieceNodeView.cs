using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace NextGenDialogue.Graph.Editor
{
    [CustomNodeView(typeof(NextPieceModule))]
    public class NextPieceNodeView : ModuleNodeView, ILayoutNode
    {
        private Port _childPort;
        
        private static readonly Color PortColor = new(97 / 255f, 95 / 255f, 212 / 255f, 0.91f);
        
        private PieceIDField _nextIDField;
        
        private Toggle _useReferenceField;

        VisualElement ILayoutNode.View => this;

        public NextPieceNodeView(Type type, CeresGraphView graphView): base(type, graphView)
        {

        }
        
        protected override void Initialize(Type nodeType, DialogueGraphView graphView)
        {
            base.Initialize(nodeType, graphView);
            _useReferenceField = ((BoolResolver)GetFieldResolver("useReference")).BaseField;
            _nextIDField = ((PieceIDResolver)GetFieldResolver("nextID")).BaseField;
            _useReferenceField.RegisterValueChangedCallback(x => OnToggle(x.newValue));
            _childPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(PiecePort));
            _childPort.portName = string.Empty;
            _childPort.portColor = PortColor;
            outputContainer.Add(_childPort);
            OnToggle(_useReferenceField.value);
        }
        
        protected sealed override async void OnRestore()
        {
            //Connect after loaded
            await Task.Delay(1);
            var node = GraphView.FindPiece(_nextIDField.value.Name);
            if (node != null)
            {
                var edge = PortHelper.ConnectPorts(_childPort, node.Parent);
                GraphView.Add(edge);
            }
            OnToggle(_useReferenceField.value);
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
            _nextIDField.SetEnabled(useReference);
        }

        protected sealed override void OnSerialize()
        {
            if (_useReferenceField.value) return;
            if (_childPort.connected)
            {
                //Use weak reference instead of serialize reference
                var node = (PieceContainerView)PortHelper.FindChildNode(_childPort);
                _nextIDField.value.Name = node.GetPieceID();
            }
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

        public IReadOnlyList<ILayoutNode> GetLayoutChildren()
        {
            var list = new List<ILayoutNode>();
            if (TryGetPiece(out var pieceContainer))
                list.Add(pieceContainer);
            return list;
        }
    }
}
