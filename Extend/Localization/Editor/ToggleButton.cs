using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.Localization.Editor
{
    /// <summary>
    /// Toggle Button that can use as Toggle with button appearance
    /// </summary>
    public class ToggleButton : Button
    {
        public bool IsOn { get; private set; }
        public System.Action<bool> OnToggle;
        public ToggleButton() : base()
        {
            clicked += Toggle;
        }
        public ToggleButton(System.Action<bool> toggleCallBack) : base()
        {
            clicked += Toggle;
            OnToggle += toggleCallBack;
        }
        private void Toggle()
        {
            IsOn = !IsOn;
            style.backgroundColor = IsOn ? OnColor : Color.grey;
            OnToggle?.Invoke(IsOn);
        }
        public Color OnColor { get; set; } = new Color(253 / 255f, 163 / 255f, 255 / 255f);
    }
}
