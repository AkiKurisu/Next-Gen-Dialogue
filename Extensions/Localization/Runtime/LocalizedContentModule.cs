using System;
using Ceres.Annotations;
using Ceres.Graph;
using UnityEngine;
using UnityEngine.Localization;
namespace NextGenDialogue.Graph.Localization
{
    [Serializable]
    [CeresLabel("Localized Content")]
    [NodeInfo("Module: Localized Content is used to modify dialogue content such as piece and option using Unity.Localization.")]
    [ModuleOf(typeof(Piece))]
    [ModuleOf(typeof(Option))]
    public class LocalizedContentModule : CustomModule
    {
        [SerializeField, Setting]
        private SharedString tableEntry;
        
        [SerializeField, Setting]
        private SharedString stringEntry;

        protected sealed override IDialogueModule GetModule()
        {
            return new NextGenDialogue.ContentModule(new LocalizedString(tableEntry.Value, stringEntry.Value).GetLocalizedString());
        }
    }
}
