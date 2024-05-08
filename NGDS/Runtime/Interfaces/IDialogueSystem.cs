using System;
using System.Collections.Generic;
namespace Kurisu.NGDS
{
    public interface IDialogueSystem
    {
        /// <summary>
        /// Whether a dialogue is playing
        /// </summary>
        /// <value></value>
        bool IsPlaying { get; }
        /// <summary>
        /// Start resolving dialogue
        /// </summary>
        /// <param name="data">Dialog data</param>
        void StartDialogue(IDialogueLookup lookup);
        /// <summary>
        /// Play target dialogue piece
        /// </summary>
        /// <param name="targetID"></param>
        void PlayDialoguePiece(string targetID);
        /// <summary>
        /// Create dialogue options
        /// </summary>
        /// <param name="options"></param>
        void CreateOption(IReadOnlyList<Option> options);
        /// <summary>
        /// Complete current dialogue
        /// </summary>
        /// <param name="forceEnd">Whether should force complete if has dialogue playing</param>
        void EndDialogue(bool forceEnd = false);
        /// <summary>
        /// Get current playing dialogue
        /// </summary>
        /// <returns></returns>
        Dialogue GetCurrentDialogue();
        /// <summary>
        /// Get current using dialogue proxy
        /// </summary>
        /// <returns></returns>
        IDialogueLookup GetCurrentLookup();
        event Action<IDialogueResolver> OnDialogueStart;
        event Action<IPieceResolver> OnPiecePlay;
        event Action<IOptionResolver> OnOptionCreate;
        event Action OnDialogueOver;
    }
}
