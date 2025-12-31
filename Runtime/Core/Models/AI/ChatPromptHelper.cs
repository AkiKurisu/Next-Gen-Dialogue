using System.Text;
namespace NextGenDialogue.AI
{
    public class ChatPromptHelper
    {
        /// <summary>
        /// Construct a chat style prompt with non-instruct
        /// </summary>
        public static string ConstructPrompt(
            string user_Name = "You",
            string char_name = "Bot",
            string char_persona = null,
            string world_scenario = null
        )
        {
            StringBuilder stringBuilder = new();
            if (!string.IsNullOrEmpty(char_persona))
                stringBuilder.AppendLine($"{char_name}'s persona: {char_persona.Replace("{{char}}", char_name).Replace("{{user}}", user_Name)}");
            if (!string.IsNullOrEmpty(world_scenario))
                stringBuilder.AppendLine($"World's scenario: {world_scenario.Replace("{{char}}", char_name).Replace("{{user}}", user_Name)}");
            //<START> means chat beginning
            stringBuilder.AppendLine("<START>");
            return stringBuilder.ToString();
        }
    }
}