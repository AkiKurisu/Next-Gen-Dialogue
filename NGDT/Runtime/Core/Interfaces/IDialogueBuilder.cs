using Kurisu.NGDS;
namespace Kurisu.NGDT
{
    public interface IDialogueBuilder
    {
        /// <summary>
        /// Push new node into write buffer
        /// </summary>
        /// <param name="node"></param>
        void StartWriteNode(DialogueNode node);
        DialogueNode GetNode();
        /// <summary>
        /// Dispose current writing node
        /// </summary>
        void DisposeWriteNode();

        void ProvideDialogue(IProvideDialogue dialogue);
        void EndWriteNode();
    }
}
