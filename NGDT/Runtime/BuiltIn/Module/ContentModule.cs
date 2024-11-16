using Ceres;
using Ceres.Annotations;
using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [NodeInfo("Module: Content Module is used to modify dialogue content such as piece and option.")]
    [ModuleOf(typeof(Piece), true)]
    [ModuleOf(typeof(Option))]
    public class ContentModule : CustomModule, IExposedContent
    {
        public ContentModule() { }
        public ContentModule(string contentValue)
        {
            content = new Ceres.SharedString(contentValue);
        }
        [Multiline, TranslateEntry]
        public Ceres.SharedString content;
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
