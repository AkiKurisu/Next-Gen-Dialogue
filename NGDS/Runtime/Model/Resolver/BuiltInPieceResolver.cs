using System.Collections;
namespace Kurisu.NGDS
{
    public class BuiltInPieceResolver : IPieceResolver
    {
        private IDialogueSystem system;
        public Piece DialoguePiece { get; private set; }
        protected ObjectContainer ObjectContainer { get; } = new();
        public void Inject(Piece piece, IDialogueSystem system)
        {
            DialoguePiece = piece;
            this.system = system;
            ObjectContainer.Register<IContent>(piece);
        }
        public IEnumerator EnterPiece()
        {
            for (int i = 0; i < DialoguePiece.Modules.Count; i++)
            {
                if (DialoguePiece.Modules[i] is IProcessable injectable)
                    yield return injectable.Process(ObjectContainer);
            }
            yield return OnPieceResolve(DialoguePiece);
        }
        protected virtual IEnumerator OnPieceResolve(Piece piece)
        {
            yield break;
        }
        public IEnumerator ExitPiece()
        {
            if (DialoguePiece.Options.Count == 0)
            {
                if (DialoguePiece.TryGetModule(out NextPieceModule module))
                {
                    system.PlayDialoguePiece(module.NextID);
                }
                else
                {
                    //Exit Dialogue
                    system.EndDialogue();
                }
            }
            else
            {
                system.CreateOption(DialoguePiece.Options);
            }
            yield break;
        }
    }
}
