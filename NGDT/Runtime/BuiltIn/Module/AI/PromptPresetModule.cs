using System;
using Ceres.Annotations;
using Ceres.Graph;
using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [Serializable]
    [CeresLabel("Prompt Preset")]
    [NodeInfo("Module: Prompt Preset is used to set up AI dialogue always included prompt using external text file.")]
    [CeresGroup("AIGC")]
    [ModuleOf(typeof(Dialogue))]
    public class PromptPresetModule : CustomModule
    {
        [Multiline]
        public SharedUObject<TextAsset> prompt;
        protected sealed override IDialogueModule GetModule()
        {
            return new NGDS.SystemPromptModule(prompt.Value.text);
        }
    }
}
