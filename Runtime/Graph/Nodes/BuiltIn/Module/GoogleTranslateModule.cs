using System;
using Ceres.Annotations;
using NextGenDialogue;
namespace NextGenDialogue.Graph
{
    [Serializable]
    [CeresLabel("Google Translate")]
    [NodeInfo("Module: Google Translate Module is used to translate content.")]
    [ModuleOf(typeof(Piece))]
    [ModuleOf(typeof(Option))]
    public class GoogleTranslateModule : CustomModule
    {
        [LanguageCode]
        public string sourceLanguageCode;
        
        [LanguageCode]
        public string targetLanguageCode;
        
        protected sealed override IDialogueModule GetModule()
        {
            return new NextGenDialogue.GoogleTranslateModule(sourceLanguageCode, targetLanguageCode);
        }
    }
}
