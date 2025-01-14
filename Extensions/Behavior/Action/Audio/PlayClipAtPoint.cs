using System;
using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [Serializable]
    [NodeInfo("Action: Plays an AudioClip at a given position in world space")]
    [CeresLabel("Audio: PlayClipAtPoint")]
    [CeresGroup("Audio")]
    public class PlayClipAtPoint : Action
    {
        public SharedUObject<AudioClip> audioClip;
        
        public SharedVector3 position;
        
        protected override Status OnUpdate()
        {
            AudioSource.PlayClipAtPoint(audioClip.Value, position.Value);
            return Status.Success;
        }
    }
}
