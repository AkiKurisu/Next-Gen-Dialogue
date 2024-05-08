using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace Kurisu.NGDS
{
    public interface ILLMResponse
    {
        string Response { get; }
    }
    public enum MessageRole
    {
        System,
        User,
        Bot
    }
    public interface IMessage
    {
        public MessageRole Role { get; }
        public string Content { get; }
    }
    public interface ILLMRequest
    {
        string Context { get; }
        /// <summary>
        /// LLM history
        /// </summary>
        /// <value></value>
        IEnumerable<IMessage> History { get; }
    }
    public interface ITranslator
    {
        Task<string> Translate(string input, CancellationToken ct);
    }
    public interface ILargeLanguageModel
    {
        /// <summary>
        /// Generate llm data from unstructured llm input
        /// </summary>
        /// <param name="input"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ILLMResponse> GenerateAsync(ILLMRequest input, CancellationToken ct);
        /// <summary>
        /// Generate llm data from structured input
        /// </summary>
        /// <param name="input"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ILLMResponse> GenerateAsync(string input, CancellationToken ct);
    }
}