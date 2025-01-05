using System;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT
{
    [Serializable]
    [NodeInfo("Editor Module: Novel Prompt Module is used to set up novel prompt.")]
    [NodeGroup("Editor/AIGC")]
    [ModuleOf(typeof(Dialogue))]
    public class NovelPromptModule : EditorModule
    {
#pragma warning disable IDE0052
        [SerializeField, Multiline, TranslateEntry]
        private Ceres.SharedString pieceSystemPrompt;
        
        [SerializeField, Multiline, TranslateEntry]
        private Ceres.SharedString optionSystemPrompt;
        
        [SerializeField, Multiline, TranslateEntry]
        private Ceres.SharedString storySummary;
        
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
