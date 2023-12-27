using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Module : Content Module is used to modify dialogue content such as piece and option.")]
    [ModuleOf(typeof(Piece))]
    [ModuleOf(typeof(Option))]
    public class ContentModule : CustomModule, IExposedContent
    {
        public ContentModule() { }
        public ContentModule(string contentValue)
        {
            content = new SharedString(contentValue);
        }
        [SerializeField, Multiline, TranslateEntry]
        private SharedString content;
        public override void Awake()
        {
            InitVariable(content);
        }
        protected sealed override IDialogueModule GetModule()
        {
            return new NGDS.ContentModule(content.Value);
        }
        public string GetContent()
        {
            return content.Value;
        }
    }
}
