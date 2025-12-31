# VITS Extension Explanation

## Setup
The VITS extension package is used to support the use of the TTS speech synthesis model, which converts dialogues into speech in real time at runtime. Next Gen Dialogue does not provide language synthesis services. You can deploy the model locally, and then use the extension package to call the API of the server.

To deploy VITS locally, please Clone this repository: [VITS Simple API](https://github.com/Artrajz/vits-simple-api).

## Features

1. During runtime, ``VITSResolver`` will generate speech based on the current conversation content.
2. In the Editor, add ``VITSModule`` to the Piece or Option that has ``ContentModule``, right-click to bake the voice and preview and download it.(You need to download it to save it, otherwise it will be lost after exiting the editor)
3. If there is a referenced AudioClip in ``VITSModule``, the Runtime no longer generates it in real time but uses the speech.

## Bake In Editor

1. Manually add `AIGC/VITSModule` to the Container that needs to add voice or add `Editor/AIGC/VITSEditorModule` under the Dialogue node and add it to all Piece or Option with one click
2. Fill in `characterID` according to the inferred model ID
3. Right-click `AIGC/VITSModule` to generate speech or click `Editor/AIGC/VITSEditorModule` to batch generate (actually generated sequentially)