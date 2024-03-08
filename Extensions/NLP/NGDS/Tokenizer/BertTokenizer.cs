using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;
using Newtonsoft.Json.Linq;
using HuggingFace.SharpTransformers.Normalizers;
using HuggingFace.SharpTransformers.PreTokenizers;
using HuggingFace.SharpTransformers.Tokenizers;
using HuggingFace.SharpTransformers.PostProcessors;
using Newtonsoft.Json;
namespace Kurisu.NGDS.NLP
{
    //Modified from https://thomassimonini.substack.com/p/create-an-ai-robot-npc-using-hugging?r=dq5fg&utm_campaign=post&utm_medium=web
    public class BertTokenizer
    {
        private readonly BertNormalizer bertNorm;
        private readonly BertPreTokenizer bertPreTok;
        private readonly WordPieceTokenizer wordPieceTokenizer;
        private readonly TemplateProcessing templateProcessing;
        private readonly JObject tokenizerJsonObject;
        public BertTokenizer(JObject tokenizerJsonObject)
        {
            this.tokenizerJsonObject = tokenizerJsonObject;
            bertNorm = new(JObject.FromObject(tokenizerJsonObject["normalizer"]));
            bertPreTok = new(JObject.FromObject(tokenizerJsonObject["pre_tokenizer"]));
            wordPieceTokenizer = new(JObject.FromObject(tokenizerJsonObject["model"]));
            templateProcessing = new(JObject.FromObject(tokenizerJsonObject["post_processor"]));
        }
        public BertTokenizer(string tokenizerJsonData)
        {
            tokenizerJsonObject = JsonConvert.DeserializeObject<JObject>(tokenizerJsonData);
            bertNorm = new(JObject.FromObject(tokenizerJsonObject["normalizer"]));
            bertPreTok = new(JObject.FromObject(tokenizerJsonObject["pre_tokenizer"]));
            wordPieceTokenizer = new(JObject.FromObject(tokenizerJsonObject["model"]));
            templateProcessing = new(JObject.FromObject(tokenizerJsonObject["post_processor"]));
        }
        /// <summary>
        /// Tokenize the sentences
        /// </summary>
        /// <param name="candidates"></param>
        /// <returns>Tuple of 3 lists:
        /// - TokenIds
        /// - Attention Mask
        /// - TokenTypeIds </returns>
        private Tuple<List<List<int>>, List<List<int>>, List<List<int>>> TokenizeInputs(IReadOnlyList<string> inputs)
        {
            List<List<int>> sentences = new();
            foreach (string text in inputs)
            {
                // Normalize the sentence
                string normalized = bertNorm.Normalize(text);
                // Pretokenize the sentence
                List<string> pretokenized = bertPreTok.PreTokenize(normalized);
                // Tokenize with WordPieceTokenizer
                List<string> tokenized = wordPieceTokenizer.Encode(pretokenized);
                // Template Process
                List<string> processed = templateProcessing.PostProcess(tokenized);
                // Tokens to Ids
                List<int> ids = wordPieceTokenizer.ConvertTokensToIds(processed);
                sentences.Add(ids);
            }
            int max_length = (int)tokenizerJsonObject["truncation"]["max_length"];
            Tuple<List<List<int>>, List<List<int>>> tuple_ = PaddingOrTruncate(true, true, sentences, max_length, JObject.FromObject(tokenizerJsonObject["padding"]));
            return Tuple.Create(tuple_.Item2, tuple_.Item1, AddTokenTypes(tuple_.Item2));
        }
        public List<string> Decode(List<int> ids)
        {
            return wordPieceTokenizer.ConvertIdsToTokens(ids);
        }
        /// <summary>
        /// Tokenize the input to be fed to the Transformer model
        /// </summary>
        /// <param name="sentences"></param>
        /// <returns></returns>
        public Dictionary<string, Tensor> Tokenize(IReadOnlyList<string> sentences)
        {

            Tuple<List<List<int>>, List<List<int>>, List<List<int>>> FinalTuple = TokenizeInputs(sentences);

            List<List<int>> TokenIds = FinalTuple.Item1;
            List<List<int>> AttentionMask = FinalTuple.Item2;
            List<List<int>> TokenTypeIds = FinalTuple.Item3;
            // Flatten the nested list into a one-dimensional array
            List<int> flattenedData = new();
            foreach (var innerList in TokenIds)
            {
                flattenedData.AddRange(innerList);
            }
            int[] data = flattenedData.ToArray();
            // Create a 3D tensor shape
            TensorShape shape = new(TokenIds.Count, TokenIds[0].Count);
            // Create a new tensor from the array
            TensorInt TokenIdsTensor = new(shape, data);
            // Flatten the nested list into a one-dimensional array
            List<int> flattenedData2 = new();
            foreach (var innerList in AttentionMask)
            {
                flattenedData2.AddRange(innerList);
            }
            int[] data2 = flattenedData2.ToArray();
            // Create a 3D tensor shape
            TensorShape shape2 = new(AttentionMask.Count, AttentionMask[0].Count);
            // Create a new tensor from the array
            TensorInt AttentionMaskTensor = new(shape2, data2);
            // Flatten the nested list into a one-dimensional array
            List<int> flattenedData3 = new();
            foreach (var innerList in TokenTypeIds)
            {
                flattenedData3.AddRange(innerList);
            }
            int[] data3 = flattenedData3.ToArray();
            // Create a 3D tensor shape
            TensorShape shape3 = new(TokenTypeIds.Count, TokenTypeIds[0].Count);
            // Create a new tensor from the array
            TensorInt TokenTypeIdsTensor = new(shape3, data3);

            Dictionary<string, Tensor> inputTensors = new() {
                { "input_ids", TokenIdsTensor },
                {"token_type_ids", TokenTypeIdsTensor },
                { "attention_mask", AttentionMaskTensor }
                };

            return inputTensors;
        }


        /// <summary>
        /// Padding or Truncate Tensor
        /// </summary>
        /// <param name="padding"></param>
        /// <param name="truncation"></param>
        /// <param name="tokens"></param>
        /// <param name="max_length"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private static Tuple<List<List<int>>, List<List<int>>> PaddingOrTruncate(bool padding, bool truncation, List<List<int>> tokens, int max_length, JObject config)
        {
            // TODO allow user to change
            string padding_side = "right";

            int pad_token_id = 0; // TODO Change (int)config["pad_token"]

            List<List<int>> attentionMask = new();

            int maxLengthOfBatch = tokens.Max(x => x.Count);

            max_length = maxLengthOfBatch;

            // TODO Check the logic
            /*if (max_length == null)
            {
                max_length = maxLengthOfBatch;
            }

            max_length = Math.Min(max_length.Value, model_max_length);*/

            if (padding || truncation)
            {
                for (int i = 0; i < tokens.Count; ++i)
                {
                    if (tokens[i].Count == max_length)
                    {
                        attentionMask.Add(Enumerable.Repeat(1, tokens[i].Count).ToList());
                        continue;
                    }
                    else if (tokens[i].Count > max_length)
                    {
                        if (truncation)
                        {
                            tokens[i] = tokens[i].Take(max_length).ToList();
                        }
                        attentionMask.Add(Enumerable.Repeat(1, tokens[i].Count).ToList());
                    }
                    else
                    {
                        if (padding)
                        {
                            int diff = max_length - tokens[i].Count;

                            if (padding_side == "right")
                            {
                                attentionMask.Add(Enumerable.Repeat(1, tokens[i].Count)
                                    .Concat(Enumerable.Repeat(0, diff)).ToList());
                                tokens[i].AddRange(Enumerable.Repeat(pad_token_id, diff));
                            }
                            else
                            {
                                attentionMask.Add(Enumerable.Repeat(0, diff)
                                    .Concat(Enumerable.Repeat(1, tokens[i].Count)).ToList());
                                tokens[i].InsertRange(0, Enumerable.Repeat(pad_token_id, diff));
                            }
                        }
                        else
                        {
                            attentionMask.Add(Enumerable.Repeat(1, tokens[i].Count).ToList());
                        }
                    }
                }
            }
            else
            {
                attentionMask = tokens.Select(x => Enumerable.Repeat(1, x.Count).ToList()).ToList();
            }
            return Tuple.Create(attentionMask, tokens);
        }


        /// <summary>
        /// For now it's just copy inputIds and set all elements to 0
        /// </summary>
        /// <returns></returns>
        private static List<List<int>> AddTokenTypes(List<List<int>> inputIds)
        {
            return inputIds.Select(innerList => innerList.Select(_ => 0).ToList()).ToList();

        }
    }
}
