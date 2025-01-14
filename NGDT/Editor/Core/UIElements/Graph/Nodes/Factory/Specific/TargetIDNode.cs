using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [CustomNodeView(typeof(TargetIDModule))]
    public class TargetIDNode : ModuleNode, ILayoutNode
    {
        private readonly Port _childPort;
        
        private static readonly Color PortColor = new(97 / 255f, 95 / 255f, 212 / 255f, 0.91f);
        
        private PieceIDField _targetIDField;
        
        private Toggle _useReferenceField;
        
        VisualElement ILayoutNode.View => this;
        
        public TargetIDNode()
        {
            AddToClassList(nameof(ModuleNode));
            _childPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(PiecePort));
            _childPort.portName = string.Empty;
            _childPort.portColor = PortColor;
            outputContainer.Add(_childPort);
        }
        
        public bool TryGetPiece(out PieceContainer pieceContainer)
        {
            if (_useReferenceField.value || !_childPort.connected)
            {
                pieceContainer = null;
                return false;
            }
            pieceContainer = PortHelper.FindChildNode(_childPort) as PieceContainer;
            return pieceContainer != null;
        }
        
        protected sealed override void OnPostSetNodeType()
        {
            _useReferenceField = ((BoolResolver)GetFieldResolver("useReference")).BaseField;
            _targetIDField = ((PieceIDResolver)GetFieldResolver("targetID")).BaseField;
            _useReferenceField.RegisterValueChangedCallback(x => OnToggle(x.newValue));
            OnToggle(_useReferenceField.value);
        }
        
        protected sealed override async void OnRestore()
        {
            // Connect after loaded
            await Task.Delay(1);
            var node = GraphView.FindPiece(_targetIDField.value.Name);
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
            _targetIDField.SetEnabled(useReference);
        }
        
        protected sealed override bool OnValidate(Stack<IDialogueNode> stack)
        {
            // Prevent circle validation
            return true;
        }

        protected sealed override void OnCommit(Stack<IDialogueNode> stack)
        {
            if (_useReferenceField.value) return;
            if (!_childPort.connected) return;
            
            // Use weak reference instead of serialize reference
            var node = PortHelper.FindChildNode(_childPort) as PieceContainer;
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
