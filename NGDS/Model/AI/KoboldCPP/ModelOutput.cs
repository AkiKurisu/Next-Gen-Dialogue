using System.Collections.Generic;
using Newtonsoft.Json;
namespace Kurisu.NGDS.AI
{
    //Modify from https://github.com/pboardman/KoboldSharp
    public class ModelOutput
    {
        [JsonProperty("results")]
        public List<Result> Results { get; set; }
    }

    public class Result
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
