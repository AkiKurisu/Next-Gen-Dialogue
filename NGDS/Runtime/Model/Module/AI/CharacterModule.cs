namespace Kurisu.NGDS
{
    public readonly struct CharacterModule : IDialogueModule
    {
        private readonly string characterName;
        public string CharacterName => characterName;
        public CharacterModule(string characterName)
        {
            this.characterName = characterName;
        }
    }
}
