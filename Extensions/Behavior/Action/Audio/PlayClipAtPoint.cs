using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Plays an AudioClip at a given position in world space")]
    [NodeLabel("Audio: PlayClipAtPoint")]
    [NodeGroup("Audio")]
    public class PlayClipAtPoint : Action
    {
        public SharedTObject<AudioClip> audioClip;
        public SharedVector3 position;
        protected override Status OnUpdate()
        {
            AudioSource.PlayClipAtPoint(audioClip.Value, position.Value);
            return Status.Success;
        }
    }
}
