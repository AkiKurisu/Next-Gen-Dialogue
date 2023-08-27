using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    internal class ParentBridge : Node
    {
        public Port Parent { get; }
        public ParentBridge(Type portType) : base()
        {
            AddToClassList("BridgeNode");
            capabilities &= ~Capabilities.Copiable;
            capabilities &= ~Capabilities.Deletable;
            capabilities &= ~Capabilities.Movable;
            title = "Parent";
            Parent = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, portType);
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
    internal abstract class ChildBridge : Node
    {
        public Port Child { get; private set; }
        private readonly Color portColor;
        private readonly Type portType;
        public ChildBridge(string title, Type portType, Color portColor) : base()
        {
            AddToClassList("BridgeNode");
            capabilities &= ~Capabilities.Copiable;
            this.portType = portType;
            this.portColor = portColor;
            this.title = title;
            AddChild();
        }
        protected void AddChild()
        {
            Child = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, portType);
            Child.portName = string.Empty;
            Child.portColor = portColor;
            outputContainer.Add(Child);
        }

        internal void Validate(Stack<IDialogueNode> stack)
        {
            OnValidate(stack);
        }

        internal void Commit(Container container, Stack<IDialogueNode> stack)
        {
            OnCommit(container, stack);
        }
        protected virtual void OnValidate(Stack<IDialogueNode> stack) { }

        protected virtual void OnCommit(Container container, Stack<IDialogueNode> stack)
        {
            if (Child.connected)
            {
                var node = PortHelper.FindChildNode(Child);
                stack.Push(node);
                container.AddChild(node.ReplaceBehavior());
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
    }
    internal class PieceBridge : ChildBridge
    {
        private readonly PieceIDField pieceIDField;
        private bool useReference;
        private readonly IDialogueTreeView treeView;
        public PieceBridge(IDialogueTreeView treeView, Type portType, Color portColor, string pieceIDName)
        : base("Piece", portType, portColor)
        {
            this.treeView = treeView;
            var toggle = new Toggle("Use Reference");
            toggle.RegisterValueChangedCallback(evt => OnToggle(evt.newValue));
            mainContainer.Add(toggle);
            pieceIDField = new PieceIDField("Reference", treeView, true)
            {
                value = new PieceID()
                {
                    Name = pieceIDName
                }
            };
            pieceIDField.InitField(treeView);
            mainContainer.Add(pieceIDField);
            toggle.value = !string.IsNullOrEmpty(pieceIDName);
            OnToggle(toggle.value);
        }
        private void OnToggle(bool useReference)
        {
            this.useReference = useReference;
            if (useReference && Child.connected)
            {
                var edge = Child.connections.First();
                edge.output.Disconnect(edge);
                edge.input.Disconnect(edge);
                edge.RemoveFromHierarchy();
            }
            Child.SetEnabled(!useReference);
            pieceIDField.SetEnabled(useReference);
        }
        protected sealed override void OnCommit(Container container, Stack<IDialogueNode> stack)
        {
            if (useReference)
            {
                var node = treeView.FindPiece(pieceIDField.value.Name);
                if (node == null) return;
                (container as Dialogue).AddPiece(node.GetPiece(), pieceIDField.value.Name);
            }
            else if (Child.connected)
            {
                var node = PortHelper.FindChildNode(Child) as PieceContainer;
                (container as Dialogue).AddPiece(node.GetPiece(), string.Empty);
            }
        }
        public string PieceID
        {
            get
            {
                if (useReference)
                {
                    return pieceIDField.value.Name;
                }
                else if (Child.connected)
                {
                    var node = PortHelper.FindChildNode(Child) as PieceContainer;
                    return node.GetPieceID();
                }
                return string.Empty;
            }
        }
    }
    internal class OptionBridge : ChildBridge
    {
        public OptionBridge(string title, Type portType, Color portColor) : base(title, portType, portColor)
        {
        }
        protected override void OnValidate(Stack<IDialogueNode> stack)
        {
            if (Child.connected)
            {
                stack.Push(PortHelper.FindChildNode(Child));
            }
        }
    }
}
