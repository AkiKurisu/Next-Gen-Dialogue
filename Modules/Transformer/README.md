# Transformer Extension Explanation Transformer拓展包说明

## Features 特点
基于`Unity.Sentis`和`HuggingFace.SharpTransformers`为对话树增加基于注意力机制（Transformer模型）的AI功能，例如文本匹配

Add attention mechanism (Transformer model)-based AI functions to the dialogue tree based on `Unity.Sentis` and `HuggingFace.SharpTransformers`, such as text matching

## Dependency 依赖库

1. `Unity.Sentis` 
   
   Add dependency in manifest.json :`"com.unity.sentis": "1.3.0-pre.1"`
2. `HuggingFace.SharpTransformers` 
   
   Download in packageManager by git
   https://github.com/huggingface/sharp-transformers

## How to convert Transformer model to ONNX 如何转换Transformer模型到Onnx格式

1. cd to Python folder in Example

2. Install dependency library `pip install -r requirements.txt`

3. Use Convert.py to convert bert model on HuggingFace to onnx format

```python
from optimum.onnxruntime import ORTModel
if __name__ == "__main__": 
    model = ORTModel.from_pretrained("shibing624/text2vec-base-multilingual",from_transformers=True,export=True)
    model.save_pretrained("onnx/")
```