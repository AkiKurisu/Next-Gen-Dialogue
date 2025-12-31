using System.Threading;
using Cysharp.Threading.Tasks;

namespace NextGenDialogue
{
    public class DefaultPieceResolver : IPieceResolver
    {
        private DialogueSystem _system;
        
        public Piece DialoguePiece { get; private set; }
        
        protected ObjectContainer ObjectContainer { get; } = new();
        
        public void Inject(Piece piece, DialogueSystem system)
        {
            DialoguePiece = piece;
            _system = system;
            ObjectContainer.Register<IContentModule>(piece);
        }
        
        public async UniTask EnterPiece(CancellationToken cancellationToken)
        {
            await DialoguePiece.ProcessModules(ObjectContainer, cancellationToken);
            await OnPieceResolve(DialoguePiece, cancellationToken);
        }
        
        protected virtual UniTask OnPieceResolve(Piece piece, CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
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
                    //Exit Dialogue
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
