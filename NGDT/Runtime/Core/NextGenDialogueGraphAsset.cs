using System.Collections.Generic;
using Ceres;
using UnityEngine;
using Ceres.Graph;
using UObject = UnityEngine.Object;
namespace Kurisu.NGDT
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Next Gen Dialogue/Dialogue Asset")]
    public class NextGenDialogueGraphAsset : ScriptableObject, IDialogueGraphContainer
    {
        [HideInInspector, SerializeField]
        private DialogueGraphData graphData;
        
        public UObject Object => this;
        
        [Multiline, SerializeField]
        private string description;

        /* Migration from v1 */
#if UNITY_EDITOR
        [SerializeReference, HideInInspector]
        internal Root root = new();
        
        [SerializeReference, HideInInspector]
        internal List<SharedVariable> sharedVariables = new();
#endif
       
        public void Deserialize(string serializedData)
        {
            var data = CeresGraphData.FromJson<DialogueGraphData>(serializedData);
            if (data == null)
            {
                return;
            }
            SetGraphData(data);
        }
        
        public CeresGraph GetGraph()
        {
            return GetDialogueGraph();
        }
        
        public DialogueGraph GetDialogueGraph()
        {
            graphData ??= new DialogueGraphData();
#if UNITY_EDITOR
            if (!graphData.IsValid())
            {
                return new DialogueGraph(this);
            }
#endif
            return new DialogueGraph(graphData.CloneT<DialogueGraphData>());
        }

        public void SetGraphData(CeresGraphData data)
        {
            graphData = (DialogueGraphData)data;
        }
    }
}
