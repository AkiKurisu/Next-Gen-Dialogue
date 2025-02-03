using System;
using UnityEngine;
using UnityEngine.UI;
namespace Kurisu.NGDS.Example
{
    public class OptionUI : MonoBehaviour
    {
        private Text _optionText;
        
        private Button _button;
        
        private Option _option;
        
        private Action<Option> _onClick;
        
        private RectTransform _rectTransform;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _optionText = GetComponentInChildren<Text>();
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnOptionClick);
        }
        
        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }
        
        public void UpdateOption(Option option, Action<Option> callBack)
        {
            this._option = option;
            _optionText.text = option.Content;
            _onClick = callBack;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_optionText.rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
        }
        
        private void OnOptionClick()
        {
            _onClick?.Invoke(_option);
        }
    }
}