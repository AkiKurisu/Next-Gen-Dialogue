using Kurisu.NGDS;
using Kurisu.NGDS.AI;
using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Module : Character Preset Module is used to set up AI prompt, used for chat dialogue.")]
    [AkiGroup("AIGC")]
    [ModuleOf(typeof(Dialogue))]
    public class CharacterPresetModule : CustomModule
    {
        [SerializeField]
        private SharedString user_Name = new("You");
        [SerializeField]
        private SharedString char_name = new("Bot");
        [SerializeField, Multiline]
        private SharedString char_persona;
        [SerializeField, Multiline]
        private SharedString world_scenario;
        public override void Awake()
        {
            InitVariable(user_Name);
            InitVariable(char_name);
            InitVariable(char_persona);
            InitVariable(world_scenario);
        }
        protected sealed override IDialogueModule GetModule()
        {
            return new NGDS.PromptModule(CharacterPresetHelper.GeneratePrompt(
                user_Name.Value,
                char_name.Value,
                char_persona.Value,
                world_scenario.Value
             ));
        }
    }
}
