from optimum.onnxruntime import ORTModel
if __name__ == "__main__": 
    model = ORTModel.from_pretrained("shibing624/text2vec-base-multilingual",from_transformers=True,export=True)
    model.save_pretrained("onnx/")