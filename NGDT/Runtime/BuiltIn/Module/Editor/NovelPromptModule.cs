using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Editor Module : Novel Prompt Module is used to set up novel prompt.")]
    [AkiGroup("Editor/AIGC")]
    [ModuleOf(typeof(Dialogue))]
    public class NovelPromptModule : EditorModule
    {
#pragma warning disable IDE0052
        [SerializeField, Multiline, TranslateEntry]
        private SharedString pieceSystemPrompt;
        [SerializeField, Multiline, TranslateEntry]
        private SharedString optionSystemPrompt;
        [SerializeField, Multiline, TranslateEntry]
        private SharedString storySummary;
        public NovelPromptModule() { }
        public NovelPromptModule(string pieceSystemPrompt, string optionSystemPrompt, string storySummary)
        {
            this.pieceSystemPrompt = new(pieceSystemPrompt);
            this.optionSystemPrompt = new(optionSystemPrompt);
            this.storySummary = new(storySummary);
        }
#pragma warning restore IDE0052
    }
}
