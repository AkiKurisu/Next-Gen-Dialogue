using System;
using UnityEngine;
using UnityEngine.UI;
namespace Kurisu.NGDS.Example
{
    public class OptionUI : MonoBehaviour
    {
        private Text optionText;
        private Button button;
        private Option option;
        private Action<Option> onClickCallBack;
        private RectTransform rectTransform;
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            optionText = GetComponentInChildren<Text>();
            button = GetComponent<Button>();
            button.onClick.AddListener(OnOptionClick);
        }
        private void OnDestroy()
        {
            button.onClick.RemoveAllListeners();
        }
        public void UpdateOption(Option option, Action<Option> callBack)
        {
            this.option = option;
            optionText.text = option.Content;
            onClickCallBack = callBack;
            LayoutRebuilder.ForceRebuildLayoutImmediate(optionText.rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
        private void OnOptionClick()
        {
            onClickCallBack?.Invoke(option);
        }
    }
}