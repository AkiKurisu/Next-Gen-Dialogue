using System;
using System.Collections.Generic;
namespace Kurisu.NGDS
{
    public interface IDialogueSystem
    {
        /// <summary>
        /// Start resolving dialogue
        /// </summary>
        /// <param name="data">Dialog data</param>
        void StartDialogue(IProvideDialogue dialogueProvider);
        void PlayDialoguePiece(string targetID);
        void CreateOption(IReadOnlyList<DialogueOption> options);
        void EndDialogue();
        event Action<IDialogueResolver> OnDialogueStart;
        event Action<IPieceResolver> OnPiecePlay;
        event Action<IOptionResolver> OnOptionCreate;
        event Action OnDialogueOver;
    }
}
