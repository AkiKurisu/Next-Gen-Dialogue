using System.Collections.Generic;
namespace Kurisu.NGDS.NLP
{
    /// <summary>
    /// Common recursive text splitter for natural language processing
    /// </summary>
    public class RecursiveCharacterTextSplitter : ISplitter
    {
        public char[] punctuations = new char[] { '。', '？', '！', '.', ':', ';', '!', '?', '~' };
        public void Split(string input, IList<string> outputs)
        {
            int endIndex = input.IndexOfAny(punctuations);
            if (endIndex != -1)
            {
                string sentence = input[..(endIndex + 1)];
                outputs.Add(sentence.Trim());
                string remainingText = input[(endIndex + 1)..];
                Split(remainingText, outputs);
            }
            else
            {
                outputs.Add(input.Trim());
            }
        }
    }
}