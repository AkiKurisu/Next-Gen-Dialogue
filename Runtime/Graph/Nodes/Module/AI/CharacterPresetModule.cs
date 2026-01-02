using System;
using Ceres.Annotations;
using Ceres.Graph;
using NextGenDialogue.AI;
using UnityEngine;

namespace NextGenDialogue.Graph
{
    [Serializable]
    [CeresLabel("Character Preset")]
    [NodeInfo("Setup ai chat character prompt.")]
    [CeresGroup("AIGC")]
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
            return new NextGenDialogue.SystemPromptModule(ChatPromptHelper.ConstructPrompt(
                user_Name.Value,
                char_name.Value,
                char_persona.Value,
                world_scenario.Value
             ));
        }
        
        public CharacterPresetModule() { }
        
        public CharacterPresetModule(string userName, string charName, string charPersona, string worldScenario)
        {
            user_Name = new SharedString(userName);
            char_name = new SharedString(charName);
            char_persona = new SharedString(charPersona);
            world_scenario = new SharedString(worldScenario);
        }
    }
}
