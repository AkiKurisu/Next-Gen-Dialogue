using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Kurisu.NGDS.AI;
using UnityEngine;
namespace Kurisu.NGDS.VITS
{
    public class VITSPieceResolver : IPieceResolver
    {
        public VITSPieceResolver(VITSTurbo vitsTurbo, AudioSource audioSource)
        {
            this.vitsTurbo = vitsTurbo;
            this.audioSource = audioSource;
        }
        private readonly VITSTurbo vitsTurbo;
        private readonly AudioSource audioSource;
        private IDialogueSystem system;
        private readonly CancellationTokenSource ct = new();
        private readonly ObjectContainer objectContainer = new();
        public Piece DialoguePiece { get; private set; }
        public float MaxWaitTime { get; set; } = 30f;
        public void Inject(Piece piece, IDialogueSystem system)
        {
            DialoguePiece = piece;
            this.system = system;
            objectContainer.Register<IContent>(piece);
        }
        public IEnumerator EnterPiece()
        {
            for (int i = 0; i < DialoguePiece.Modules.Count; i++)
            {
                if (DialoguePiece.Modules[i] is IProcessable injectable)
                    yield return injectable.Process(objectContainer);
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
                Task<VITSResponse> task;
                if (vitsModule.NoTranslation)
                {
                    task = vitsTurbo.SendVITSRequestAsync(DialoguePiece.Content, vitsModule.CharacterID, ct.Token);
                }
                else
                {
                    task = vitsTurbo.SendVITSRequestAsyncWithTranslation(DialoguePiece.Content, vitsModule.CharacterID, ct.Token);
                }
                float waitTime = 0;
                while (!task.IsCompleted)
                {
                    yield return null;
                    waitTime += Time.deltaTime;
                    if (waitTime >= MaxWaitTime)
                    {
                        ct.Cancel();
                        break;
                    }
                }
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
        public IEnumerator ExitPiece()
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
            yield break;
        }
    }
}