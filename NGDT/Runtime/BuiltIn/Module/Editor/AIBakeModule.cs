using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Editor Module: Use AI dialogue baker in Editor, should be added to last select node that needs to generate dialogue.")]
    [AkiGroup("Editor/AIGC")]
    [ModuleOf(typeof(Piece))]
    [ModuleOf(typeof(Option))]
    public class AIBakeModule : EditorModule
    {
#if UNITY_EDITOR
#pragma warning disable IDE0052
        [SerializeField, Tooltip("Auto generate depth"), Setting]
        private int generateDepth = 3;
        [SerializeField, Tooltip("Option generate count"), Setting]
        private int optionCount = 2;
        [HideInEditorWindow]
        public string lastSelection;
        public AIBakeModule() { }
        public AIBakeModule(int generateDepth = 3, int optionCount = 2)
        {
            this.generateDepth = generateDepth;
            this.optionCount = optionCount;
        }
#pragma warning restore IDE0052
#endif
    }
}
