# NLP Extension Explanation 自然语言处理拓展包说明

## Features 特点
基于`Unity.Sentis`和`HuggingFace.SharpTransformers`为对话树增加NLP的AI功能，例如`文本匹配`、`情感识别`和`文本向量嵌入`等。

Add natural language processing functions to the dialogue tree based on `Unity.Sentis` and `HuggingFace.SharpTransformers`, such as `Sentence Similarity`, `Sentiment Analysis` and `Text2Vec`, etc.

## Dependency 依赖库

1. `Unity.Sentis` 
   
   Add dependency in manifest.json :`"com.unity.sentis": "1.3.0-pre.1"`
2. `HuggingFace.SharpTransformers` 
   
   Download in packageManager by git
   https://github.com/huggingface/sharp-transformers

## How to convert Transformer model to ONNX 如何转换Transformer模型到Onnx格式

1. cd to Python folder in Example

2. Install dependency library `pip install -r requirements.txt`

3. Use onnxruntime to convert HuggingFace model to onnx format

```python
# Example
from optimum.onnxruntime import ORTModel
if __name__ == "__main__": 
    model = ORTModel.from_pretrained("shibing624/text2vec-base-multilingual",from_transformers=True,export=True)
    model.save_pretrained("onnx/")
```