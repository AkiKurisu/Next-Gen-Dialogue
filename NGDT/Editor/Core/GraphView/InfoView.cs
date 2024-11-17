using System.Reflection;
using Ceres.Annotations;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public class InfoView : VisualElement
    {
        private IMGUIContainer _container;
        public InfoView(string info)
        {
            Clear();
            _container = new IMGUIContainer();
            _container.Add(new Label(info));
            Add(_container);
        }
        public void UpdateSelection(IDialogueNode node)
        {
            _container?.Dispose();
            _container = null;
            Clear();
            var attribute = node.GetBehavior().GetCustomAttribute<NodeInfoAttribute>();
            if (attribute != null)
            {
                _container = new IMGUIContainer();
                _container.Add(new Label(attribute.Description));
                Add(_container);
            }
        }
    }
}