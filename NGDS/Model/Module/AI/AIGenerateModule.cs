using System.Threading.Tasks;
using Kurisu.NGDS.AI;
namespace Kurisu.NGDS
{
    public readonly struct AIGenerateModule : IDialogueModule, IInjectable
    {
        private readonly string characterName;
        public string CharacterName => characterName;
        public AIGenerateModule(string characterName)
        {
            this.characterName = characterName;
        }

        public async Task Inject(IObjectResolver resolver)
        {
            var promptBuilder = resolver.Resolve<AIPromptBuilder>();
            var response = await promptBuilder.Generate(CharacterName);
            if (response.Status) resolver.Resolve<IContent>().Content = response.Response;
            promptBuilder.Append(CharacterName, response.Response);
        }
    }
}
