using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Module : Prompt Preset Module is used to set up AI dialogue always included prompt using external text file.")]
    [AkiGroup("AIGC")]
    [ModuleOf(typeof(Dialogue))]
    public class PromptPresetModule : CustomModule
    {
        [SerializeField, Multiline]
        private SharedTObject<TextAsset> prompt;
        public override void Awake()
        {
            InitVariable(prompt);
        }
        protected sealed override IDialogueModule GetModule()
        {
            return new NGDS.PromptModule(prompt.Value.text);
        }
    }
}
