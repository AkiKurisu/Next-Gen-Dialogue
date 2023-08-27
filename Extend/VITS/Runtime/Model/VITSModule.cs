namespace Kurisu.NGDS.VITS
{
    public readonly struct VITSModule : IDialogueModule
    {
        private readonly int characterID;
        public int CharacterID => characterID;
        public VITSModule(int characterID)
        {
            this.characterID = characterID;
        }
    }
}
