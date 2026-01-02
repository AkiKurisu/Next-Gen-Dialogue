using System;
using Ceres.Annotations;
using Ceres.Graph;
using UnityEngine;

namespace NextGenDialogue.Graph
{
    [Serializable]
    [CeresLabel("Prompt Preset")]
    [NodeInfo("Setup ai chat system prompt by text file.")]
    [CeresGroup("AIGC")]
    [ModuleOf(typeof(Dialogue))]
    public class PromptPresetModule : CustomModule
    {
        [Multiline]
        public SharedUObject<TextAsset> prompt;
        
        protected sealed override IDialogueModule GetModule()
        {
            return new NextGenDialogue.SystemPromptModule(prompt.Value.text);
        }
    }
}
