using System.Threading;
using Cysharp.Threading.Tasks;

namespace NextGenDialogue
{
    public interface IPieceResolver
    {
        UniTask EnterPiece(CancellationToken cancellationToken);
        
        UniTask ExitPiece();
        
        Piece DialoguePiece { get; }
        
        void Inject(Piece piece, DialogueSystem system);
    }
}
