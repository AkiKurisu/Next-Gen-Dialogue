using System.Linq;
using Ceres;
using Ceres.Editor.Graph;
using Ceres.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
namespace NextGenDialogue.Graph.Editor
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

        protected override BlackboardRow CreateVariableBlackboardRow(SharedVariable variable, BlackboardField blackboardField, VisualElement valueField)
        {
            var propertyView = base.CreateVariableBlackboardRow(variable, blackboardField, valueField);
            if (variable is not PieceID) return propertyView;
            
            blackboardField.RegisterCallback<ClickEvent>(_ => FindRelatedPiece(variable));
            propertyView.Q<Button>("expandButton").RemoveFromHierarchy();
            propertyView.AddToClassList("pieceId-blackboard");
            return propertyView;
        }

        protected override void AddVariableRow(SharedVariable variable, BlackboardRow blackboardRow)
        {
            if (variable is PieceID)
            {
                GetOrAddSection("Dialogue Pieces").Add(blackboardRow);
                return;
            }
            base.AddVariableRow(variable, blackboardRow);
        }

        private void FindRelatedPiece(SharedVariable variable)
        {
            var piece = graphView.nodes.OfType<PieceContainerView>().FirstOrDefault(x => x.GetPieceID() == variable.Name);
            if (piece != null)
            {
                graphView.AddToSelection(piece);
            }
        }
    }
}