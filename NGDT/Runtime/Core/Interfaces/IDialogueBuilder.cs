using Kurisu.NGDS;
namespace Kurisu.NGDT
{
    public interface IDialogueBuilder
    {
        /// <summary>
        /// Push new node into write buffer
        /// </summary>
        /// <param name="node"></param>
        void StartWriteNode(Node node);
        /// <summary>
        /// Get current writing node
        /// </summary>
        /// <returns></returns>
        Node GetNode();
        /// <summary>
        /// Dispose current writing node
        /// </summary>
        void DisposeWriteNode();
        /// <summary>
        /// End build dialogue nodes
        /// </summary>
        /// <param name="dialogue">Inject dialogue proxy</param>
        void EndBuildDialogue(IDialogueProxy dialogue);
        /// <summary>
        /// End writing node
        /// </summary>
        void EndWriteNode();
    }
}
