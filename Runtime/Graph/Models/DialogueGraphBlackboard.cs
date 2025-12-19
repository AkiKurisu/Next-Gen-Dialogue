using Ceres.Graph;

namespace NextGenDialogue.Graph
{
    public class DialogueGraphBlackboard : Blackboard
    {
        protected override bool CanVariableExposed(SharedVariable variable)
        {
            return variable is not PieceID;
        }
    }
}