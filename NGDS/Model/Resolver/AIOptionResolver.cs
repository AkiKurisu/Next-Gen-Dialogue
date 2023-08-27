using System.Threading.Tasks;
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
        protected override Task OnOptionResolve(DialogueOption option)
        {
            if (option.TryGetModule(out CharacterModule characterModule))
            {
                promptBuilder.Append(characterModule.CharacterName, option.Content);
            }
            return Task.CompletedTask;
        }
    }
}
