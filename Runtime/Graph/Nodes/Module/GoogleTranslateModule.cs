using System;
using Ceres.Annotations;

namespace NextGenDialogue.Graph
{
    [Serializable]
    [CeresLabel("Google Translate")]
    [NodeInfo("Translate content with google translator.")]
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
