using Cysharp.Threading.Tasks;
namespace Kurisu.NGDS
{
    public interface IPieceResolver
    {
        UniTask EnterPiece();
        
        UniTask ExitPiece();
        
        Piece DialoguePiece { get; }
        
        void Inject(Piece piece, DialogueSystem system);
    }
}
