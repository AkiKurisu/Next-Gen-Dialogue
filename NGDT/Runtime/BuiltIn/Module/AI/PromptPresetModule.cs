using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [NodeInfo("Module: Prompt Preset Module is used to set up AI dialogue always included prompt using external text file.")]
    [NodeGroup("AIGC")]
    [ModuleOf(typeof(Dialogue))]
    public class PromptPresetModule : CustomModule
    {
        [Multiline]
        public SharedTObject<TextAsset> prompt;
        protected sealed override IDialogueModule GetModule()
        {
            return new NGDS.SystemPromptModule(prompt.Value.text);
        }
    }
}
