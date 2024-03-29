using Kurisu.NGDS;
namespace Kurisu.NGDT
{
    [AkiInfo("Module: Google Translate Module is used to translate content.")]
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
            return new NGDS.GoogleTranslateModule(sourceLanguageCode, targetLanguageCode);
        }
    }
}
