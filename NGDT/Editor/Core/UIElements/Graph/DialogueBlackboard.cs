using System.Linq;
using Ceres;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public class DialogueBlackboard: CeresBlackboard
    {
        public DialogueBlackboard(CeresGraphView graphView) : base(graphView)
        {
        }

        protected override bool CanVariableExposed(SharedVariable variable)
        {
            return variable is not PieceID;
        }

        protected override BlackboardRow CreateBlackboardPropertyRow(SharedVariable variable, BlackboardField blackboardField, VisualElement valueField)
        {
            var propertyView = base.CreateBlackboardPropertyRow(variable, blackboardField, valueField);
            if (variable is PieceID)
            {
                blackboardField.RegisterCallback<ClickEvent>((evt) => FindRelatedPiece(variable));
                propertyView.Q<Button>("expandButton").RemoveFromHierarchy();
            }
            return propertyView;
        }
        private void FindRelatedPiece(SharedVariable variable)
        {
            var piece = graphView.nodes.OfType<PieceContainer>().FirstOrDefault(x => x.GetPieceID() == variable.Name);
            if (piece != null)
            {
                graphView.AddToSelection(piece);
            }
        }
    }
}