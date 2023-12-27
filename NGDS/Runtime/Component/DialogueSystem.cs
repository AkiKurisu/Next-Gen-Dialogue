using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.NGDS
{
    public class ResolverHandler
    {
        private readonly IDialogueResolver dialogueResolver;
        private readonly IPieceResolver pieceResolver;
        private readonly IOptionResolver optionResolver;
        private ResolverModule resolverModule;
        public IDialogueResolver DialogueResolver => resolverModule.DialogueResolver ?? dialogueResolver;
        public IPieceResolver PieceResolver => resolverModule.PieceResolver ?? pieceResolver;
        public IOptionResolver OptionResolver => resolverModule.OptionResolver ?? optionResolver;
        public ResolverHandler()
        {
            //Collect global resolver
            dialogueResolver = IOCContainer.Resolve<IDialogueResolver>() ?? new BuiltInDialogueResolver();
            pieceResolver = IOCContainer.Resolve<IPieceResolver>() ?? new BuiltInPieceResolver();
            optionResolver = IOCContainer.Resolve<IOptionResolver>() ?? new BuiltInOptionResolver();
        }
        public void Handle(Dialogue dialogue)
        {
            //Collect dialogue specific resolver
            dialogue.TryGetModule(out resolverModule);
        }
    }
    public class DialogueSystem : MonoBehaviour, IDialogueSystem
    {

        private IDialogueProxy dialogue;
        private void Awake()
        {
            IOCContainer.Register<IDialogueSystem>(this);
        }
        private void OnDestroy()
        {
            IOCContainer.UnRegister<IDialogueSystem>(this);
        }
        public event Action<IDialogueResolver> OnDialogueStart;
        public event Action<IPieceResolver> OnPiecePlay;
        public event Action<IOptionResolver> OnOptionCreate;
        public event Action OnDialogueOver;
        private ResolverHandler resolverHandler;
        public ResolverHandler ResolverHandler
        {
            get
            {
                //Lazy initialization
                resolverHandler ??= new();
                return resolverHandler;
            }
        }
        public void StartDialogue(IDialogueProxy dialogueProvider)
        {
            dialogue = dialogueProvider;
            var dialogueData = dialogueProvider.CastDialogue();
            ResolverHandler.Handle(dialogueData);
            ResolverHandler.DialogueResolver.Inject(dialogueData, this);
            StartCoroutine(DialogueEnterCoroutine());
        }
        private IEnumerator DialogueEnterCoroutine()
        {
            yield return ResolverHandler.DialogueResolver.EnterDialogue();
            OnDialogueStart?.Invoke(ResolverHandler.DialogueResolver);
            PlayDialoguePiece(dialogue.GetFirst());
        }
        private void PlayDialoguePiece(Piece piece)
        {
            ResolverHandler.PieceResolver.Inject(piece, this);
            StartCoroutine(PieceEnterCoroutine());
        }
        private IEnumerator PieceEnterCoroutine()
        {
            yield return ResolverHandler.PieceResolver.EnterPiece();
            OnPiecePlay?.Invoke(ResolverHandler.PieceResolver);
        }
        public void PlayDialoguePiece(string targetID)
        {
            PlayDialoguePiece(dialogue.GetNext(targetID));
        }
        public void CreateOption(IReadOnlyList<Option> options)
        {
            ResolverHandler.OptionResolver.Inject(options, this);
            StartCoroutine(OptionEnterCoroutine());
        }

        private IEnumerator OptionEnterCoroutine()
        {
            yield return ResolverHandler.OptionResolver.EnterOption();
            OnOptionCreate?.Invoke(ResolverHandler.OptionResolver);
        }

        public void EndDialogue()
        {
            ResolverHandler.DialogueResolver.ExitDialogue();
            OnDialogueOver?.Invoke();
        }
    }
}
