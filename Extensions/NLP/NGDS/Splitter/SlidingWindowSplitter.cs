using System.Collections.Generic;
namespace Kurisu.NGDS.NLP
{
    /// <summary>
    /// Represents a sliding window algorithm
    /// </summary>
    public class SlidingWindowSplitter : ISplitter
    {
        private readonly int windowSize;
        public char[] punctuations = new char[] { '。', '？', '！', '.', ':', ';', '!', '?', '~' };
        public SlidingWindowSplitter(int windowSize)
        {
            this.windowSize = windowSize;
        }
        public void Split(string input, IList<string> outputs)
        {
            int pointer1 = 0;
            int pointer2 = windowSize;

            while (pointer2 < input.Length)
            {
                // Find the last occurrence of a delimiter between pointer1 and pointer2
                int lastDelimiterIndex = input.LastIndexOfAny(punctuations, pointer2, windowSize);

                // If a delimiter is found, split the segment and add it to the result
                if (lastDelimiterIndex != -1)
                {
                    string segment = input.Substring(pointer1, lastDelimiterIndex - pointer1 + 1).Trim();
                    outputs.Add(segment);
                    pointer2 = pointer1 = lastDelimiterIndex + 1;
                }
                else
                {
                    // No delimiter found, split the segment at pointer2
                    string segment = input[pointer1..pointer2].Trim();
                    outputs.Add(segment);
                    pointer1 = pointer2;
                }
                pointer2 += windowSize;
            }
            pointer2 = input.Length;
            if (pointer2 > pointer1)
            {
                outputs.Add(input[pointer1..pointer2].Trim());
            }
        }
    }
}