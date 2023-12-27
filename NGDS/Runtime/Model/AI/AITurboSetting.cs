using UnityEngine;
namespace Kurisu.NGDS.AI
{
        [CreateAssetMenu(fileName = "AITurboSetting", menuName = "Next Gen Dialogue/AITurboSetting")]
        public class AITurboSetting : ScriptableObject
        {
                private enum GPT_APIMode
                {
                        Chat, Completions
                }
                [field: Header("LLM Setting")]
                [field: SerializeField]
                public string ChatGPT_URL_Override { get; private set; }
                [SerializeField, Tooltip("Whether use GPT Chat Mode or Completions Mode, Chat Mode is better when only has two role")]
                private GPT_APIMode gpt_APIMode;
                public bool ChatMode => gpt_APIMode == GPT_APIMode.Chat;
                [field: SerializeField]
                public string OpenAIKey { get; private set; }
                [field: SerializeField]
                public string LLM_Address { get; private set; } = "127.0.0.1";
                [field: SerializeField]
                public string LLM_Port { get; private set; } = "5001";
                [field: Header("Translation Setting")]
                [field: SerializeField, Tooltip("Enable this to use auto google translation before LLM input")]
                public bool Enable_GoogleTranslation { get; private set; }
                [field: SerializeField, Tooltip("LLM output and input language code")]
                public string LLM_Language { get; private set; } = "en";
#if NGD_USE_VITS
                [field: SerializeField, Tooltip("VITS input language code")]
                public string VITS_Language { get; private set; } = "ja";
                [field: Header("VITS Setting (Experimental)")]
                [field: SerializeField]
                public string VITS_Address { get; private set; } = "127.0.0.1";
                [field: SerializeField]
                public string VITS_Port { get; private set; } = "23456";
                [field: SerializeField, Tooltip("Specify the language, leave empty to use auto mode.")]
                public string VITS_Lang { get; private set; } = string.Empty;
                [field: SerializeField, Tooltip("Specify the length of output which effects the speed of speaking.")]
                public string VITS_Length { get; private set; } = string.Empty;
#endif
        }
}
