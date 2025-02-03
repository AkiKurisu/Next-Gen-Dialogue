using System;
using System.Collections.Generic;
using System.Linq;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    internal class ParentBridgeView : Node
    {
        public Port Parent { get; }
        
        public ParentBridgeView(Type portType, Port.Capacity capacity) : base()
        {
            AddToClassList("BridgeNode");
            capabilities &= ~Capabilities.Copiable;
            capabilities &= ~Capabilities.Deletable;
            capabilities &= ~Capabilities.Movable;
            title = "Parent";
            Parent = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, capacity, portType);
            Parent.portName = string.Empty;
            inputContainer.Add(Parent);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            //Fix edge remain in the graph though Stack is removed
            if (Parent.connected)
            {
                var edge = Parent.connections.First();
                edge.input?.Disconnect(edge);
                edge.RemoveFromHierarchy();
            }
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            evt.StopPropagation();
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            evt.StopPropagation();
        }
    }
    internal abstract class ChildBridgeView : Node
    {
        public Port Child { get; private set; }
        
        private readonly Color _portColor;
        
        private readonly Type _portType;
        
        public ChildBridgeView(string title, Type portType, Color portColor)
        {
            AddToClassList("BridgeNode");
            capabilities &= ~Capabilities.Copiable;
            _portType = portType;
            _portColor = portColor;
            this.title = title;
            AddChild();
        }
        private void AddChild()
        {
            Child = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, _portType);
            Child.portName = string.Empty;
            Child.portColor = _portColor;
            outputContainer.Add(Child);
        }

        internal void Validate(Stack<IDialogueNodeView> stack)
        {
            OnValidate(stack);
        }

        internal void Commit(NGDT.ContainerNode containerNode, Stack<IDialogueNodeView> stack)
        {
            OnCommit(containerNode, stack);
        }
        protected virtual void OnValidate(Stack<IDialogueNodeView> stack) { }

        protected virtual void OnCommit(NGDT.ContainerNode containerNode, Stack<IDialogueNodeView> stack)
        {
            if (Child.connected)
            {
                var node = PortHelper.FindChildNode(Child);
                stack.Push(node);
                containerNode.AddChild(node.Compile());
            }
        }

        internal void ClearStyle()
        {
            if (Child.connected)
            {
                var node = PortHelper.FindChildNode(Child);
                node.ClearStyle();
            }
        }
        public abstract ChildBridgeView Clone();
    }
    internal class PieceBridgeView : ChildBridgeView, ILayoutNode
    {
        private readonly PieceIDField _pieceIDField;
        
        private bool _useReference;
        
        private readonly DialogueGraphView _graphView;
        
        public string PieceID
        {
            get
            {
                if (_useReference)
                {
                    return _pieceIDField.value.Name;
                }

                if (Child.connected)
                {
                    var node = (PieceContainerView)PortHelper.FindChildNode(Child);
                    return node.GetPieceID();
                }
                return string.Empty;
            }
        }
        
        public VisualElement View => this;
        
        public PieceBridgeView(DialogueGraphView graphView, Color portColor, string pieceIDName)
        : base("Piece", typeof(PiecePort), portColor)
        {
            _graphView = graphView;
            var toggle = new Toggle("Use Reference");
            toggle.RegisterValueChangedCallback(evt => OnToggle(evt.newValue));
            mainContainer.Add(toggle);
            _pieceIDField = new PieceIDField("Reference", true);
            _pieceIDField.BindGraph(graphView);
            _pieceIDField.value = new PieceID() { Name = pieceIDName };
            mainContainer.Add(_pieceIDField);
            toggle.value = !string.IsNullOrEmpty(pieceIDName);
            OnToggle(toggle.value);
        }
        
        private void OnToggle(bool useReference)
        {
            _useReference = useReference;
            if (useReference && Child.connected)
            {
                var edge = Child.connections.First();
                edge.output.Disconnect(edge);
                edge.input.Disconnect(edge);
                edge.RemoveFromHierarchy();
            }
            Child.SetEnabled(!useReference);
            _pieceIDField.SetEnabled(useReference);
        }
        
        protected sealed override void OnCommit(NGDT.ContainerNode containerNode, Stack<IDialogueNodeView> stack)
        {
            if (_useReference)
            {
                var node = _graphView.FindPiece(_pieceIDField.value.Name);
                if (node == null) return;
                ((Dialogue)containerNode).AddPiece(node.GetPiece(), _pieceIDField.value.Name);
            }
            else if (Child.connected)
            {
                var node = (PieceContainerView)PortHelper.FindChildNode(Child);
                ((Dialogue)containerNode).AddPiece(node.GetPiece(), string.Empty);
            }
        }

        public override ChildBridgeView Clone()
        {
            return new PieceBridgeView(_graphView, Child.portColor, _useReference ? _pieceIDField.value.Name : string.Empty);
        }

        public bool TryGetPiece(out PieceContainerView pieceContainerView)
        {
            if (_useReference || !Child.connected)
            {
                pieceContainerView = null;
                return false;
            }
            pieceContainerView = PortHelper.FindChildNode(Child) as PieceContainerView;
            return pieceContainerView != null;
        }
        
        public IReadOnlyList<ILayoutNode> GetLayoutChildren()
        {
            var list = new List<ILayoutNode>();
            if (TryGetPiece(out var piece))
                list.Add(piece);
            return list;
        }
    }
    
    internal class OptionBridgeView : ChildBridgeView, ILayoutNode
    {
        public VisualElement View => this;

        public OptionBridgeView(string title, Color portColor) : base(title, typeof(OptionPort), portColor)
        {
        }
        protected override void OnValidate(Stack<IDialogueNodeView> stack)
        {
            if (Child.connected)
            {
                stack.Push(PortHelper.FindChildNode(Child));
            }
        }
        public bool TryGetOption(out OptionContainerView optionContainerView)
        {
            optionContainerView = PortHelper.FindChildNode(Child) as OptionContainerView;
            return optionContainerView != null;
        }
        public override ChildBridgeView Clone()
        {
            return new OptionBridgeView(title, Child.portColor);
        }

        public IReadOnlyList<ILayoutNode> GetLayoutChildren()
        {
            var list = new List<ILayoutNode>();
            if (TryGetOption(out var option))
                list.Add(option);
            return list;
        }
    }
}
