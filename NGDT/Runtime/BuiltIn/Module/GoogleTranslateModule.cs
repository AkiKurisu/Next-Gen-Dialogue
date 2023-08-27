using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Module : Google Translate Module is used to translate content.")]
    [ModuleOf(typeof(Piece))]
    [ModuleOf(typeof(Option))]
    public class GoogleTranslateModule : CustomModule
    {
        [SerializeField, LanguageCode]
        private string sourceLanguageCode;
        [SerializeField, LanguageCode]
        private string targetLanguageCode;
        protected sealed override IDialogueModule GetModule()
        {
            return new NGDS.GoogleTranslateModule(sourceLanguageCode, targetLanguageCode);
        }
    }
}
