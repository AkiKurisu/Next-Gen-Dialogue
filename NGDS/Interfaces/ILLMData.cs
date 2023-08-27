using System.Collections.Generic;
using System.Threading.Tasks;
using Kurisu.NGDS.AI;
namespace Kurisu.NGDS
{
    public interface ILLMData
    {
        bool Status { get; }
        string Response { get; }
    }
    public interface ILLMInput
    {
        string Character { get; }
        IEnumerable<string> OtherCharacters { get; }
        Queue<DialogueParam> History { get; }
    }
    public interface ILLMDriver
    {
        GoogleTranslateModule? PreTranslateModule { get; set; }
        Task<ILLMData> ProcessLLM(ILLMInput input);
        void SetPrompt(string prompt);
    }
    public enum LLMType
    {
        ChatGPT, KoboldCPP, Oobabooga, ChatGLM, ChatGLM_OpenAI
    }
}