namespace NextGenDialogue
{
    public readonly struct ResolverModule : IDialogueModule
    {
        public IDialogueResolver DialogueResolver { get; }
        public IPieceResolver PieceResolver { get; }
        public IOptionResolver OptionResolver { get; }
        public ResolverModule(IDialogueResolver dialogueResolver, IPieceResolver pieceResolver, IOptionResolver optionResolver)
        {
            DialogueResolver = dialogueResolver;
            PieceResolver = pieceResolver;
            OptionResolver = optionResolver;
        }
    }
}
