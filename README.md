<div align="center">

# Next Gen Dialogue

Next Gen Dialogue is a Unity dialogue plugin combined with large language model design, won the 2023 Unity AI Plugin Excellence Award from Unity China. 

It combines the traditional dialogue design pattern with AIGC to simlify your workflow. Hopes you enjoy it. 

<Img src = "Documentation~/Images/BilibiliDemo.png">

Demo video https://www.bilibili.com/video/BV1hg4y1U7FG 

</div>

# Content

- [Features](#features)
- [Supported version](#supported-version)
- [Install](#install)
  - [Modules](#modules)
- [Quick Start](#quick-start)
    - [Create a Dialogue Graph](#create-a-dialogue-graph)
    - [Add Custom Actions](#add-custom-actions)
    - [AI Bake Dialogue](#ai-bake-dialogue)
    - [AI Generate Novel](#ai-generate-novel)
- [Nodes](#nodes)
- [Modules](#modules-1)
  - [General Modules](#general-modules)
  - [AIGC Modules](#aigc-modules)
  - [Editor Modules](#editor-modules)
    - [One-click Translation](#one-click-translation)
- [Extensions](#extensions)
  - [Localization Extension](#localization-extension)
  - [VITS Speech Extension](#vits-speech-extension)
    - [Bake Voice](#bake-voice)
- [Resolvers](#resolvers)
  - [How to Switch Resolver](#how-to-switch-resolver)
- [Create Dialogue in Script](#create-dialogue-in-script)


# Features

- Visual dialogue editor
- Modular dialogue function
- AIGC dialogue
- Custom actions support

# Supported version

* Unity 2022.3 or Later

# Install

Add following dependencies to `manifest.json`.

```json
  "dependencies": {
    "com.kurisu.chris": "https://github.com/AkiKurisu/Chris.git",
    "com.kurisu.chris.gameplay": "https://github.com/AkiKurisu/Chris.Gameplay.git",
    "com.kurisu.ceres": "https://github.com/AkiKurisu/Ceres.git",
    "com.cysharp.unitask":"https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"
  }

```

Use git URL to download package by Unity Package Manager ```https://github.com/AkiKurisu/Next-Gen-Dialogue.git```.

## Modules

The experimental features of Next Gen Dialogue are placed in the Modules folder and will not be enabled without installing the corresponding dependencies. 
You can view the dependencies in the `README.md` document under its folder.

# Quick Start

If you are using this plugin for the first time, it is recommended to play the following example scenes first:

`` 1.Normal Usage.unity `` this scene contains the use of NextGenDialogueComponent and NextGenDialogueGraphAsset;

`` 2.Editor Bake Dialogue.unity``this scene contains the sample of baking conversation conversation in the use of AI dialogue Baker in Editor;

```3.Build Dialogue by Code.unity``` this scene contains the use of Code to generate dialogue.

```4.Bake Novel.unity``` An example of using ChatGPT to infinitely generate dialogue trees.

### Create a Dialogue Graph

`NextGenDialogueComponent` and `NextGenDialogueGraphAsset` are used to store dialogue data. In order to facilitate understanding, it is collectively referred to as dialogue tree.
The following process is to create a dialogue tree that contains only a single dialogue and a single option:

1. Mount `NextGenDialogueComponent` on any gameObject
2. Click ``Open Dialogue Graph`` to enter the editor
3. Create the Container/dialogue node, the node is the dialogue container used in the game
4. Connect the Parent port of the dialogue Node to the root node. You can have multiple dialogue in one dialogueTree, but only those connected to the root node will be used.
5. Create the Container/Piece node and create our first dialogue fragment
6. Right -click Piece node ``Add Module`` add ``Content Module``, you can fill in the contents of the conversation in ``Content``
7. Create a Container/Option node and create a dialogue option corresponding to the PIECE node
8. Right-click Piece node ``Add Option``, connect Piece with Option
9. <b style = "color:#ee819e"> Very important: </b> At least one Piece node needs to be added to the Dialogue as the first piece of the dialogue. 
    You can click ``Collect All Pieces`` in context menu to collect all pieces in the Graph to the dialogue and adjust priority of them.
    * For priority, please refer to [General Module-Condition Module](#general-modules)

    <Img src = "Documentation~/Images/CreateDialogue.png">
  
10. Click on the upper left of the editor's `` Save`` to save dialogue
11. Click Play to enter PlayMode
12. Click on NextGenDialogueComponent ``Play dialogue`` to play conversation
13. Click `` Open Dialogue Graph `` to enter the debug mode
    
<IMG SRC = "Documentation~/Images/RuntimeDebug.png">

> The playing dialogue piece will be displayed in green

### Add Custom Actions

From V2, Next Gen Dialogue now use `Ceres.Flow` to implement custom action feature.

You can now add `ExecuteFlowModule` to fire a flow execution event at runtime.

For more details about `Ceres.Flow`, please refer to [AkiKurisu/Ceres](https://github.com/AkiKurisu/Ceres).

![Execute Flow](./Documentation~/Images/execute_flow.png)

### AI Bake Dialogue

You can use AI dialogue Baker to bake the dialogue content generated by AI in advance when designing the dialogue tree, so as to improve the workflow efficiency without affecting your design framework.

<img src="Documentation~/Images/BakeDialogue.png">

1. The basic dialogue graph design is consistent with the process of [Create a Dialogue Graph](#create-a-dialogue-graph)
2. Add ```AI Bake Module``` for the fragments or options that need to be baked, and remove the module for nodes that do not need to be baked
3. Select the type of LLM you are baking with
4. <b>Select in turn</b> the nodes that AI dialogue Baker needs to recognize, the order of recognition is based on the order selected by the mouse, and finally select the nodes that need to be baked
5. If the selection is successful, you can see the preview input content at the bottom of the editor
6. Click the ``Bake Dialogue`` button on the ````AI Bake Module```` and wait for the AI response
7. After the language model responds, a ```Content Module``` will be automatically added to the node to store the baked dialogue content
8. You can continuously generate conversations based on your needs

### AI Generate Novel

Different from talking directly to AI in baking dialogue, novel mode allows AI to play the role of copywriter and planner to write dialogue, so it can control options and fragments more precisely. Please refer to the example: ``4.Bake Novel.unity``

<img src="Documentation~/Images/BakeNovel.png" >

# Nodes

NGD use node based visual editor framework, most of the features are presented through nodes.
The construction dialogue are divided into the following parts in NGD:
  
| Name     | Description                                                                            |
| -------- | -------------------------------------------------------------------------------------- |
| Dialogue | Used to define dialogues, such as the first piece of the dialogue and other attributes |
| Piece    | dialogue piece, usually store the core dialogue content                                |
| Option   | dialogue options, usually used for interaction and bridging dialogues                  |


# Modules

In addition to the above nodes, a more flexible concept is used in NGD, that is, Module. 
You can use Module to change the output form of the dialogue, such as Google translation, localization, add callbacks, or be executed as a markup. 

## General Modules

The following are built-in general modules:

| Name             | Description                                                                                                                                                                                                                                                                                                                            |
| -----------------| -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Content          | Provide text content for Option or Piece                                                                                                                                                                                                                                                                                               |
| TargetID         | Add jumping target dialogue fragments for Option                                                                                                                                                                                                                                                                                       |
| PreUpdate        | Add pre-update behavior for Container, it will update when jumping to the Container                                                                                                                                                                                                                                                    |
| Callback         | Add callback behavior for Option, they will be updated after selection                                                                                                                                                                                                                                                                 |
| Condition        | Add judgment behavior for Option or Piece, it will be updated when jumping to the Container, if the return value is ``Status.Failure``, the Container will be discarded. If it is the first Piece of the dialogue, the system will try to jump to the next Piece according to the order in which the Pieces are placed in the dialogue |
| Next Piece       | Add the next dialogue segment after the end of the Piece. If there is no option, it will jump to the specified dialogue segment after playing the content of the Piece                                                                                                                                                                 |
| Google Translate | Use Google Translate to translate the content of current Option or Piece                                                                                                                                                                                                                                                               |

## AIGC Modules

The following are the built-in AIGC modules:

| Name                      | Description                                                                  |
| ------------------------- | ---------------------------------------------------------------------------- |
| Prompt                    | Prompt words that provide the basis for subsequent dialogue generation       |

## Editor Modules

Editor modules are used to provide some useful tools for the editor, such as translation.

### One-click Translation

Add Editor/EditorTranslateModule in the Dialogue node, set the source language (`sourceLanguageCode`) and target language (`targetLanguageCode`) of the translation, right-click and select `Translate All Contents` to perform all Piece and Option with `ContentModule` translate.

<img src="Documentation~/Images/FastTranslation.png" >

For nodes other than `ContentModule`, if the `TranslateEntryAttribute` is added to the field, you can right-click a single node to translate it.

```c#
public class ExampleAction : Action
{
    //Notify field can be translated
    //* Only work for SharedString and string
    [SerializeField, Multiline, TranslateEntry]
    private SharedString value;
}
```
<img src="Documentation~/Images/SingleTranslate.png" >

# Extensions

The following are extensions, you need to install the corresponding Package or configure the corresponding environment before use:

## Localization Extension

   Based on the [UnityEngine.Localization](https://docs.unity3d.com/Packages/com.unity.localization@1.4/manual/Installation.html) plugin to support the localization of dialogue

| Name              | Description                                                              |
| ------------------| ------------------------------------------------------------------------ |
| Localized Content | Provide content for Option or Piece after getting text from localization |

![Localization](./Documentation~/Images/localization_support.png)

## VITS Speech Extension

For VITS local deployment, please refer to this repository: [VITS Simple API](https://github.com/Artrajz/vits-simple-api)

If you want to use the VITS module, please use it with VITSAIReResolver. For the use of the Resolver, please refer to the following [Resolver](#Resolver)

| Name        | Description                                                                           |
| ----------- | ------------------------------------------------------------------------------------- |
| VITS Voice  | Use VITS speech synthesis model to generate language for Piece or Option in real time |

### Bake Voice

Before use, you need to install the corresponding dependencies of `Modules/VITS` and open the local VITS server (refer to `Modules/VITS/README.md`). Add `AIGC/VITSModule` to the node where speech needs to be generated, right-click and select ``Bake Audio ``

<img src="Documentation~/Images/BakeAudio.png" >

If you are satisfied with the generated audio, click `Download` to save it locally to complete the baking, otherwise the audio file will not be retained after exiting the editor.

It is no longer necessary to start the VITS server at runtime after baking is complete.

* If the AudioClip field is empty, the run generation mode is enabled by default. If there is no connection, the conversation may not proceed. If you only need to use the baking function, please keep the AudioClip field not empty at all times.

# Resolvers
Resolver is used to detect the Module in the Container at runtime and execute a series of preset logic such as injecting dependencies and executing behaviors, the difference between NGD's built-in Resolver is as follows:

| Name                                  | Description                                                                                                         |
| ------------------------------------- | ------------------------------------------------------------------------------------------------------------------- |
| Default Resolver                      | The most basic resolver, supporting all built-in common modules                                                     |
| VITS Resolver                         | Additionally detect VITS modules to generate voice in real time                                                     |

## How to Switch Resolver

1. In-scene Global Resolver
    You can mount the ```VITSSetup``` script on any GameObject to enable AIResolver in the scene

2. Dialogue specified Resolver
   
    You can add  ```VITSResolverModule``` to the dialogue node to specify the resolver used by the dialogue, and you can also click the Setting button in the upper right corner of the module and select which Resolvers to be replaced in ``Advanced Settings``


# Create Dialogue in Script

NGD is divided into two parts, DialogueSystem and DialogueGraph. 
The former defines the data structure of the dialogue, which is interpreted by resolver after receiving the data. 
The latter provides a visual scripting solution and inherits the interface from the former. 
So you can also use scripts to write dialogues, examples are as follows:

```C#
using UnityEngine;
public class CodeDialogueBuilder : MonoBehaviour
{
    private RuntimeDialogueBuilder _builder;
    
    private void Start()
    {
        PlayDialogue();
    }
    
    private void PlayDialogue()
    {
        var dialogueSystem = DialogueSystem.Get();
        _builder = new RuntimeDialogueBuilder();
        // First Piece
        _builder.AddPiece(GetFirstPiece());
        // Second Piece
        _builder.AddPiece(GetSecondPiece());
        dialogueSystem.StartDialogue(_builder);
    }
    
    private static Piece GetFirstPiece()
    {
        var piece = Piece.GetPooled();
        piece.AddContent("This is the first dialogue piece");
        piece.ID = "01";
        piece.AddOption(new Option
        {
            Content = "Jump to Next",
            TargetID = "02"
        });
        return piece;
    }
    
    private static Piece GetSecondPiece()
    {
        var piece = Piece.GetPooled();
        piece.AddContent("This is the second dialogue piece");
        piece.ID = "02";
        piece.AddOption(GetFirstOption());
        piece.AddOption(GetSecondOption());
        return piece;
    }
    
    private static Option GetFirstOption()
    {
        var callBackOption = Option.GetPooled();
        // Add CallBack Module
        callBackOption.AddModule(new CallBackModule(() => Debug.Log("Hello World!")));
        callBackOption.Content = "Log";
        return callBackOption;
    }
    
    private static Option GetSecondOption()
    {
        var option = Option.GetPooled();
        option.Content = "Back To First";
        option.TargetID = "01";
        return option;
    }
}
```