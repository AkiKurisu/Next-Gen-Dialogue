using System;
using System.Collections;
using System.Collections.Generic;
using Chris.Gameplay;
using UnityEngine;
namespace Kurisu.NGDS
{
    public class ResolverMgr
    {
        private readonly IDialogueResolver dialogueResolver;
        
        private readonly IPieceResolver pieceResolver;
        
        private readonly IOptionResolver optionResolver;
        
        private ResolverModule resolverModule;
        
        public IDialogueResolver DialogueResolver => resolverModule.DialogueResolver ?? dialogueResolver;
        
        public IPieceResolver PieceResolver => resolverModule.PieceResolver ?? pieceResolver;
        
        public IOptionResolver OptionResolver => resolverModule.OptionResolver ?? optionResolver;
        
        public ResolverMgr()
        {
            //Collect global resolver
            dialogueResolver = ContainerSubsystem.Get().Resolve<IDialogueResolver>() ?? new DefaultDialogueResolver();
            pieceResolver = ContainerSubsystem.Get().Resolve<IPieceResolver>() ?? new DefaultPieceResolver();
            optionResolver = ContainerSubsystem.Get().Resolve<IOptionResolver>() ?? new DefaultOptionResolver();
        }
        /// <summary>
        /// Collect dialogue specific resolver
        /// </summary>
        /// <param name="dialogue"></param>
        public void Install(Dialogue dialogue)
        {
            dialogue.TryGetModule(out resolverModule);
        }
    }
    // TODO: Move to world subsystem
    /// <summary>
    /// Dialogue system implementation
    /// </summary>
    public class DialogueSystem : MonoBehaviour, IDialogueSystem
    {
        private IDialogueLookup _dialogueLookup;
        public bool IsPlaying => _dialogueLookup != null;
        public event Action<IDialogueResolver> OnDialogueStart;
        public event Action<IPieceResolver> OnPiecePlay;
        public event Action<IOptionResolver> OnOptionCreate;
        public event Action OnDialogueOver;
        
        private ResolverMgr _resolverMgr;
        public ResolverMgr ResolverMgr => _resolverMgr ??= new ResolverMgr();
        
        private Coroutine _runningCoroutine;
        
        private void Awake()
        {
            ContainerSubsystem.Get().Register<IDialogueSystem>(this);
        }
        private void OnDestroy()
        {
            ContainerSubsystem.Get()?.Unregister<IDialogueSystem>(this);
        }
        public IDialogueLookup GetCurrentLookup()
        {
            return _dialogueLookup;
        }
        public Dialogue GetCurrentDialogue()
        {
            return _dialogueLookup?.ToDialogue();
        }
        public void StartDialogue(IDialogueLookup dialogueProvider)
        {
            _dialogueLookup = dialogueProvider;
            var dialogueData = dialogueProvider.ToDialogue();
            ResolverMgr.Install(dialogueData);
            ResolverMgr.DialogueResolver.Inject(dialogueData, this);
            _runningCoroutine = StartCoroutine(DialogueEnterCoroutine());
        }
        private IEnumerator DialogueEnterCoroutine()
        {
            yield return ResolverMgr.DialogueResolver.EnterDialogue();
            OnDialogueStart?.Invoke(ResolverMgr.DialogueResolver);
            PlayDialoguePiece(_dialogueLookup.GetFirst());
        }
        private void PlayDialoguePiece(Piece piece)
        {
            ResolverMgr.PieceResolver.Inject(piece, this);
            _runningCoroutine = StartCoroutine(PieceEnterCoroutine());
        }
        private IEnumerator PieceEnterCoroutine()
        {
            yield return ResolverMgr.PieceResolver.EnterPiece();
            OnPiecePlay?.Invoke(ResolverMgr.PieceResolver);
        }
        public void PlayDialoguePiece(string targetID)
        {
            PlayDialoguePiece(_dialogueLookup.GetNext(targetID));
        }
        public void CreateOption(IReadOnlyList<Option> options)
        {
            ResolverMgr.OptionResolver.Inject(options, this);
            _runningCoroutine = StartCoroutine(OptionEnterCoroutine());
        }

        private IEnumerator OptionEnterCoroutine()
        {
            yield return ResolverMgr.OptionResolver.EnterOption();
            OnOptionCreate?.Invoke(ResolverMgr.OptionResolver);
        }

        public void EndDialogue(bool forceEnd)
        {
            if (forceEnd && _runningCoroutine != null)
            {
                StopCoroutine(_runningCoroutine);
            }
            ResolverMgr.DialogueResolver.ExitDialogue();
            OnDialogueOver?.Invoke();
            _dialogueLookup = null;
            _runningCoroutine = null;
        }
    }
}
