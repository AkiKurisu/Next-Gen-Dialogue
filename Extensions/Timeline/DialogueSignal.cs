using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;
namespace Kurisu.NGDT.Timeline
{
    public class DialogueSignal : Marker, INotification
    {
        public string dialogueName;
        
        [FormerlySerializedAs("dialogueAsset")]
        public NextGenDialogueGraphAsset dialogueGraphAsset;
        
        public bool pausePlayable;
        
        public PropertyName id { get; }
    }
}
