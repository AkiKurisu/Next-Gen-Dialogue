using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Editor Module : Use Google Translate to translate all valid contents.")]
    [AkiGroup("Editor")]
    [ModuleOf(typeof(Dialogue))]
    public class EditorTranslateModule : EditorModule
    {
#if UNITY_EDITOR
        [SerializeField, LanguageCode]
        private string sourceLanguageCode;
        [SerializeField, LanguageCode]
        private string targetLanguageCode;
#endif
    }
}
