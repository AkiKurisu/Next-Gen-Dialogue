#if USE_VITS
using System.Threading.Tasks;
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
        public async Task OnPieceEnter()
        {
            for (int i = 0; i < DialoguePiece.Modules.Count; i++)
            {
                if (DialoguePiece.Modules[i] is IInjectable injectable)
                    await injectable.Inject(objectContainer);
            }
            if (DialoguePiece.TryGetModule(out CharacterModule characterModule))
            {
                promptBuilder.Append(characterModule.CharacterName, DialoguePiece.Content);
            }
            if (DialoguePiece.TryGetModule(out VITSAudioClipModule audioClipModule))
            {
                while (audioSource.isPlaying) await Task.Yield();
                audioSource.clip = audioClipModule.AudioClip;
                audioSource.Play();
                return;
            }
            if (DialoguePiece.TryGetModule(out VITSGenerateModule vitsModule))
            {
                var response = await vitsTurbo.SendVITSRequestAsync(DialoguePiece.Content, vitsModule.CharacterID);
                if (response.Status)
                {
                    while (audioSource.isPlaying) await Task.Yield();
                    audioSource.clip = response.Result;
                    audioSource.Play();
                }
                else
                {
                    Debug.LogWarning("[VITS Piece Resolver] VITS Request failed !");
                }
            }
        }
        public void OnPieceExit()
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