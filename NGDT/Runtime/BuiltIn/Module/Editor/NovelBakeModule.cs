using System;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT
{
    [Serializable]
    [NodeInfo("Editor Module: Use Novel baker in Editor, should be added to last select node that needs to generate novel" +
    ", currently can only use ChatGPT as LLM backend.")]
    [NodeGroup("Editor/AIGC")]
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
        
        [HideInGraphEditor]
        public string lastSelection;
        
        public NovelBakeModule() { }
        
        public NovelBakeModule(int generateDepth = 3, int optionCount = 2)
        {
            this.generateDepth = generateDepth;
            this.optionCount = optionCount;
        }
#pragma warning restore IDE0052
#endif
    }
}
