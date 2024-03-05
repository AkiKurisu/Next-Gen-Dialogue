using UnityEngine;
using Unity.Sentis;
namespace Kurisu.NGDS.NLP
{
    //Modified from https://thomassimonini.substack.com/p/create-an-ai-robot-npc-using-hugging?r=dq5fg&utm_campaign=post&utm_medium=web
    public static class NLPUtils
    {
        /// <summary>
        /// Perform Mean Pooling
        /// </summary>
        /// <param name="AttentionMaskTensor">Attention Mask Tensor</param>
        /// <param name="outputTensor">Output Tensor</param>
        /// <param name="ops">Ops on tensor</param>
        /// <returns></returns>
        public static TensorFloat MeanPooling(Tensor AttentionMaskTensor, TensorFloat outputTensor, Ops ops)
        {
            if (AttentionMaskTensor == null || outputTensor == null)
            {
                Debug.LogError("AttentionMaskTensor or outputTensor is null.");
            }

            // Create an attention mask and 
            // add a new dimension (to make the mask compatible for element wise multiplication with token embeddings)
            TensorFloat AttentionMaskTensorFloat = ops.Cast(AttentionMaskTensor, DataType.Float) as TensorFloat;
            Tensor InputMaskExpanded = AttentionMaskTensorFloat.ShallowReshape(AttentionMaskTensorFloat.shape.Unsqueeze(-1));
            TensorFloat InputMaskExpandedFloat = ops.Cast(InputMaskExpanded, DataType.Float) as TensorFloat;

            TensorShape outputShape = outputTensor.shape;

            // Expand to 384 => [2, 6, 384]
            InputMaskExpandedFloat = ops.Expand(InputMaskExpandedFloat, outputShape);

            // torch.sum(token_embeddings * input_mask_expanded, 1) / torch.clamp(input_mask_expanded.sum(1), min=1e-9)
            TensorFloat temp_ = ops.Mul(outputTensor, InputMaskExpandedFloat);
            TensorFloat MeanPooledTensor = ops.ReduceMean(temp_, new int[] { 1 }, false);

            return MeanPooledTensor;
        }


        /// <summary>
        /// L2 Normalization
        /// </summary>
        /// <param name="MeanPooledTensor"></param>
        /// <param name="ops">Ops on tensor</param>
        /// <returns></returns>
        public static TensorFloat L2Norm(TensorFloat MeanPooledTensor, Ops ops)
        {
            // L2 NORMALIZATION
            // Compute L2 norm along axis 1 (dim=1)
            TensorFloat l2Norms = ops.ReduceL2(MeanPooledTensor, new int[] { 1 }, true);

            // Broadcast the L2 norms to the original shape
            TensorFloat l2NormsBroadcasted = ops.Expand(l2Norms, MeanPooledTensor.shape) as TensorFloat;

            // Divide sentence_embeddings by their L2 norms to achieve normalization
            TensorFloat NormalizedEmbeddings = ops.Div(MeanPooledTensor, l2NormsBroadcasted);

            return NormalizedEmbeddings;
        }
    }
}
