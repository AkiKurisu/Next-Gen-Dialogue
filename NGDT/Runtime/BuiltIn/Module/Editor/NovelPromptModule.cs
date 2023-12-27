using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Editor Module : Novel Prompt Module is used to set up novel prompt.")]
    [AkiGroup("Editor/AIGC")]
    [ModuleOf(typeof(Dialogue))]
    public class NovelPromptModule : EditorModule
    {
        [SerializeField, Multiline]
        private SharedString pieceSystemPrompt;
        [SerializeField, Multiline]
        private SharedString optionSystemPrompt;
        [SerializeField, Multiline]
        private SharedString storySummary;
    }
}
