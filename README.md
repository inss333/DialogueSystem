# DialogueSystem

一个基于 Unity 的轻量级对话系统示例项目。

这个项目使用 `DialogueData` 和 `SelectData` 两张表来描述对话节点、分支选择和跳转关系，运行时通过 `Resources` 加载 JSON 数据，并使用统一的 `DialogueUI` 预制体进行渲染，适合用来快速搭建剧情对话、NPC 对话或简单的分支叙事系统。

## 项目特性

- 基于配置表驱动对话内容
- 支持顺序对话，使用 `NextID` 串联节点
- 支持分支选项，使用 `SelectList` 跳转不同节点
- 支持打字机效果
- 启动时自动校验表数据引用关系
- 选项按钮支持按数量动态扩展
- 自带可运行示例场景

## 运行环境

- Unity `2022.3.62f1c1`
- UI: `UGUI`
- 文本组件: `TextMeshPro`
- 表工具: `tabtoy`

## 目录结构

```text
Assets/
  Resources/
    LoadableAssets/Table/      # 导出的 JSON 配置表
    Perfab/DialogueUI.prefab   # 对话 UI 预制体
  Scripts/
    Dialogue/
      Dialogue.cs
      DialogueController.cs
      DialogueRepository.cs
      DialogueResult.cs
      DialogueUI.cs
      DialogueValidator.cs
    Generated/
      table_gen.cs             # 自动生成的表结构代码
    Main.cs                    # 示例入口
    TableDataMgr.cs            # 表加载与索引构建
  Scenes/
    SampleScene.unity

Config/
  xlsx/
    Dialogue.xlsx
    Select.xlsx
    Index.xlsx
    Make.bat                   # 导表脚本
```

## 核心流程

### 1. 表数据加载

启动时先调用 `TableDataMgr.Init()`，它会读取：

- `Assets/Resources/LoadableAssets/Table/tablelist.json`
- `Assets/Resources/LoadableAssets/Table/DialogueData.json`
- `Assets/Resources/LoadableAssets/Table/SelectData.json`

加载完成后，会自动构建以下字典索引，方便运行时快速查找：

- `DialogueDataByID`
- `SelectDataByID`

## 2. 对话运行逻辑

运行时状态由 [Dialogue.cs](/d:/unitycode2/Dialogue/Assets/Scripts/Dialogue/Dialogue.cs) 管理，主要接口包括：

- `StartDialogue(int dialogueID)`：开始对话
- `NextDialogue()`：跳转到下一个顺序节点
- `ChoiceDialogue(int dialogueID)`：跳转到选择分支目标节点
- `EndDialogue()`：结束当前对话

每次切换节点后，系统都会生成一个 `DialogueResult`，其中包含：

- 当前节点数据
- 当前节点可选项列表
- 当前节点是否还能继续
- 当前节点是否已经结束

## 3. UI 控制流程

[DialogueController.cs](/d:/unitycode2/Dialogue/Assets/Scripts/Dialogue/DialogueController.cs) 负责：

- 初始化数据仓库
- 执行对话表校验
- 从 `Resources/Perfab/DialogueUI` 加载 UI 预制体
- 绑定下一句事件和选项点击事件
- 把当前节点渲染到界面上

[DialogueUI.cs](/d:/unitycode2/Dialogue/Assets/Scripts/Dialogue/DialogueUI.cs) 负责：

- 显示当前对话文本
- 根据选项数量显示或隐藏按钮
- 当选项数量超过初始模板数量时，自动克隆按钮

## 数据表结构

### DialogueData

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `ID` | `int` | 对话节点 ID |
| `Name` | `string` | 说话人名称 |
| `Text` | `string` | 对话内容 |
| `SelectList` | `List<int>` | 当前节点关联的选项 ID 列表 |
| `NextID` | `int` | 下一个顺序节点 ID，`0` 表示没有后续节点 |

### SelectData

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `ID` | `int` | 选项 ID |
| `Text` | `string` | 选项文本 |
| `Goto` | `int` | 选择后跳转到的目标对话节点 ID |

## 示例入口

当前示例启动逻辑位于 [Main.cs](/d:/unitycode2/Dialogue/Assets/Scripts/Main.cs)：

```csharp
if (!TableDataMgr.Init())
{
    return;
}

var controller = gameObject.AddComponent<DialogueController>().Init();
if (!controller)
{
    Debug.LogError("DialogueController init failed.");
    return;
}

controller.RenderNode(101);
```

这意味着示例场景启动后会：

1. 初始化表数据
2. 创建并初始化 `DialogueController`
3. 从对话节点 `101` 开始播放

## 使用方法

### 直接运行示例

1. 用 Unity 打开项目
2. 打开 `Assets/Scenes/SampleScene.unity`
3. 进入 Play 模式
4. 系统会自动从 `101` 节点开始显示对话

### 接入到你自己的场景

1. 先调用 `TableDataMgr.Init()`
2. 创建或挂载 `DialogueController`
3. 调用 `Init()`
4. 调用 `RenderNode(startID)` 开始对话

示例：

```csharp
using D1;
using UnityEngine;

public class DialogueEntry : MonoBehaviour
{
    private void Start()
    {
        if (!TableDataMgr.Init())
        {
            return;
        }

        var controller = gameObject.AddComponent<DialogueController>().Init();
        if (controller != null)
        {
            controller.RenderNode(101);
        }
    }
}
```

## 如何导表

编辑 `Config/xlsx` 目录下的 Excel 文件后，执行：

```bat
Config\xlsx\Make.bat
```

这个脚本会自动完成以下工作：

- 生成 `Assets/Scripts/Generated/table_gen.cs`
- 导出 JSON 到 `Assets/Resources/LoadableAssets/Table`
- 重建 `tablelist.json`

## 当前实现说明

基于当前代码，这个项目已经实现了对话主流程，但还有一些地方是保留扩展位：

- `DialogueData.Name` 已经存在，但当前 UI 里还没有显示说话人名字
- 当节点既没有 `NextID` 也没有选项时，系统会自动隐藏对话 UI，作为结束处理
- UI 预制体路径目前写死为 `Resources/Perfab/DialogueUI`

## 适合继续扩展的方向

- 增加角色名显示
- 增加立绘和背景切换
- 增加对话历史记录
- 增加节点事件回调
- 增加变量条件判断与分支控制
- 增加本地化支持
- 把 `Resources` 加载方式替换成 Addressables

## 主要脚本说明

- [Dialogue.cs](/d:/unitycode2/Dialogue/Assets/Scripts/Dialogue/Dialogue.cs)：对话状态与节点跳转
- [DialogueController.cs](/d:/unitycode2/Dialogue/Assets/Scripts/Dialogue/DialogueController.cs)：初始化、UI 生命周期与节点渲染
- [DialogueUI.cs](/d:/unitycode2/Dialogue/Assets/Scripts/Dialogue/DialogueUI.cs)：对话面板和选项按钮控制
- [DialogueRepository.cs](/d:/unitycode2/Dialogue/Assets/Scripts/Dialogue/DialogueRepository.cs)：配置表查询封装
- [DialogueValidator.cs](/d:/unitycode2/Dialogue/Assets/Scripts/Dialogue/DialogueValidator.cs)：配置合法性校验
- [TableDataMgr.cs](/d:/unitycode2/Dialogue/Assets/Scripts/TableDataMgr.cs)：表加载与索引构建

## License

谢谢大家。
