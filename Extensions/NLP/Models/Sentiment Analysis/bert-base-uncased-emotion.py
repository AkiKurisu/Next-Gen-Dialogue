from optimum.onnxruntime import ORTModelForSequenceClassification
if __name__ == "__main__": 
    #Change model you like
    model = ORTModelForSequenceClassification.from_pretrained("bhadresh-savani/bert-base-uncased-emotion",from_transformers=True,export=True)
    model.save_pretrained("onnx/")