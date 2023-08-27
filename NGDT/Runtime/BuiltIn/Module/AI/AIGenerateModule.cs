using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Module : AI Generate Module is used to generate dialogue piece content.")]
    [AkiGroup("AIGC")]
    [ModuleOf(typeof(Piece))]
    public class AIGenerateModule : CustomModule
    {
        [SerializeField, Tooltip("You should tell AI who will speech in this dialogue piece")]
        private SharedString characterName;
        public override void Awake()
        {
            InitVariable(characterName);
        }
        protected sealed override IDialogueModule GetModule()
        {
            return new NGDS.AIGenerateModule(characterName.Value);
        }
    }
}
