
一个用 C# 写在团结引擎（Tuanjie，Unity 2022.3 内核）上的 2D 横向平台跳跃小游戏。一张一张的关卡，捡金币、买能力、往后打。


## 跑起来

- 引擎版本：**团结引擎*（基于 Unity 2022.3）。用 Tuanjie Hub 打开就能编译。
- 打开后，顶部菜单跑一次：
  `Tools → Metroidvania Demo → Build All Scenes`
  （这一步生成主菜单、商城、关卡、结算场景，写进 Build Settings，并生成一个默认的商城目录。）
- 打开 `MainMenu` 场景，点 Play。

**操作**：`A/D` 移动，`空格` 跳，`Shift`（或 `L`）冲刺，`R` 重生，`Esc` 暂停。

## 里面有什么

- 平台跳跃的一套手感：土狼时间、跳跃缓冲、可变跳跃高度、二段跳、蹬墙跳、爬墙、冲刺（冲刺带短暂无敌帧，能直接穿尖刺和怪）。
- **100 关**，难度逐关往上：第 12 关起有会蹦的怪，16 关有移动平台，26 关有像跳楼机一样的下压墙，30 关起隔几关会有一个追兵在后面撵你。
- 吃金币，到主菜单的商城买东西：**皮肤**（换角色颜色）和**能力**（冲刺 / 二段跳 / 爬墙）。这三个能力**没买之前是锁着的**，HUD 上会标 `✓/✗`。

## 代码结构

```
Assets/Scripts/
  Player/    玩家移动（手感、冲刺、能力解锁）
  World/     敌人、金币、尖刺、终点、移动平台、下压墙、追兵
  Managers/  GameManager（单例+场景流+存档）、SaveManager、PauseManager
  UI/        UiHelper（一套 IMGUI 主题）、主菜单、HUD、商城、结算
  Level/     LevelRuntimeBuilder：按关卡号在运行时生成关卡
Assets/Editor/
  DemoSceneBuilder      生成场景
  LevelGeneratorWindow  手摆关卡用
```

## 两个我比较想说的点

**手摆 + 程序生成混着来。** 平时走 `LevelRuntimeBuilder` 按公式生成关卡；但某一关你一旦用 `Tools → 关卡生成器` 生成了 `LevelN.unity`，流程（`GameManager.SceneFor`）就会去加载这一关，不再重新生成。这样通用关省事，关键关能一个物体一个物体地调。`Build All Scenes` 也不会清掉这些手摆的场景。

**能力解锁反过来影响关卡。** 冲刺、二段跳、爬墙是靠平时捡的金币在商城买的，没买就是封锁的。所以早期的关卡得保证"单跳"就能过，买到能力之后再去碰需要这些能力的关。顺带把程序生成的平台高度压到了单跳能到的范围。

## 目前的状态

- UI 是用 `OnGUI`（IMGUI）写的，不是 UGUI / UI Toolkit。能用，但只能说"功能齐"，谈不上精致。
- 存档用 `PlayerPrefs` + `JsonUtility`，单设备、没加密。
- 100 关大部分是程序生成，个别关我没细调，可能有一两处跳不过去。手摆功能就是用来补这个的。
- 代码用 Roslyn 对着 Tuanjie **编译通过（无错误）**；运行时我在编辑器里点过主要流程，但没有做完整回归测试。



