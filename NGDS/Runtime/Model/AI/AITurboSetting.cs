using Kurisu.NGDS.Translator;
using UnityEngine;
namespace Kurisu.NGDS.AI
{
        [CreateAssetMenu(fileName = "AITurboSetting", menuName = "Next Gen Dialogue/AITurboSetting")]
        public class AITurboSetting : ScriptableObject
        {
                public enum GPT_APIMode
                {
                        Chat, Completions
                }
                public enum GPT_ModelType
                {
                        [InspectorName("GPT-3.5-Turbo")]
                        GPT3_5,
                        [InspectorName("GPT-4")]
                        GPT4
                }
                public enum VITS_ModelType
                {
                        VITS, BertVITS2
                }
                [field: Header("LLM Setting")]
                [field: SerializeField]
                public string ChatGPT_URL_Override { get; set; }
                [Tooltip("Whether use GPT Chat Mode or Completions Mode, Chat Mode is better when only has two role")]
                public GPT_APIMode gpt_APIMode;
                [Tooltip("Set ChatGPT model type")]
                public GPT_ModelType gptType;
                public string GPT_Model
                {
                        get
                        {
                                if (gptType == GPT_ModelType.GPT3_5) return "gpt-3.5-turbo";
                                else return "gpt-4";
                        }
                }
                public bool ChatMode => gpt_APIMode == GPT_APIMode.Chat;
                [field: SerializeField]
                public string OpenAIKey { get; set; }
                [field: SerializeField]
                public string LLM_Address { get; set; } = "127.0.0.1";
                [field: SerializeField]
                public string LLM_Port { get; set; } = "5001";
                [field: SerializeField, Tooltip("Set translator to use before LLM input"), Header("Translation Setting")]
                public TranslatorType TranslatorType { get; set; }
                [field: SerializeField, Tooltip("LLM output and input language code")]
                public string LLM_Language { get; set; } = "en";
                [field: SerializeField, Tooltip("VITS input language code")]
                public string VITS_Language { get; set; } = "ja";
                [field: SerializeField, Header("VITS Setting (Experimental)")]
                public string VITS_Address { get; set; } = "127.0.0.1";
                [field: SerializeField, Tooltip("Set vits model type")]
                public VITS_ModelType VITSType { get; set; }
                public string VITS_Model
                {
                        get
                        {
                                if (VITSType == VITS_ModelType.VITS) return "vits";
                                else return "bert-vits2";
                        }
                }
                [field: SerializeField]
                public string VITS_Port { get; set; } = "23456";
                [field: SerializeField, Tooltip("Specify the language, leave empty to use auto mode.")]
                public string VITS_Lang { get; set; } = string.Empty;
                [field: SerializeField, Tooltip("Specify the length of output which effects the speed of speaking.")]
                public string VITS_Length { get; set; } = string.Empty;
        }
}
