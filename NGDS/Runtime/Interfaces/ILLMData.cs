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
        IEnumerable<IMessage> Messages { get; }
    }
    public interface ILargeLanguageModel
    {
        /// <summary>
        /// Generate llm data from unstructured llm request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ILLMResponse> GenerateAsync(ILLMRequest request, CancellationToken ct);
        /// <summary>
        /// Generate llm data from string input
        /// </summary>
        /// <param name="input"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ILLMResponse> GenerateAsync(string input, CancellationToken ct);
    }
}