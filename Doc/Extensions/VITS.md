# VITS Extension Explanation VITS拓展包说明

## Setup 安装
VITS拓展包用于支持使用TTS语音合成模型，从而在运行时将对话实时转换为语音。Next Gen Dialogue不会提供语言合成服务，您可以将模型自行部署在本地，然后使用该拓展包调用Server的API。

VITS本地部署请Clone该仓库：[VITS Simple API](https://github.com/Artrajz/vits-simple-api)。

The VITS extension package is used to support the use of the TTS speech synthesis model, which converts dialogues into speech in real time at runtime. Next Gen Dialogue does not provide language synthesis services. You can deploy the model locally, and then use the extension package to call the API of the server.

To deploy VITS locally, please Clone this repository: [VITS Simple API](https://github.com/Artrajz/vits-simple-api).

## Features 特点

1. Runtime时``VITSResolver``会根据当前对话内容生成语音
    
    During runtime, ``VITSResolver`` will generate speech based on the current conversation content.
2. Editor中在拥有```ContentModule```的Piece或Option中添加``VITSModule``右键烘焙语音并进行预览和下载（需要下载才能保存，否则退出编辑器后丢失）
    
    In the Editor, add ``VITSModule`` to the Piece or Option that has ``ContentModule``, right-click to bake the voice and preview and download it.(You need to download it to save it, otherwise it will be lost after exiting the editor)
3. 如果``VITSModule``中存在引用的AudioClip，则Runtime不再实时生成而是使用该语音
   
   If there is a referenced AudioClip in ``VITSModule``, the Runtime no longer generates it in real time but uses the speech.

## Bake In Editor 编辑器中烘焙语音

1. 为需要添加语音的Container手动添加`AIGC/VITSModule`或在Dialogue结点下添加`Editor/AIGC/VITSEditorModule`后一键为所有Piece或Option添加

    Manually add `AIGC/VITSModule` to the Container that needs to add voice or add `Editor/AIGC/VITSEditorModule` under the Dialogue node and add it to all Piece or Option with one click
2. 根据推理的模型ID填写`characterID`

    Fill in `characterID` according to the inferred model ID
3. 右键`AIGC/VITSModule`生成语音或点击`Editor/AIGC/VITSEditorModule`的批量生成（实际为依次生成）

    Right-click `AIGC/VITSModule` to generate speech or click `Editor/AIGC/VITSEditorModule` to batch generate (actually generated sequentially)