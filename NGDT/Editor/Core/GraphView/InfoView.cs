using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public class InfoView : VisualElement
    {
        private IMGUIContainer container;
        public InfoView(string info)
        {
            Clear();
            container = new IMGUIContainer();
            container.Add(new Label(info));
            Add(container);
        }
        public void UpdateSelection(IDialogueNode node)
        {
            container?.Dispose();
            container = null;
            Clear();
            AkiInfoAttribute[] array;
            if ((array = node.GetBehavior().GetCustomAttributes(typeof(AkiInfoAttribute), false) as AkiInfoAttribute[]).Length > 0)
            {
                container = new IMGUIContainer();
                container.Add(new Label(array[0].Description));
                Add(container);
            }
        }
    }
}