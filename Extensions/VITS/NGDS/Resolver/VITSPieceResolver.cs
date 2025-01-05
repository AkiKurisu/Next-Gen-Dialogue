using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
namespace Kurisu.NGDS.VITS
{
    public class VITSPieceResolver : IPieceResolver
    {
        public VITSPieceResolver(VITSTurbo vitsTurbo, AudioSource audioSource)
        {
            _vitsTurbo = vitsTurbo;
            _audioSource = audioSource;
        }
        private readonly VITSTurbo _vitsTurbo;
        
        private readonly AudioSource _audioSource;
        
        private DialogueSystem _system;
        
        private readonly CancellationTokenSource _ct = new();
        
        private readonly ObjectContainer _objectContainer = new();
        
        public Piece DialoguePiece { get; private set; }
        
        public float MaxWaitTime { get; set; } = 30f;
        
        public AudioClip[] AudioClips { get; private set; }
        
        public void Inject(Piece piece, DialogueSystem system)
        {
            DialoguePiece = piece;
            _system = system;
            _objectContainer.Register<IContentModule>(piece);
        }
        
        public async UniTask EnterPiece()
        {
            await DialoguePiece.ProcessModules(_objectContainer);
            AudioClips = new AudioClip[DialoguePiece.Contents.Length];
            var modules = ListPool<VITSModule>.Get();
            DialoguePiece.CollectModules(modules);
            await UniTask.WhenAll(modules.Select((x, idx) => x.RequestOrLoadAudioClipParallel(idx, _vitsTurbo, DialoguePiece.Contents, AudioClips, _ct.Token)))
                        .Timeout(TimeSpan.FromSeconds(MaxWaitTime));
            ListPool<VITSModule>.Release(modules);
            await UniTask.WaitUntil(() => !_audioSource.isPlaying);
        }
        
        public UniTask ExitPiece()
        {
            if (DialoguePiece.Options.Count == 0)
            {
                if (DialoguePiece.TryGetModule(out NextPieceModule module))
                {
                    _system.PlayDialoguePiece(module.NextID);
                }
                else
                {
                    // Exit Dialogue
                    _system.EndDialogue(false);
                }
            }
            else
            {
                _system.CreateOption(DialoguePiece.Options);
            }

            return UniTask.CompletedTask;
        }
    }
}