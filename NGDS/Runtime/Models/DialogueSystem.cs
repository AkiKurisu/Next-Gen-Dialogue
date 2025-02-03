using System;
using System.Collections.Generic;
using System.Threading;
using Chris.Gameplay;
using Cysharp.Threading.Tasks;
using R3;

namespace Kurisu.NGDS
{
    /// <summary>
    /// Dialogue <see cref="WorldSubsystem"/> implementation
    /// </summary>
    public class DialogueSystem : WorldSubsystem
    {
        private class DialogueResolverContainer
        {
            private readonly IDialogueResolver _dialogueResolver;
            
            private readonly IPieceResolver _pieceResolver;
            
            private readonly IOptionResolver _optionResolver;
            
            private ResolverModule _resolverModule;
            
            public IDialogueResolver DialogueResolver => _resolverModule.DialogueResolver ?? _dialogueResolver;
            
            public IPieceResolver PieceResolver => _resolverModule.PieceResolver ?? _pieceResolver;
            
            public IOptionResolver OptionResolver => _resolverModule.OptionResolver ?? _optionResolver;
            
            public DialogueResolverContainer(ContainerSubsystem containerSubsystem)
            {
                // Collect global resolver
                _dialogueResolver = containerSubsystem.Resolve<IDialogueResolver>() ?? new DefaultDialogueResolver();
                _pieceResolver = containerSubsystem.Resolve<IPieceResolver>() ?? new DefaultPieceResolver();
                _optionResolver = containerSubsystem.Resolve<IOptionResolver>() ?? new DefaultOptionResolver();
            }
            
            /// <summary>
            /// Collect dialogue specific resolver
            /// </summary>
            /// <param name="dialogue"></param>
            public void Install(Dialogue dialogue)
            {
                dialogue.TryGetModule(out _resolverModule);
            }
        }
        
        private IDialogueContainer _dialogueContainer;
        
        public bool IsPlaying => _dialogueContainer != null;
        
        public readonly Subject<IDialogueResolver> OnDialogueStart = new();
        
        public readonly Subject<IPieceResolver> OnPiecePlay = new();
        
        public readonly Subject<IOptionResolver> OnOptionCreate = new();
        
        public readonly Subject<Unit> OnDialogueOver = new();
        
        private DialogueResolverContainer _resolverContainer;
        
        private DialogueResolverContainer ResolverContainer
        {
            get
            {
                return _resolverContainer ??= new DialogueResolverContainer(ContainerSubsystem.Get());
            }
        }

        private CancellationTokenSource _cts = new();

        public static DialogueSystem Get()
        {
            return GetOrCreate<DialogueSystem>();
        }
        
        protected override void Release()
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
            OnDialogueStart.Dispose();
            OnOptionCreate.Dispose();
            OnPiecePlay.Dispose();
            OnDialogueOver.Dispose();
        }
        
        public IDialogueContainer GetCurrentContainer()
        {
            return _dialogueContainer;
        }
        
        public Dialogue GetCurrentDialogue()
        {
            return _dialogueContainer?.ToDialogue();
        }
        
        public void StartDialogue(IDialogueContainer dialogueProvider)
        {
            _dialogueContainer = dialogueProvider;
            var dialogueData = dialogueProvider.ToDialogue();
            ResolverContainer.Install(dialogueData);
            ResolverContainer.DialogueResolver.Inject(dialogueData, this);
            DialogueEnterAsync().AttachExternalCancellation(_cts.Token).Forget();
        }
        
        private async UniTask DialogueEnterAsync()
        {
            await ResolverContainer.DialogueResolver.EnterDialogue().AttachExternalCancellation(_cts.Token);
            OnDialogueStart.OnNext(ResolverContainer.DialogueResolver);
            PlayDialoguePiece(_dialogueContainer.GetFirst());
        }
        
        private void PlayDialoguePiece(Piece piece)
        {
            ResolverContainer.PieceResolver.Inject(piece, this);
            PieceEnterAsync().AttachExternalCancellation(_cts.Token).Forget();
        }
        
        private async UniTask PieceEnterAsync()
        {
            await ResolverContainer.PieceResolver.EnterPiece().AttachExternalCancellation(_cts.Token);
            OnPiecePlay.OnNext(ResolverContainer.PieceResolver);
        }
        
        public void PlayDialoguePiece(string targetID)
        {
            PlayDialoguePiece(_dialogueContainer.GetNext(targetID));
        }
        
        public void CreateOption(IReadOnlyList<Option> options)
        {
            ResolverContainer.OptionResolver.Inject(options, this);
            OptionEnterAsync().AttachExternalCancellation(_cts.Token).Forget();
        }

        private async UniTask OptionEnterAsync()
        {
            await ResolverContainer.OptionResolver.EnterOption().AttachExternalCancellation(_cts.Token);
            OnOptionCreate.OnNext(ResolverContainer.OptionResolver);
        }

        public void EndDialogue(bool forceEnd)
        {
            if (forceEnd)
            {
                _cts.Cancel();
                _cts = new CancellationTokenSource();
            }
            ResolverContainer.DialogueResolver.ExitDialogue();
            OnDialogueOver.OnNext(Unit.Default);
            _dialogueContainer = null;
        }
    }
}
