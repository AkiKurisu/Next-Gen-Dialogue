using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Module : Prompt Module is used to set up AI dialogue always included prompt.")]
    [AkiGroup("AIGC")]
    [ModuleOf(typeof(Dialogue))]
    public class PromptModule : CustomModule
    {
        [SerializeField, Multiline, TranslateEntry]
        private SharedString prompt;
        public PromptModule() { }
        public PromptModule(SharedString prompt)
        {
            this.prompt = prompt;
        }
        public PromptModule(string prompt)
        {
            this.prompt = new(prompt);
        }
        public override void Awake()
        {
            InitVariable(prompt);
        }
        protected sealed override IDialogueModule GetModule()
        {
            return new NGDS.PromptModule(prompt.Value);
        }
    }
}
