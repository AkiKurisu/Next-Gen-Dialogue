using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Module: AI Generate Module is used to generate dialogue piece content.")]
    [AkiGroup("AIGC")]
    [ModuleOf(typeof(Piece))]
    public class AIGenerateModule : CustomModule
    {
        [Tooltip("You should tell AI who will speech in this dialogue piece")]
        public SharedString characterName;
        protected sealed override IDialogueModule GetModule()
        {
            return new NGDS.AIGenerateModule(characterName.Value);
        }
    }
}
