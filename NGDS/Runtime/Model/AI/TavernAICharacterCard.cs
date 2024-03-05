using System.Text;
namespace Kurisu.NGDS.AI
{
    public class TavernAICharacterCard
    {
        public string char_name;
        public string char_persona;
        public string char_greeting;
        public string example_dialogue;
        public string world_scenario;
        public string ConstructPrompt(string user_Name)
        {
            StringBuilder stringBuilder = new();
            if (!string.IsNullOrEmpty(char_persona))
                stringBuilder.AppendLine($"{char_name}'s persona: {ReplaceCharName(char_persona)}");
            if (!string.IsNullOrEmpty(world_scenario))
                stringBuilder.AppendLine($"Scenario: {ReplaceCharName(world_scenario)}");
            if (!string.IsNullOrEmpty(example_dialogue))
            {
                // Few-shot
                stringBuilder.AppendLine("<START>");
                stringBuilder.AppendLine($"{ReplaceCharName(example_dialogue)}");
            }
            stringBuilder.AppendLine("<START>");
            if (!string.IsNullOrEmpty(char_greeting))
            {
                stringBuilder.AppendLine($"{ReplaceCharName(char_greeting)}");
            }
            return stringBuilder.ToString();
            string ReplaceCharName(string input)
            {
                return input.Replace("{{char}}", char_name).Replace("{{user}}", user_Name);
            }
        }
    }
}