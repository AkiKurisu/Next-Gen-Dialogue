using System;
using Ceres.Annotations;
using Ceres.Graph;
using UnityEngine;
namespace Kurisu.NGDT
{
    [Serializable]
    [CeresLabel("Novel Prompt")]
    [NodeInfo("Editor Module: Novel Prompt is used to set up novel prompt.")]
    [CeresGroup("Editor/AIGC")]
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
            this.pieceSystemPrompt = new SharedString(pieceSystemPrompt);
            this.optionSystemPrompt = new SharedString(optionSystemPrompt);
            this.storySummary = new SharedString(storySummary);
        }
#pragma warning restore IDE0052
    }
}
