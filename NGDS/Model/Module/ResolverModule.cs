namespace Kurisu.NGDS
{
    public readonly struct ResolverModule : IDialogueModule
    {
        public IDialogueResolver DialogueResolver { get; }
        public IPieceResolver PieceResolver { get; }
        public IOptionResolver OptionResolver { get; }
        public ResolverModule(IDialogueResolver dialogueResolver, IPieceResolver peceResolver, IOptionResolver optionResolver)
        {
            DialogueResolver = dialogueResolver;
            PieceResolver = peceResolver;
            OptionResolver = optionResolver;
        }
    }
}
