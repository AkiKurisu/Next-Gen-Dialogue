using System.Collections.Generic;
using Kurisu.NGDS;
namespace Kurisu.NGDT
{
    internal class DialogueBuilder : IDialogueBuilder
    {
        internal DialogueBuilder(ResolveDialogueDelegate resolveDialogueCallBack)
        {
            this.resolveDialogueCallBack = resolveDialogueCallBack;
        }
        private readonly ResolveDialogueDelegate resolveDialogueCallBack;
        private readonly Stack<Node> nodesBuffer = new();
        public void StartWriteNode(Node node)
        {
            nodesBuffer.Push(node);
        }
        public void DisposeWriteNode()
        {
            nodesBuffer.Pop().NodePushPool();
        }
        public Node GetNode()
        {
            return nodesBuffer.Peek();
        }
        public void EndWriteNode()
        {
            var node = nodesBuffer.Pop();
            if (nodesBuffer.TryPeek(out Node parentNode) && node is IDialogueModule module)
                parentNode.AddModule(module);
        }
        internal void ClearBuffer()
        {
            nodesBuffer.Clear();
        }

        public void EndBuildDialogue(IDialogueProxy dialogue)
        {
            resolveDialogueCallBack(dialogue);
        }
    }
}
