using System;
using Ceres.Annotations;
using UnityEngine;
namespace NextGenDialogue.Graph.VITS
{
    [Serializable]
    [CeresLabel("VITS Baker")]
    [NodeInfo("Editor Module : Use VITS Editor Module to attach VITS module easily and generate audio for each module.")]
    [CeresGroup("Editor/AIGC")]
    [ModuleOf(typeof(Dialogue))]
    public class VITSEditorModule : EditorModule
    {
#if UNITY_EDITOR
#pragma warning disable IDE0052
        [SerializeField, Tooltip("Skip vits modules contained audioClip"), Setting]
        private bool skipContainedAudioClip = true;
        
        [SerializeField, Tooltip("Skip vits modules used shared audioClip"), Setting]
        private bool skipSharedAudioClip = true;
        
        public VITSEditorModule() { }
        
        public VITSEditorModule(bool skipContainedAudioClip, bool skipSharedAudioClip)
        {
            this.skipContainedAudioClip = skipContainedAudioClip;
            this.skipSharedAudioClip = skipSharedAudioClip;
        }
#pragma warning restore IDE0052
#endif
    }
}
