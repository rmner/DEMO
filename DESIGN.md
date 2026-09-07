

---

## 1. 设计模式（本项目用的 6 种）

| 模式 | 用在哪 | 为什么用 | 代价/注意 |
|---|---|---|---|
| **单例 Singleton** | GameManager、PauseManager | 全局唯一状态（分数/暂停），还能**跨场景共享** | 全局可访问=隐蔽依赖；别滥用，只给跨场景必需的状态用 |
| **静态工具类** | SaveManager | 纯序列化工具，不需要分量/生命周期 | 无实例、难 mock；适合无状态工具 |
| **控制器 Controller** | Player/Enemy/Coin/Hazard/Goal/Camera | 每个 GameObject 一个脚本管自己的行为（单一职责） | 太多小脚本时需注意命名组织 |
| **场景即状态** | MainMenu/L1/L2/Victory | 1 个场景=1 个画面状态，Unity 原生、可视化编辑 | 切换用 SceneManager，需在 Build Settings 登记 |
| **事件驱动** | GameManager 订阅 `sceneLoaded` | 跨场景刷新"玩家/出生点"引用，和场景生命周期解耦 | 记得 OnDisable 退订，防止泄漏 |
| **数据驱动** | DemoSceneBuilder、SaveData | 场景/存档都由"数据清单"生成，改布局=改数据，不重写逻辑 | 数据要集中、有命名规范 |

---

### 2.1 Managers

**GameManager.cs —— 全局单例**
- `static GameManager I`：全局唯一入口。
- `Awake()`：`if (I != null && I != this) Destroy(this);` 去重；`I = this; DontDestroyOnLoad(gameObject);`——**跨场景不销毁**。
- `OnEnable/OnDisable`：订阅/退订 `SceneManager.sceneLoaded`。为什么不用 `Start`？因为 `DontDestroyOnLoad` 的对象在后续场景加载时 **Start 不会重跑**，所以用事件在每次进新场景时刷新 `player`/`spawn` 引用（`RefreshRefs`）。
- `AddScore(v)`：加分，并顺手更新 `bestScore` 存档。
- `Kill()`：显示死亡提示 + `Invoke("Respawn", 0.5f)` 延迟重生；`Respawn` 把玩家挪回 `PlayerSpawn` 并清零速度。
- `LevelComplete(nextScene, levelIndex)`：① 更新 `unlockedLevel` 存档 ② 更新最高分 ③ 恢复时间 ④ `LoadScene(nextScene)`。
- `Update()`：按 R 重置本局分数并重生。

**SaveManager.cs —— 静态存档工具**
- `SaveData`（可序列化）：`unlockedLevel`（0=第1关，1=解锁第2关，2=全部）、`bestScore`。
- `Save/Load/ResetAll`：`PlayerPrefs` + `JsonUtility`。`PlayerPrefs` 是本地键值存储（**换设备不同步**；做联机/云存档需换方案）。

**PauseManager.cs —— 暂停单例**
- 同单例 + `DontDestroyOnLoad`。
- `Update()`：如果当前场景是 `MainMenu`/`Victory` 就不响应 Esc；否则按 Esc 切换 `isPaused`。
- `Toggle()`：切 `isPaused`，`Time.timeScale = 0/1`。**用 timeScale 冻结整个游戏时间**（简单、全局），代价是协程/动画也要停，需注意。
- `Resume()`、`ReturnToMenu()`（恢复时间 + 回 MainMenu）。

### 2.2 UI

**MainMenu.cs —— 开始界面**
- `OnGUI()` 画标题、最高分、按钮。按钮根据 `unlockedLevel` 动态显示"新游戏/继续第二关/终章"。
- `StartScene(name)`：`GameManager.ResetRun()` 后 `LoadScene`。

**HUD.cs —— 游戏内 HUD**
- 显示分数；暂停时画覆盖层（继续 / 返回主菜单），并 `return` 不再画操作提示。
- 只读 `GameManager/PauseManager` 数据，不含逻辑（符合"UI 只显示"）。

**VictoryUI.cs —— 通关界面**
- 画恭喜 + 最高分；按钮：返回主菜单 / 重新开始。进入时就 `Resume()` 确保时间恢复正常。

### 2.3 World

**PlayerController.cs —— 手感核心**
- 移动：`Mathf.MoveTowards` 让速度朝目标速度逼近（地面加速/减速/摩擦、空中系数）。
- 跳跃：土狼时间（coyote）、跳跃缓冲（buffer）、可变跳高（松键压短）；`maxJumps=2` 实现**二段跳**。
- 蹬墙跳：空中 `touchingWall` + 按跳 → 反方向弹开，用 `lockTimer` 短暂锁定方向输入防止被抵消；`WallSlide` 贴墙减速。
- 检测：`OverlapCircleAll` 只认 `attachedRigidbody == null` 的**静态**碰撞体（地面/平台/墙），这样玩家自己的刚体、敌人、金币都不会被误判成地面/墙。
- 特效：`UpdateDust` 只在"地面移动"时播尘土。

**Enemy.cs**：巡逻往返、按方向 `flipX`、`OnCollisionEnter2D` 碰到玩家 → `GameManager.Kill()`。
**Collectible.cs**：旋转、`OnTriggerEnter2D` 碰到玩家 → 加分并销毁。
**Hazard.cs**（尖刺）：`OnCollisionEnter2D` 碰到玩家 → 死亡。
**Goal.cs**：`nextScene`/`levelIndex` 由生成器配置；`OnTriggerEnter2D` → `GameManager.LevelComplete()`。
**CameraFollow.cs**：`LateUpdate` 用 `Lerp` 平滑跟随玩家。

### 2.4 Editor

**DemoSceneBuilder.cs —— 一键生成器**
- `Tools → Metroidvania Demo → Build All Scenes`：依次生成 **MainMenu / Level1 / Level2 / Victory** 四个场景，并写入 `EditorBuildSettings.scenes`。
- 做法：`data-driven`，世界布局就是 `Solid/AddCoin/AddEnemy/AddSpike/AddGoal` 的坐标清单——**加平台、加金币就是加一行**，不碰逻辑。
- 辅助：`PrepareSprites`（把贴图导入成 Sprite、PPU=32）、`EnsureTag`（加自定义 Tag）、`EnsureManagers`（每场景放单例，运行时靠单例去重）。

---

## 3. 运行流程（数据流）

```
MainMenu
  └ 新游戏 ──► Level1 ──(旗子)──► Level2 ──(旗子)──► Victory
                 │                    │                 └ 返回主菜单 / 重开
                 └─ Esc ═► 暂停覆盖层（继续 / 返回主菜单）
                 └─ R ═► 重生
每次 LevelComplete：SaveManager 记下 unlockedLevel + bestScore → PlayerPrefs
```

输入 → Controller（玩家/敌人/金币）→ GameManager（计分/死亡/通关）→ SaveManager（持久化）→ HUD（显示）。

---

---

## v2 更新（8 关卡 + 解密 + 居中 UI）

**本版改动：**
1. **8 个关卡**：`BuildLevel(idx=1..8)` 由**程序化生成器** `BuildWorld(idx)` 创建，平台/金币/敌人随关卡递增难度；第 4 关起加入蹬墙跳竖井；每关宽度递增。生成 10 个场景并写入 Build Settings。
2. **解密环节（每关都有）**：
   - `Gate.cs`：带 BoxCollider2D 的闸门挡在终点前；`required` = 需要踩到的开关数；集齐后 `Open()` 禁用碰撞体。
   - `SwitchPlate.cs`：实心开关台，玩家踩上去（`OnCollisionEnter2D`）触发 `gate.Trigger()`，变绿。
   - 玩法：走到终点被闸门挡住 → 回头踩亮旁边的绿色开关（第 3 关起要踩 2 个）→ 闸门打开 → 到终点。这就是"找开关 → 开门"的解密循环。
3. **界面居中**：
   - `UiHelper.cs`：`Dim()` 画半透明黑幕、`Centered()` 把内容垂直+水平居中。
   - MainMenu / Victory / HUD 的暂停覆盖层都居中；游戏内分数保持左上角（避免挡住操作）。

**新增脚本：** Gate、SwitchPlate、UiHelper。
**通用解谜数据驱动：** 加"开关开门"就是 `AddGate(pos, required)` + `AddSwitch(pos, gate)` 各一行。

---

## v3 —— 扩展到 100 关 + 新机制（运行时生成 / 点击才加载）

> 复用 v2 的解谜+居中 UI，但把"逐关造场景"升级成"**1 个关卡场景 + 运行时按关卡号生成**"，这样才能真正撑到 100 关，也符合"点击时才加载这一关"的省性能思路。

**架构变化：**
- 场景从 10 个精简为 **3 个**：`MainMenu / Level / Victory`。
- `Level.unity` 只放一个 `LevelRuntimeBuilder`；它按 `GameManager.I.currentLevel`（1..100）在 `Awake` 里动态搭建整关，素材 Sprite 由编辑器 `DemoSceneBuilder` 序列化填进去。
- 通关推进：`Goal` → `GameManager.LevelComplete(levelIndex)` → 保存解锁/最高分 → `LoadScene("Level")`（下一关）或 `Victory`（第 100 关）。

**随关卡递增、逐步解锁的机制（`LevelRuntimeBuilder` 按 `idx` 阈值开放）：**

| 关卡区间 | 新机制 |
|---|---|
| 第 1 关起 | 基础平台 + 尖刺 + 巡逻怪 + 金币 |
| 第 12 关起 | **跳跃怪**（怪物会原地小跳，扑向玩家） |
| 第 16 关起 | **垂直移动平台**（电梯/跳楼机小球台，玩家站上去会被带着走） |
| 第 21 关起 | **横向移动平台** |
| 第 26 关起 | **跳楼机下压墙**（`Crusher`：升得慢、落得快、底部停留，要掐时机穿越） |
| 第 30 关起、每 4 关 | **追逐战**（`Chaser` 红色死亡之墙从后匀速推进，必须一路往右跑/冲刺） |

**新增脚本：** `LevelRuntimeBuilder`、`MovingPlatform`、`Crusher`、`Chaser`。
**冲刺（始终可用）：** `PlayerController` 加 `Shift`(或 `L`) 冲刺 —— 水平高速滑行 + 短暂失重 + **冲刺无敌帧**（`Invincible`），可穿过尖刺/怪物/下压墙/追兵，用来过大缺口。

**注意：** 100 关里不少是靠公式生成，个别关可能出现难跳、不流畅，需要按手感微调（这是程序化生成的固有代价，也意味着随便哪关都不同）。


