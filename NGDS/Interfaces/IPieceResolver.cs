using System.Collections;
namespace Kurisu.NGDS
{
    public interface IPieceResolver
    {
        IEnumerator EnterPiece();
        void ExitPiece();
        DialoguePiece DialoguePiece { get; }
        void Inject(DialoguePiece piece, IDialogueSystem system);
    }
}
