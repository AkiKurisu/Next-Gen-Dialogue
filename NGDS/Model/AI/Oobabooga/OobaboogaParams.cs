using System.Collections.Generic;
using Newtonsoft.Json;
namespace Kurisu.NGDS.AI
{
    //Modify from https://github.com/oobabooga/text-generation-webui/blob/main/api-examples/api-example.py
    public class OobaboogaParams
    {
        [JsonProperty("n")]
        public int N { get; set; } = 1;
        [JsonProperty("max_new_tokens")]
        public int MaxNewTokens { get; set; } = 250;
        [JsonProperty("auto_max_new_tokens")]
        public bool AutoMaxNewTokens { get; set; } = false;
        [JsonProperty("repetition_penalty")]
        public float RepPen { get; set; } = 1.18f;
        [JsonProperty("repetition_penalty_range")]
        public int RepPenRange { get; set; } = 0;
        [JsonProperty("temperature")]
        public float Temperature { get; set; } = 0.7f;
        [JsonProperty("top_p")]
        public float TopP { get; set; } = 0.1f;
        [JsonProperty("top_k")]
        public int TopK { get; set; } = 40;
        [JsonProperty("top_a")]
        public int TopA { get; set; } = 0;
        [JsonProperty("typical_p")]
        public int Typical { get; set; } = 1;
        [JsonProperty("tfs")]
        public int Tfs { get; set; } = 1;
        [JsonProperty("prompt")]
        public string Prompt { get; set; } = "None";
        [JsonProperty("early_stopping")]
        public bool EarlyStopping { get; set; } = false;
        [JsonProperty("guidance_scale")]
        public int GuidanceScale { get; set; } = 1;
        [JsonProperty("negative_prompt")]
        public string NegativePrompt { get; set; } = string.Empty;
        [JsonProperty("seed")]
        public int Seed { get; set; } = -1;
        [JsonProperty("add_bos_token")]
        public bool AddBosToken { get; set; } = true;
        [JsonProperty("truncation_length")]
        public int TruncationLength { get; set; } = 2048;
        [JsonProperty("ban_eos_token")]
        public bool BanEosToken { get; set; } = false;
        [JsonProperty("skip_special_tokens")]
        public bool SkipSpecialTokens { get; set; } = true;
        [JsonProperty("stopping_strings")]
        public List<string> StopStrings { get; set; } = new List<string>() { "You:", "\nYou " };
        public string GetJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
