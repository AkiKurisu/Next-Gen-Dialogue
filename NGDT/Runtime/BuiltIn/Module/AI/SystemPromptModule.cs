using System;
using Ceres;
using Ceres.Annotations;
using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [Serializable]
    [NodeInfo("Module: Prompt Module is used to set up AI dialogue system prompt.")]
    [CeresGroup("AIGC")]
    [ModuleOf(typeof(Dialogue))]
    public class SystemPromptModule : CustomModule
    {
        [Multiline, TranslateEntry]
        public SharedString prompt;
        
        public SystemPromptModule() { }
        
        public SystemPromptModule(SharedString prompt)
        {
            this.prompt = prompt;
        }
        
        public SystemPromptModule(string prompt)
        {
            this.prompt = new SharedString(prompt);
        }
        
        protected sealed override IDialogueModule GetModule()
        {
            return new NGDS.SystemPromptModule(prompt.Value);
        }
    }
}
