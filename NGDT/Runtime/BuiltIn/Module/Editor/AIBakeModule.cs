using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Editor Module : Use AI dialogue baker in Editor, should be added to last select node that needs to generate dialogue.")]
    [AkiGroup("AIGC/Editor")]
    [ModuleOf(typeof(Piece))]
    [ModuleOf(typeof(Option))]
    public class AIBakeModule : EditorModule
    {
#if UNITY_EDITOR
        [SerializeField, Tooltip("You should tell AI who will speech in this dialogue piece")]
        private SharedString characterName;
        [SerializeField, Tooltip("The type of LLM model used to bake dialogue")]
        private LLMType llmType;
#endif
    }
}
