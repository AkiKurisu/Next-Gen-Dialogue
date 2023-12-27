using System.Collections;
namespace Kurisu.NGDS.AI
{
    public class AIOptionResolver : BuiltInOptionResolver
    {
        private readonly AIPromptBuilder promptBuilder;
        public AIOptionResolver(AIPromptBuilder promptBuilder)
        {
            this.promptBuilder = promptBuilder;
            ObjectContainer.Register(promptBuilder);
        }
        protected override IEnumerator OnOptionResolve(Option option)
        {
            if (option.TryGetModule(out CharacterModule characterModule))
            {
                promptBuilder.Append(characterModule.CharacterName, option.Content);
            }
            yield return null;
        }
    }
}
