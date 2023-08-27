namespace Kurisu.NGDS.AI
{
    public class AIDialogueResolver : BuiltInDialogueResolver
    {
        public AIDialogueResolver(AIPromptBuilder promptBuilder)
        {
            ObjectContainer.Register(promptBuilder);
        }
    }
}
