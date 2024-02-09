using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Module: Character Module is used to identify speaker for piece and option.")]
    [AkiGroup("AIGC")]
    [ModuleOf(typeof(Piece))]
    [ModuleOf(typeof(Option))]
    public class CharacterModule : CustomModule
    {
        public CharacterModule() { }
        public CharacterModule(SharedString characterName)
        {
            this.characterName = characterName;
        }
        public CharacterModule(string characterName)
        {
            this.characterName = new(characterName);
        }
        [Tooltip("You should let AI know this content's speaker")]
        public SharedString characterName;
        protected sealed override IDialogueModule GetModule()
        {
            return new NGDS.CharacterModule(characterName.Value);
        }
    }
}
