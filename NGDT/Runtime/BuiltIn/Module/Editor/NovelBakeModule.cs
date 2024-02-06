using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Editor Module : Use Novel baker in Editor, should be added to last select node that needs to generate novel" +
    ", currently can only use ChatGPT as LLM backend.")]
    [AkiGroup("Editor/AIGC")]
    [ModuleOf(typeof(Piece))]
    [ModuleOf(typeof(Option))]
    public class NovelBakeModule : EditorModule
    {
#if UNITY_EDITOR
#pragma warning disable IDE0052
        [SerializeField, Tooltip("Auto generate depth"), Setting]
        private int generateDepth = 3;
        [SerializeField, Tooltip("Option generate count"), Setting]
        private int optionCount = 2;
        [SerializeField, Tooltip("Override piece prompt")]
        private TextAsset overridePiecePrompt;
        [SerializeField, Tooltip("Override option prompt")]
        private TextAsset overrideOptionPrompt;
        [SerializeField, Tooltip("The type of LLM model used to bake dialogue")]
        private LLMType llmType;
        [HideInEditorWindow]
        public string lastSelection;
        public NovelBakeModule() { }
        public NovelBakeModule(int generateDepth = 3, int optionCount = 2, LLMType llmType = LLMType.ChatGPT)
        {
            this.generateDepth = generateDepth;
            this.optionCount = optionCount;
            this.llmType = llmType;
        }
#pragma warning restore IDE0052
#endif
    }
}
