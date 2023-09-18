using System.Collections.Generic;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Localization.Editor
{
    /// <summary>
    /// Toggle Group that can hide and show elements by toggle index
    /// </summary>
    public class ToggleGroup : VisualElement
    {
        private int currentIndex;
        public event System.Action<int> OnToggle;
        private readonly List<VisualElement> elements = new();
        /// <summary>
        /// Toggle Group will only set visible of element but don't remove it from parent (eg. use for propertyField)
        /// </summary>
        /// <value></value>
        public bool DoNotRemove { get; set; }
        public void AddToggleElement(VisualElement element)
        {
            elements.Add(element);
            if (DoNotRemove) Add(element);
        }
        public void Toggle(int index)
        {
            currentIndex = index;
            for (int i = 0; i < elements.Count; i++)
            {
                bool isCurrent = currentIndex == i;
                elements[i].visible = isCurrent;
                if (isCurrent) Insert(0, elements[i]);
                else if (!DoNotRemove && Contains(elements[i])) Remove(elements[i]);
            }
            OnToggle?.Invoke(index);
        }
    }
}
