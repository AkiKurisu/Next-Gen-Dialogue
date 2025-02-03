using System;
using Ceres.Annotations;
using Ceres.Graph;
using NextGenDialogue;
using UnityEngine;
namespace NextGenDialogue.Graph
{
    [Serializable]
    [CeresLabel("System Prompt")]
    [NodeInfo("Module: System Prompt is used to set up AI dialogue system prompt.")]
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
            return new NextGenDialogue.SystemPromptModule(prompt.Value);
        }
    }
}
