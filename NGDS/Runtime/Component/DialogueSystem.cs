using System;
using System.Collections;
using System.Collections.Generic;
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
            dialogueResolver = IOCContainer.Resolve<IDialogueResolver>() ?? new BuiltInDialogueResolver();
            pieceResolver = IOCContainer.Resolve<IPieceResolver>() ?? new BuiltInPieceResolver();
            optionResolver = IOCContainer.Resolve<IOptionResolver>() ?? new BuiltInOptionResolver();
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
    /// <summary>
    /// Basic dialogue system implementation
    /// </summary>
    public class DialogueSystem : MonoBehaviour, IDialogueSystem
    {
        private IDialogueLookup dialogueLookup;
        public bool IsPlaying => dialogueLookup != null;
        public event Action<IDialogueResolver> OnDialogueStart;
        public event Action<IPieceResolver> OnPiecePlay;
        public event Action<IOptionResolver> OnOptionCreate;
        public event Action OnDialogueOver;
        private ResolverMgr resolverMgr;
        public ResolverMgr ResolverMgr => resolverMgr ??= new();
        private Coroutine runningCoroutine;
        private void Awake()
        {
            IOCContainer.Register<IDialogueSystem>(this);
        }
        private void OnDestroy()
        {
            IOCContainer.UnRegister<IDialogueSystem>(this);
        }
        public IDialogueLookup GetCurrentLookup()
        {
            return dialogueLookup;
        }
        public T GetCurrentLookup<T>() where T : IDialogueLookup
        {
            return (T)dialogueLookup;
        }
        public Dialogue GetCurrentDialogue()
        {
            return dialogueLookup?.ToDialogue();
        }
        public void StartDialogue(IDialogueLookup dialogueProvider)
        {
            dialogueLookup = dialogueProvider;
            var dialogueData = dialogueProvider.ToDialogue();
            ResolverMgr.Install(dialogueData);
            ResolverMgr.DialogueResolver.Inject(dialogueData, this);
            runningCoroutine = StartCoroutine(DialogueEnterCoroutine());
        }
        private IEnumerator DialogueEnterCoroutine()
        {
            yield return ResolverMgr.DialogueResolver.EnterDialogue();
            OnDialogueStart?.Invoke(ResolverMgr.DialogueResolver);
            PlayDialoguePiece(dialogueLookup.GetFirst());
        }
        private void PlayDialoguePiece(Piece piece)
        {
            ResolverMgr.PieceResolver.Inject(piece, this);
            runningCoroutine = StartCoroutine(PieceEnterCoroutine());
        }
        private IEnumerator PieceEnterCoroutine()
        {
            yield return ResolverMgr.PieceResolver.EnterPiece();
            OnPiecePlay?.Invoke(ResolverMgr.PieceResolver);
        }
        public void PlayDialoguePiece(string targetID)
        {
            PlayDialoguePiece(dialogueLookup.GetNext(targetID));
        }
        public void CreateOption(IReadOnlyList<Option> options)
        {
            ResolverMgr.OptionResolver.Inject(options, this);
            runningCoroutine = StartCoroutine(OptionEnterCoroutine());
        }

        private IEnumerator OptionEnterCoroutine()
        {
            yield return ResolverMgr.OptionResolver.EnterOption();
            OnOptionCreate?.Invoke(ResolverMgr.OptionResolver);
        }

        public void EndDialogue(bool forceEnd)
        {
            if (forceEnd && runningCoroutine != null)
            {
                StopCoroutine(runningCoroutine);
            }
            ResolverMgr.DialogueResolver.ExitDialogue();
            OnDialogueOver?.Invoke();
            dialogueLookup = null;
            runningCoroutine = null;
        }
    }
}
