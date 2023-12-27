using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [Ordered]
    public class NextPieceResolver : INodeResolver
    {
        public IDialogueNode CreateNodeInstance(Type type)
        {
            return new NextPieceNode();
        }
        public static bool IsAcceptable(Type behaviorType) => behaviorType == typeof(NextPieceModule);
        private class NextPieceNode : ModuleNode
        {
            private readonly Port childPort;
            private static Color PortColor = new(97 / 255f, 95 / 255f, 212 / 255f, 0.91f);
            private PieceIDField nextIDField;
            private Toggle useReferenceField;
            public NextPieceNode() : base()
            {
                AddToClassList(nameof(ModuleNode));
                childPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(PiecePort));
                childPort.portName = string.Empty;
                childPort.portColor = PortColor;
                outputContainer.Add(childPort);
            }
            protected sealed override void OnBehaviorSet()
            {
                useReferenceField = (GetFieldResolver("useReference") as BoolResolver).EditorField;
                nextIDField = (GetFieldResolver("nextID") as PieceIDResolver).EditorField;
                useReferenceField.RegisterValueChangedCallback(x => OnToggle(x.newValue));
                OnToggle(useReferenceField.value);
            }
            protected sealed override async void OnRestore()
            {
                //Connect after loaded
                await Task.Delay(1);
                var node = MapTreeView.FindPiece(nextIDField.value.Name);
                if (node != null)
                {
                    var edge = PortHelper.ConnectPorts(childPort, node.Parent);
                    MapTreeView.View.Add(edge);
                }
                OnToggle(useReferenceField.value);
            }
            private void OnToggle(bool useReference)
            {
                if (useReference && childPort.connected)
                {
                    var edge = childPort.connections.First();
                    edge.output.Disconnect(edge);
                    edge.input.Disconnect(edge);
                    edge.RemoveFromHierarchy();
                }
                childPort.SetEnabled(!useReference);
                nextIDField.SetEnabled(useReference);
            }
            protected sealed override bool OnValidate(Stack<IDialogueNode> stack)
            {
                //Prevent circle validation
                return true;
            }

            protected sealed override void OnCommit(Stack<IDialogueNode> stack)
            {
                if (useReferenceField.value) return;
                if (childPort.connected)
                {
                    //Use weak reference instead of serialize reference
                    var node = PortHelper.FindChildNode(childPort) as PieceContainer;
                    nextIDField.value.Name = node.GetPieceID();
                }
            }
        }
    }


}
