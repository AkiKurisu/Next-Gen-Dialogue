using UnityEngine;
namespace Kurisu.NGDS.AI
{
        [CreateAssetMenu(fileName = "AITurboSetting", menuName = "Next Gen Dialogue/AITurboSetting")]
        public class AITurboSetting : ScriptableObject
        {
                [field: Header("LLM Setting")]
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
#if USE_VITS
                [field: SerializeField, Tooltip("VITS input language code")]
                public string VITS_Language { get; private set; } = "ja";
#endif
#if USE_VITS
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
