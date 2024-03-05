using System.Collections.Generic;
namespace Kurisu.NGDS.NLP
{
    public interface ISplitter
    {
        /// <summary>
        /// Split substrings from input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        List<string> Split(string input);
    }
}