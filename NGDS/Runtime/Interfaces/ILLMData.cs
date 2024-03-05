using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kurisu.NGDS.AI;
namespace Kurisu.NGDS
{
    public interface ILLMOutput
    {
        bool Status { get; }
        string Response { get; }
    }
    public interface ILLMInput
    {
        /// <summary>
        /// The character of LLM output
        /// </summary>
        /// <value></value>
        string OutputCharacter { get; }
        /// <summary>
        /// The characters of LLM input
        /// </summary>
        /// <value></value>
        IEnumerable<string> InputCharacters { get; }
        /// <summary>
        /// LLM history
        /// </summary>
        /// <value></value>
        IEnumerable<DialogueParam> History { get; }
    }
    public interface ITranslator
    {
        Task<string> Process(string input, CancellationToken ct);
    }
    public interface ILLMDriver
    {
        /// <summary>
        /// Generate llm data from unstructured llm input
        /// </summary>
        /// <param name="input"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ILLMOutput> ProcessLLM(ILLMInput input, CancellationToken ct);
        /// <summary>
        /// Generate llm data from structured input
        /// </summary>
        /// <param name="input"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ILLMOutput> ProcessLLM(string input, CancellationToken ct);
        /// <summary>
        /// Set llm system prompt
        /// </summary>
        /// <param name="prompt"></param>
        void SetSystemPrompt(string prompt);
    }
    public enum LLMType
    {
        ChatGPT, KoboldCPP, Oobabooga, ChatGLM
    }
}