using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Localization.Editor
{
    /// <summary>
    /// Toggle Group that can hide and show elements by toggle index
    /// </summary>
    public class ToggleGroup : VisualElement
    {
        private int _currentIndex;
        
        public Action<int> OnToggle;
        
        private readonly List<VisualElement> _elements = new();
        
        /// <summary>
        /// Toggle Group will only set visible of element but don't remove it from parent (eg. use for propertyField)
        /// </summary>
        /// <value></value>
        public bool DoNotRemove { get; set; }
        
        public void AddToggleElement(VisualElement element)
        {
            _elements.Add(element);
            if (DoNotRemove) Add(element);
        }
        
        public void Toggle(int index)
        {
            _currentIndex = index;
            for (int i = 0; i < _elements.Count; i++)
            {
                bool isCurrent = _currentIndex == i;
                _elements[i].visible = isCurrent;
                if (isCurrent) Insert(0, _elements[i]);
                else if (!DoNotRemove && Contains(_elements[i])) Remove(_elements[i]);
            }
            OnToggle(index);
        }
    }
}
