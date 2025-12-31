using System;
using UnityEngine;
using UnityEngine.UI;

namespace NextGenDialogue.VITS.Example
{
    public class VITSOptionUI : MonoBehaviour
    {
        private Text _optionText;
        
        private Button _button;
        
        private Option _option;
        
        private Action<Option> _onClickCallBack;
        
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
            _option = option;
            _optionText.text = option.Content;
            _onClickCallBack = callBack;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_optionText.rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
        }
        
        private void OnOptionClick()
        {
            _onClickCallBack?.Invoke(_option);
        }
    }
}
