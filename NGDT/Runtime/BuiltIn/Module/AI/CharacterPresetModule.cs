using Kurisu.NGDS;
using Kurisu.NGDS.AI;
using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Module: Character Preset Module is used to set up AI prompt, used for chat dialogue.")]
    [AkiGroup("AIGC")]
    [ModuleOf(typeof(Dialogue))]
    public class CharacterPresetModule : CustomModule
    {
        [TranslateEntry]
        public SharedString user_Name = new("You");
        [TranslateEntry]
        public SharedString char_name = new("Bot");
        [Multiline, TranslateEntry]
        public SharedString char_persona;
        [Multiline, TranslateEntry]
        public SharedString world_scenario;
        protected sealed override IDialogueModule GetModule()
        {
            return new NGDS.PromptModule(CharacterPresetHelper.GeneratePrompt(
                user_Name.Value,
                char_name.Value,
                char_persona.Value,
                world_scenario.Value
             ));
        }
        public CharacterPresetModule() { }
        public CharacterPresetModule(string user_Name, string char_name, string char_persona, string world_scenario)
        {
            this.user_Name = new(user_Name);
            this.char_name = new(char_name);
            this.char_persona = new(char_persona);
            this.world_scenario = new(world_scenario);
        }
    }
}
