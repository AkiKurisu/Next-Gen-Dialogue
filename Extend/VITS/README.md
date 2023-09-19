# VITS Extension Explanation VITS拓展包说明

## Setup 安装
VITS拓展包用于支持使用TTS语音合成模型，从而在运行时将对话实时转换为语音。Next Gen Dialogue不会提供语言合成服务，您可以将模型自行部署在本地，然后使用该拓展包调用Server的API。

VITS本地部署请Clone该仓库：[VITS Simple API](https://github.com/Artrajz/vits-simple-api)。

The VITS extension package is used to support the use of the TTS speech synthesis model, which converts dialogues into speech in real time at runtime. Next Gen Dialogue does not provide language synthesis services. You can deploy the model locally, and then use the extension package to call the API of the server.

To deploy VITS locally, please Clone this repository: [VITS Simple API](https://github.com/Artrajz/vits-simple-api).

## Features 特点

1. Runtime时``VITSResolver``会根据当前对话内容生成语音
    
    During runtime, ``VITSResolver`` will generate speech based on the current conversation content.
2. Editor中在拥有```ContentModule```的Piece或Option中添加``VITSModule``右键烘焙语音并进行预览和下载
    
    In the Editor, add ``VITSModule`` to the Piece or Option that has ``ContentModule``, right-click to bake the voice and preview and download it.