using System;
using Ceres.Annotations;
using Ceres.Graph;
using UnityEngine;

namespace NextGenDialogue.Graph
{
    [Serializable]
    [CeresLabel("Content")]
    [NodeInfo("Provide dialogue content for parent container.")]
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
            return new NextGenDialogue.ContentModule(content.Value);
        }
    }
}
