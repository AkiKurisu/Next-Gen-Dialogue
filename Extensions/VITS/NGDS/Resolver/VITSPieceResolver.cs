using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
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
        public AudioClip[] AudioClips { get; private set; }
        public void Inject(Piece piece, IDialogueSystem system)
        {
            DialoguePiece = piece;
            this.system = system;
            objectContainer.Register<IContentModule>(piece);
        }
        public IEnumerator EnterPiece()
        {
            yield return DialoguePiece.ProcessModules(objectContainer);
            AudioClips = new AudioClip[DialoguePiece.Contents.Length];
            var modules = ListPool<VITSModule>.Get();
            DialoguePiece.CollectModules(modules);
            var task = Task.WhenAll(modules.Select((x, idx) => x.RequestOrLoadAudioClipParallel(idx, vitsTurbo, DialoguePiece.Contents, AudioClips, ct.Token)));
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
            ListPool<VITSModule>.Release(modules);
            while (audioSource.isPlaying) yield return null;
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