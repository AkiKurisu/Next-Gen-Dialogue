using System.Collections.Generic;
using System.Threading;
using Chris.Gameplay;
using Cysharp.Threading.Tasks;
using R3;

namespace NextGenDialogue
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

            public IDialogueResolver DialogueResolver => _resolverModule?.DialogueResolver ?? _dialogueResolver;

            public IPieceResolver PieceResolver => _resolverModule?.PieceResolver ?? _pieceResolver;

            public IOptionResolver OptionResolver => _resolverModule?.OptionResolver ?? _optionResolver;

            public DialogueResolverContainer(IContainerSubsystem containerSubsystem)
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

        public Subject<IDialogueResolver> OnDialogueStart { get; } = new();

        public Subject<IPieceResolver> OnPiecePlay { get; } = new();

        public Subject<IOptionResolver> OnOptionCreate { get; } = new();

        public Subject<Unit> OnDialogueOver { get; } = new();

        private DialogueResolverContainer _resolverContainer;

        private DialogueResolverContainer ResolverContainer
        {
            get
            {
                return _resolverContainer ??= new DialogueResolverContainer(ContainerSubsystem.Get());
            }
        }

        private CancellationTokenSource _cts;

        public static DialogueSystem Get()
        {
            return GetOrCreate<DialogueSystem>();
        }

        protected override void Release()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            
            OnDialogueStart.Dispose();
            OnOptionCreate.Dispose();
            OnPiecePlay.Dispose();
            OnDialogueOver.Dispose();
        }

        /// <summary>
        /// Get current playing <see cref="IDialogueContainer"/>
        /// </summary>
        /// <returns></returns>
        public IDialogueContainer GetPlayingDialogueContainer()
        {
            return _dialogueContainer;
        }

        /// <summary>
        /// Get current playing <see cref="Dialogue"/>
        /// </summary>
        /// <returns></returns>
        public Dialogue GetPlayingDialogue()
        {
            return ResolverContainer.DialogueResolver.Dialogue;
        }

        public void StartDialogue(IDialogueContainer dialogueProvider)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            _dialogueContainer = dialogueProvider;
            var dialogueData = dialogueProvider.ToDialogue();
            ResolverContainer.Install(dialogueData);
            ResolverContainer.DialogueResolver.Inject(dialogueData, this);
            DialogueEnterAsync().AttachExternalCancellation(GetCancellationToken()).Forget();
        }

        public void PlayDialoguePiece(string targetID)
        {
            PlayDialoguePiece(_dialogueContainer.GetNext(targetID));
        }

        public void CreateOption(IReadOnlyList<Option> options)
        {
            ResolverContainer.OptionResolver.Inject(options, this);
            OptionEnterAsync().Forget();
        }

        public void EndDialogue(bool forceEnd)
        {
            if (forceEnd)
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;
            }
            ResolverContainer.DialogueResolver.ExitDialogue();
            OnDialogueOver.OnNext(Unit.Default);
            _dialogueContainer = null;
        }

        private async UniTask DialogueEnterAsync()
        {
            var token = GetCancellationToken();
            await ResolverContainer.DialogueResolver.EnterDialogue(token);
            OnDialogueStart.OnNext(ResolverContainer.DialogueResolver);
            PlayDialoguePiece(_dialogueContainer.GetFirst());
        }

        private void PlayDialoguePiece(Piece piece)
        {
            ResolverContainer.PieceResolver.Inject(piece, this);
            PieceEnterAsync().Forget();
        }

        private async UniTask PieceEnterAsync()
        {
            var token = GetCancellationToken();
            await ResolverContainer.PieceResolver.EnterPiece(token);
            OnPiecePlay.OnNext(ResolverContainer.PieceResolver);
        }

        private async UniTask OptionEnterAsync()
        {
            var token = GetCancellationToken();
            await ResolverContainer.OptionResolver.EnterOption(token);
            OnOptionCreate.OnNext(ResolverContainer.OptionResolver);
        }

        private CancellationToken GetCancellationToken()
        {
            return _cts?.Token ?? default;
        }
    }
}
