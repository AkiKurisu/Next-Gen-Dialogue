#if USE_VITS
using System.Collections;
using Kurisu.NGDS.AI;
using UnityEngine;
namespace Kurisu.NGDS.VITS
{
    public class VITSPieceResolver : IPieceResolver
    {
        public VITSPieceResolver(AIPromptBuilder promptBuilder, VITSTurbo vitsTurbo, AudioSource audioSource)
        {
            this.promptBuilder = promptBuilder;
            this.vitsTurbo = vitsTurbo;
            this.audioSource = audioSource;
            objectContainer.Register(promptBuilder);
        }
        private readonly AIPromptBuilder promptBuilder;
        private readonly VITSTurbo vitsTurbo;
        private readonly AudioSource audioSource;
        private IDialogueSystem system;
        private readonly ObjectContainer objectContainer = new();
        public DialoguePiece DialoguePiece { get; private set; }
        public void Inject(DialoguePiece piece, IDialogueSystem system)
        {
            DialoguePiece = piece;
            this.system = system;
            objectContainer.Register<IContent>(piece);
        }
        public IEnumerator EnterPiece()
        {
            for (int i = 0; i < DialoguePiece.Modules.Count; i++)
            {
                if (DialoguePiece.Modules[i] is IInjectable injectable)
                    yield return injectable.Inject(objectContainer);
            }
            if (DialoguePiece.TryGetModule(out CharacterModule characterModule))
            {
                promptBuilder.Append(characterModule.CharacterName, DialoguePiece.Content);
            }
            if (DialoguePiece.TryGetModule(out VITSAudioClipModule audioClipModule))
            {
                while (audioSource.isPlaying) yield return null;
                audioSource.clip = audioClipModule.AudioClip;
                audioSource.Play();
                yield break;
            }
            if (DialoguePiece.TryGetModule(out VITSGenerateModule vitsModule))
            {
                var task = vitsTurbo.SendVITSRequestAsync(DialoguePiece.Content, vitsModule.CharacterID);
                yield return new WaitUntil(() => task.IsCompleted);
                var response = task.Result;
                if (response.Status)
                {
                    while (audioSource.isPlaying) yield return null;
                    audioSource.clip = response.Result;
                    audioSource.Play();
                }
                else
                {
                    Debug.LogWarning("[VITS Piece Resolver] VITS Request failed !");
                }
            }
        }
        public void ExitPiece()
        {
            if (DialoguePiece.Options.Count == 0)
            {
                if (DialoguePiece.TryGetModule(out NextPieceModule module))
                {
                    system.PlayDialoguePiece(module.NextID);
                }
                else
                {
                    //Exit Dialogue
                    system.EndDialogue();
                }
            }
            else
            {
                system.CreateOption(DialoguePiece.Options);
            }
        }
    }
}
#endif