# Oobabooga Extension Explanation Oobabooga拓展包说明
我发现交互式的对话生成已经非常成熟，例如Oobabooga Text-Generation-WebUI，它可以导出历史记录为json，我们可以先实时对话生成完历史记录后将其导入到对话树中。

I found that interactive dialogue generation is very mature, such as Oobabooga Text-Generation-WebUI, which can export history records as json. We can first generate real-time dialogue history records and then import them into the dialogue tree.
## Features 特点
导入Oobabooga的历史记录

Import Oobabooga's history

## How to use 使用
在Dialogue结点添加```Editor/Experimental/Novel/OobaboogaSessionModule```,右键视图点击```Load Oobabooga Session```,选择Text-Generation-WebUI的Logs文件夹中的历史记录进行导入

Add ```Editor/Experimental/Novel/OobaboogaSessionModule``` to the Dialogue node, right-click the view and click ```Load Oobabooga Session```, and select the history records in the Logs folder of Text-Generation-WebUI to import.