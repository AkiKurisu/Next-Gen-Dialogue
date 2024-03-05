using System.Collections.Generic;
namespace Kurisu.NGDS.NLP
{
    /// <summary>
    /// Common recursive text splitter for natural language processing
    /// </summary>
    public class RecursiveCharacterTextSplitter : ISplitter
    {
        public char[] punctuations = new char[] { '。', '？', '！', '.', ':', ';', '!', '?', '~' };
        public List<string> Split(string input)
        {
            List<string> sentences = new();
            SplitRecursive(input, sentences);
            return sentences;
        }

        private void SplitRecursive(string input, List<string> sentences)
        {
            int endIndex = input.IndexOfAny(punctuations);
            if (endIndex != -1)
            {
                string sentence = input[..(endIndex + 1)];
                sentences.Add(sentence.Trim());
                string remainingText = input[(endIndex + 1)..];
                SplitRecursive(remainingText, sentences);
            }
            else
            {
                sentences.Add(input.Trim());
            }
        }
    }
}