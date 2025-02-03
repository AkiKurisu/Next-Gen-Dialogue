namespace NextGenDialogue.Graph.Editor
{
    public interface IContainChild
    {
        void AddChildElement(IDialogueNodeView node, DialogueGraphView graphView);
    }
}
