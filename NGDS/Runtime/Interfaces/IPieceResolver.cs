using System.Collections;
namespace Kurisu.NGDS
{
    public interface IPieceResolver
    {
        IEnumerator EnterPiece();
        IEnumerator ExitPiece();
        Piece DialoguePiece { get; }
        void Inject(Piece piece, IDialogueSystem system);
    }
}
