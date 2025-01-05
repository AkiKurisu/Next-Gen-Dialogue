using System;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT
{
    [Serializable]
    [NodeInfo("Editor Module: Use Google Translate to translate all valid contents.")]
    [NodeGroup("Editor")]
    [ModuleOf(typeof(Dialogue))]
    public class EditorTranslateModule : EditorModule
    {
#if UNITY_EDITOR
#pragma warning disable IDE0052
        [SerializeField, LanguageCode]
        private string sourceLanguageCode = "en";
        
        [SerializeField, LanguageCode]
        private string targetLanguageCode = "zh";
        
        public EditorTranslateModule() { }
        
        public EditorTranslateModule(string sourceLanguageCode, string targetLanguageCode)
        {
            this.sourceLanguageCode = sourceLanguageCode;
            this.targetLanguageCode = targetLanguageCode;
        }
#pragma warning restore IDE0052
#endif
    }
}
