using System.Collections.Generic;
using Newtonsoft.Json;
namespace Kurisu.NGDS.AI
{
    //Modify from https://github.com/pboardman/KoboldSharp
    public class KoboldGenParams
    {
        [JsonProperty("n")]
        public int N { get; set; }
        [JsonProperty("max_context_length")]
        public int MaxContextLength { get; set; }
        [JsonProperty("max_length")]
        public int MaxLength { get; set; }
        [JsonProperty("rep_pen")]
        public float RepPen { get; set; }
        [JsonProperty("temperature")]
        public float Temperature { get; set; }
        [JsonProperty("top_p")]
        public float TopP { get; set; }
        [JsonProperty("top_k")]
        public int TopK { get; set; }
        [JsonProperty("top_a")]
        public int TopA { get; set; }
        [JsonProperty("typical")]
        public int Typical { get; set; }
        [JsonProperty("tfs")]
        public int Tfs { get; set; }
        [JsonProperty("rep_pen_range")]
        public int RepPenRange { get; set; }
        [JsonProperty("rep_pen_slope")]
        public float RepPenSlope { get; set; }
        [JsonProperty("sampler_order")]
        public List<int> SamplerOrder { get; set; }
        [JsonProperty("prompt")]
        public string Prompt { get; set; }
        [JsonProperty("quiet")]
        public bool Quiet { get; set; }
        [JsonProperty("stop_sequence")]
        public List<string> StopSequence { get; set; }

        public KoboldGenParams()
        {
            //Preset
            const int n = 1;
            const int maxContextLength = 1024;
            const int maxLength = 80;
            const float repPen = 1.1f;
            const float temperature = 0.7f;
            const float topP = 0.92f;
            const int topK = 0;
            const int topA = 0;
            const int typical = 1;
            const int tfs = 1;
            const int repPenRange = 300;
            const float repPenSlope = 0.7f;
            const bool quiet = true;
            //Load
            N = n;
            MaxContextLength = maxContextLength;
            MaxLength = maxLength;
            RepPen = repPen;
            Temperature = temperature;
            TopP = topP;
            TopK = topK;
            TopA = topA;
            Typical = typical;
            Tfs = tfs;
            RepPenRange = repPenRange;
            RepPenSlope = repPenSlope;
            SamplerOrder = new List<int> { 6, 0, 1, 3, 4, 2, 5 };
            Quiet = quiet;
            StopSequence = new List<string>() { "You:", "\nYou " };
        }
        public string GetJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
