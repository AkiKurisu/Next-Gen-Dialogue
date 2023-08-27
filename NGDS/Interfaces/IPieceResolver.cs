using System.Threading.Tasks;
namespace Kurisu.NGDS
{
    public interface IPieceResolver
    {
        Task OnPieceEnter();
        void OnPieceExit();
        DialoguePiece DialoguePiece { get; }
        void Inject(DialoguePiece piece, IDialogueSystem system);
    }
}
