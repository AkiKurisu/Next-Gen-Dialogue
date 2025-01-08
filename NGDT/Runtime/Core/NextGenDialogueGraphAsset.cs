using UnityEngine;
using Ceres.Graph;
using UObject = UnityEngine.Object;
namespace Kurisu.NGDT
{
    [CreateAssetMenu(fileName = "NextGenDialogueAsset", menuName = "Next Gen Dialogue/NextGenDialogueAsset")]
    public class NextGenDialogueGraphAsset : ScriptableObject, IDialogueGraphContainer
    {
        [HideInInspector, SerializeField]
        private DialogueGraphData graphData;
        
        public UObject Object => this;
        
        [Multiline, SerializeField]
        private string description;
       
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
            return new DialogueGraph(graphData.CloneT<DialogueGraphData>());
        }

        public void SetGraphData(CeresGraphData data)
        {
            graphData = (DialogueGraphData)data;
        }
    }
}
