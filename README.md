# DialogueSystem

基于 Unity 的轻量对话系统示例。当前工程已经把“通用表系统”和“对话模块”拆开：

- `tabtoy` 导表和二进制表加载属于项目级基础设施
- `Dialogue` 只是使用这套表系统的一个业务模块

## 环境

- Unity `2022.3.62f1c1`
- UI: `UGUI`
- 文本组件: `TextMeshPro`
- 导表工具: `tabtoy`

## 当前结构

```text
Assets/
  Resources/
    LoadableAssets/Table/                # 导表产物（.bytes + tablelist.json）
    Perfab/DialogueUI.prefab             # 对话 UI 预制体
  Scripts/
    Core/
      Localization/
        LocalizationLanguage.cs          # 支持的语言枚举
        LocalizationLanguageExtensions.cs # 语言显示名和语言列映射
        LocalizationRepository.cs        # 本地化查询入口，按 Key + 当前语言返回文本或语音路径
        LocalizationSettings.cs          # 当前语言设置
      Table/
        TableDataMgr.cs                  # 表加载入口，读取 bytes 并调用 Deserialize/IndexData
        Table.Extensions.cs              # Table 的业务级 Normalize 逻辑
        Generated/
          table_gen.cs                   # tabtoy 生成的表结构、反序列化和索引代码
        Runtime/
          TableReader.cs                 # tabtoy 二进制读表运行时
    Modules/
      Dialogue/
        Dialogue.cs                      # 对话流程状态机，负责开始/下一句/选项跳转
        DialogueController.cs            # 对话系统入口，协调 UI、语言切换和语音播放
        DialogueRepository.cs            # 原始表查询层，只取 Dialogue/Select/Character 数据
        DialogueResolver.cs              # 结果组装层，把表数据解析成最终展示结果
        DialogueResult.cs                # 对话节点结果和选项结果结构
        DialogueUI.cs                    # 对话界面表现层，负责文本、头像、选项、下拉框、打字机
        DialogueVoicePlayer.cs           # 语音播放组件，负责 AudioSource 和 AudioClip 缓存
    Main.cs                              # 示例启动入口

Config/
  Table/
    xlsx/                                # 通用配表目录
      *.xlsx
      Make.bat                           # 导表入口脚本
    tabtoy.exe                           # 预编译 tabtoy，可被 Make.bat 直接调用
```

## 表设计

### DialogueData

只保存对话逻辑和引用。

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `ID` | `int` | 对话节点 ID |
| `SelectList` | `List<int>` | 选项 ID 列表 |
| `NextID` | `int` | 顺序下一节点，`0` 表示没有后续 |
| `SpeakerID` | `int` | 说话人角色 ID |
| `TextKey` | `string` | 正文本地化 Key |
| `VoiceKey` | `string` | 语音本地化 Key |

### SelectData

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `ID` | `int` | 选项 ID |
| `Goto` | `int` | 选择后跳转节点 |
| `TextKey` | `string` | 选项文本 Key |

### CharacterData

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `ID` | `int` | 角色 ID |
| `ImagePath` | `string` | 头像资源路径，填 `Resources` 相对路径且不带扩展名 |
| `NameKey` | `string` | 角色名 Key |

### LocalizationData

文本和语音统一放在一张表里，运行时通过 `Key` 查询。

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `ID` | `int` | 记录编号，不参与运行时索引 |
| `Key` | `string` | 本地化 Key，运行时主索引 |
| `ZhCN` | `string` | 简中内容 |
| `EnUS` | `string` | 英文内容 |

示例：

- `char.1.name`
- `dlg.101.text`
- `dlg.101.voice`

## 运行时链路

### 1. 表加载

`TableDataMgr.Init()` 会：

- 读取 `tablelist.json`
- 逐个加载 `.bytes`
- 调用生成代码里的 `Deserialize()`
- 调用生成代码里的 `IndexData()`
- 最后执行 `Normalize()`

基础索引已经交给 `tabtoy` 生成。  
`Normalize()` 当前额外构建了“对话节点 -> 选项数据列表”的业务缓存。

### 2. 对话逻辑

`Dialogue.cs` 负责：

- `StartDialogue`
- `NextDialogue`
- `ChoiceDialogue`
- `RefreshCurrentDialogue`

### 3. 数据组装

`DialogueRepository.cs` 只查原始表数据。  
`LocalizationRepository.cs` 只负责按 `Key + 当前语言` 解析值。  
`DialogueResolver.cs` 负责把：

- `DialogueData`
- `CharacterData`
- 本地化文本
- 本地化语音

组装成最终的 `DialogueResult`。

### 4. UI 与语音

`DialogueUI.cs` 负责：

- 正文显示
- 角色名显示
- 头像显示
- 选项显示
- 打字机效果
- 语言下拉框事件

`DialogueVoicePlayer.cs` 负责：

- `AudioClip` 缓存
- 当前节点语音播放/停止

## 头像资源规则

如果图片文件在：

```text
Assets/Resources/Art/knight-removebg-preview.png
```

那么 `Character.xlsx` 的 `ImagePath` 应填写：

```text
Art/knight-removebg-preview
```

不要填写：

- `Assets/Resources/...`
- `.png`
- 绝对路径

并且图片导入类型必须是 `Sprite (2D and UI)`。

## 打字机规则

`DialogueUI.cs` 使用 `TextMeshProUGUI.maxVisibleCharacters` 实现逐字显示。

当前交互：

- 节点进入后正文逐字显示
- 文本没打完时点击背景，会直接补完整句
- 文本打完后，无选项节点才允许点击进入下一句
- 有选项节点会等正文打完再显示选项

速度参数在 `DialogueUI.charactersPerSecond`。

## 导表

修改 `Config/Table/xlsx` 后执行：

```bat
Config\Table\xlsx\Make.bat
```

它会生成：

- `Assets/Scripts/Core/Table/Generated/table_gen.cs`
- `Assets/Resources/LoadableAssets/Table/*.bytes`
- `Assets/Resources/LoadableAssets/Table/tablelist.json`

建议在终端里执行，便于直接看错误：

```bat
cd /d D:\unitycode2\Dialogue
Config\Table\xlsx\Make.bat
```

## 示例入口

当前示例入口在 `Assets/Scripts/Main.cs`，启动时会：

- 初始化表
- 设置默认语言
- 初始化 `DialogueController`
- 从节点 `101` 开始展示

## 当前方向

- 通用表系统已从对话模块中拆出
- 文本和语音统一使用 `LocalizationData`
- 表加载已切到 `her_simple` 风格的二进制加载模式
- 基础索引在生成层，业务关系在 `Normalize()`

## 后续可扩展

- 增加更多语言列
- 增加自动播放/历史记录
- 增加角色表情、立绘状态
- 从 `Resources` 迁移到 `Addressables`
- 为更多业务模块复用同一套配表系统
