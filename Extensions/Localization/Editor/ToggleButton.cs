using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Localization.Editor
{
    /// <summary>
    /// Toggle Button that can use as Toggle with button appearance
    /// </summary>
    public class ToggleButton : Button
    {
        public bool IsOn { get; private set; }
        
        public readonly System.Action<bool> OnToggle;
                
        private readonly Color _color = new(253 / 255f, 163 / 255f, 255 / 255f);
        
        public ToggleButton()
        {
            clicked += Toggle;
        }
        
        public ToggleButton(System.Action<bool> toggleCallBack)
        {
            clicked += Toggle;
            OnToggle += toggleCallBack;
        }
        
        private void Toggle()
        {
            IsOn = !IsOn;
            style.backgroundColor = IsOn ? _color : Color.grey;
            OnToggle?.Invoke(IsOn);
        }
    }
}
