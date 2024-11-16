using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
namespace Kurisu.NGDT.Timeline
{
    public class DialogueSignal : Marker, INotification
    {
        public string dialogueName;
        public NextGenDialogueAsset dialogueAsset;
        public bool pausePlayable;
        public PropertyName id { get; }
    }
}
