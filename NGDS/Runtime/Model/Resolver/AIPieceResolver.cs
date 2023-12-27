using System.Collections;
namespace Kurisu.NGDS.AI
{
    public class AIPieceResolver : BuiltInPieceResolver
    {
        private readonly AIPromptBuilder promptBuilder;
        public AIPieceResolver(AIPromptBuilder promptBuilder)
        {
            this.promptBuilder = promptBuilder;
            ObjectContainer.Register(promptBuilder);
        }
        protected override IEnumerator OnPieceResolve(Piece piece)
        {
            if (DialoguePiece.TryGetModule(out CharacterModule characterModule))
            {
                promptBuilder.Append(characterModule.CharacterName, DialoguePiece.Content);
            }
            yield return null;
        }
    }
}
