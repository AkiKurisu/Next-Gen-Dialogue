using System;
using Ceres.Annotations;
using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [Serializable]
    [NodeInfo("Module: Prompt Preset Module is used to set up AI dialogue always included prompt using external text file.")]
    [CeresGroup("AIGC")]
    [ModuleOf(typeof(Dialogue))]
    public class PromptPresetModule : CustomModule
    {
        [Multiline]
        public Ceres.SharedUObject<TextAsset> prompt;
        protected sealed override IDialogueModule GetModule()
        {
            return new NGDS.SystemPromptModule(prompt.Value.text);
        }
    }
}
