using Ceres;
using Ceres.Annotations;
using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [NodeInfo("Module: Prompt Module is used to set up AI dialogue system prompt.")]
    [NodeGroup("AIGC")]
    [ModuleOf(typeof(Dialogue))]
    public class SystemPromptModule : CustomModule
    {
        [Multiline, TranslateEntry]
        public Ceres.SharedString prompt;
        public SystemPromptModule() { }
        public SystemPromptModule(Ceres.SharedString prompt)
        {
            this.prompt = prompt;
        }
        public SystemPromptModule(string prompt)
        {
            this.prompt = new(prompt);
        }
        protected sealed override IDialogueModule GetModule()
        {
            return new NGDS.SystemPromptModule(prompt.Value);
        }
    }
}
