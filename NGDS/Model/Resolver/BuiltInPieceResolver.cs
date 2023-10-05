using System.Collections;
namespace Kurisu.NGDS
{
    public class BuiltInPieceResolver : IPieceResolver
    {
        private IDialogueSystem system;
        public DialoguePiece DialoguePiece { get; private set; }
        protected ObjectContainer ObjectContainer { get; } = new();
        public void Inject(DialoguePiece piece, IDialogueSystem system)
        {
            DialoguePiece = piece;
            this.system = system;
            ObjectContainer.Register<IContent>(piece);
        }
        public IEnumerator EnterPiece()
        {
            for (int i = 0; i < DialoguePiece.Modules.Count; i++)
            {
                if (DialoguePiece.Modules[i] is IInjectable injectable)
                    yield return injectable.Inject(ObjectContainer);
            }
            yield return OnPieceResolve(DialoguePiece);
        }
        protected virtual IEnumerator OnPieceResolve(DialoguePiece piece)
        {
            yield return null;
        }
        public void ExitPiece()
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
        }
    }
}
