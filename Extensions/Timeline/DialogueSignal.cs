using NextGenDialogue.Graph;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace NextGenDialogue.Timeline
{
    public class DialogueSignal : Marker, INotification
    {
        public string dialogueName;
        
        public NextGenDialogueGraphAsset dialogueGraphAsset;
        
        public bool pausePlayable;

        public PropertyName id => dialogueName;
    }
}
