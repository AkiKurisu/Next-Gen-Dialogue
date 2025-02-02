using System;
using Ceres.Annotations;
using Ceres.Graph;
using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [Serializable]
    [NodeInfo("Module: Content Module is used to modify dialogue content such as piece and option.")]
    [ModuleOf(typeof(Piece), true)]
    [ModuleOf(typeof(Option))]
    public class ContentModule : CustomModule
    {
        public ContentModule() { }
        
        public ContentModule(string contentValue)
        {
            content = new SharedString(contentValue);
        }
        
        [Multiline, TranslateEntry]
        public SharedString content;
        
        protected sealed override IDialogueModule GetModule()
        {
            return new NGDS.ContentModule(content.Value);
        }
    }
}
