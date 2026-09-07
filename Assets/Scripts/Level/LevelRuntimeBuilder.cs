using UnityEngine;

// 运行时按 GameManager.currentLevel（1..100）动态生成一关。
// 这样不用堆 100 个场景文件，也符合"点击时才加载这一关"的省性能想法。
// 由编辑器 DemoSceneBuilder 把素材 Sprite 填进本组件的字段里。
public class LevelRuntimeBuilder : MonoBehaviour
{
    [Header("素材（由编辑器 Build All Scenes 时填充）")]
    public Sprite spritePlayer;
    public Sprite spriteGround;
    public Sprite spritePlatform;
    public Sprite spriteSpike;
    public Sprite spriteCoin;
    public Sprite spriteEnemy;
    public Sprite spriteBackground;
    public Sprite spriteGoal;
    public Sprite spriteGate;
    public Sprite spriteSwitch;

    [Header("调试")]
    [Tooltip(">0 时直接从这一关开始（在编辑器里测试任意一关用）；为 0 则用 GameManager.currentLevel")]
    public int debugLevel;

    [Tooltip("勾选后运行时不再自动生成关卡，改用场景里已生成、可手摆的物体（关卡生成器会替你勾上）")]
    public bool handMode;

    int idx;

    void Awake()
    {
        // 直接 Play 本场景时管理器可能不存在，补上
        if (GameManager.I == null) new GameObject("GameManager").AddComponent<GameManager>();
        if (PauseManager.I == null) new GameObject("PauseManager").AddComponent<PauseManager>();
        new GameObject("HUD").AddComponent<HUD>();

        idx = debugLevel > 0 ? debugLevel
             : (GameManager.I != null ? Mathf.Max(1, GameManager.I.currentLevel) : 1);
        if (GameManager.I != null) GameManager.I.currentLevel = idx;   // 让过关后从 debugLevel 往后接着走

        if (!handMode) BuildLevel(idx);   // 手摆关卡：物体已经在场景里，跳过重建
    }

    // 供编辑器"关卡生成器"在编辑模式把当前 debugLevel 的关卡物体生成进场景（可拖动、可保存）
    public void PopulateInEditor()
    {
        int i = Mathf.Clamp(debugLevel > 0 ? debugLevel : 1, 1, 100);
        BuildLevel(i);
    }

    void Start()
    {
        if (IsChase(idx) && GameManager.I != null)
            GameManager.I.message = "快跑！后面有追兵，别停下！";
    }

    static bool IsChase(int i) => i >= 30 && i % 4 == 0;   // 第30关起，每4关一次追逐战

    // ---------- 关卡骨架 ----------
    void BuildLevel(int i)
    {
        bool chase = IsChase(i);
        float w = chase ? 60f : 32f + i * 0.35f;
        float goalX = w - 5f;

        var camGO = CameraAndBg(w, chase);
        var player = CreatePlayer(new Vector3(2, 1.2f, 0));
        camGO.GetComponent<CameraFollow>().target = player.transform;

        var spawn = new GameObject("PlayerSpawn");
        spawn.transform.position = player.transform.position;

        Solid("Ground", "ground", new Vector3(w / 2f, 0, 0), new Vector2(w, 1));
        Solid("WallL", "ground", new Vector3(0.5f, 4, 0), new Vector2(1, 12));
        Solid("WallR", "ground", new Vector3(w - 0.5f, 4, 0), new Vector2(1, 12));

        if (chase) BuildChase(i, w, goalX);
        else BuildPlatform(i, w, goalX);
    }

    // ---------- 普通关：平台 + 尖刺 + 怪物 + 移动平台 + 下压墙 + 开关闸门 ----------
    void BuildPlatform(int i, float w, float goalX)
    {
        // 错落平台（数量随关卡递增；后期出现需冲刺/大跳的宽缺口）
        float px = 4f, py = 1.8f;
        int n = 7 + i / 3;
        for (int k = 0; k < n; k++)
        {
            bool dashGap = i >= 10 && k % 5 == 0;
            float gap = dashGap ? 5.4f : 3.0f;
            Solid("P" + k, "platform", new Vector3(px, py, 0), new Vector2(2.4f, 0.4f));
            if (k % 2 == 0) AddCoin(new Vector3(px, py + 0.9f, 0));
            if (k >= 2 && k % 3 == 0) AddEnemy(new Vector3(px, py + 0.7f, 0), i);
            px += gap;
            py = 1.4f + (k % 3) * 0.9f;   // 最高约3.2，单跳可达（能力没解锁也能过）
            if (px > goalX - 6f) break;
        }

        // 尖刺（密度随关递增）
        int spikeRows = 1 + i / 40;
        for (int s = 0; s < spikeRows; s++)
        {
            float sx = w * (0.42f + 0.07f * s);
            AddSpike(new Vector3(sx, 0.5f, 0));
            AddSpike(new Vector3(sx + 1, 0.5f, 0));
        }

        // 垂直移动平台（电梯）：第16关起
        if (i >= 16)
        {
            int vCount = Mathf.Clamp((i - 15) / 6, 1, 6);
            for (int v = 0; v < vCount; v++)
            {
                float mx = 14f + v * (goalX - 24f) / Mathf.Max(1, vCount - 1);
                AddMovingPlatform(new Vector3(mx, 2f, 0), new Vector2(2.2f, 0.4f),
                                  new Vector2(0, 3.5f + v * 0.4f), 2f + v * 0.15f);
            }
        }

        // 横向移动平台：第21关起
        if (i >= 21)
        {
            int hCount = Mathf.Clamp((i - 20) / 8, 1, 5);
            for (int h = 0; h < hCount; h++)
            {
                float mx = 18f + h * 5f;
                AddMovingPlatform(new Vector3(mx, 3.6f + (h % 2) * 1.2f, 0),
                                  new Vector2(2f, 0.4f), new Vector2(4f, 0), 2.2f);
            }
        }

        // 跳楼机下压墙：第26关起
        if (i >= 26)
        {
            int cCount = Mathf.Clamp((i - 25) / 8, 1, 4);
            for (int c = 0; c < cCount; c++)
            {
                float cx = 16f + c * 8f;
                AddCrusher(new Vector3(cx, 2.5f, 0), new Vector2(1.4f, 5f), 6.5f, 2.5f);
            }
        }

        // 开关闸门解谜（需要的开关数随关递增）
        int requiredSwitches = i >= 12 ? 2 : 1;
        if (i >= 60) requiredSwitches = 3;
        var gate = AddGate(new Vector3(goalX, 1.5f, 0), requiredSwitches);
        AddSwitch(new Vector3(goalX - 6, 3.0f, 0), gate);
        if (requiredSwitches >= 2) AddSwitch(new Vector3(goalX - 9, 1.4f, 0), gate);
        if (requiredSwitches >= 3) AddSwitch(new Vector3(goalX - 12, 3.2f, 0), gate);

        AddGoal(new Vector3(goalX + 3, 1.5f, 0), i);
    }

    // ---------- 追逐关：长跑道 + 少许平台/尖刺 + 后方死亡之墙，一路往前跑 ----------
    void BuildChase(int i, float w, float goalX)
    {
        float px = 3f, py = 1.6f;
        int n = 12 + i / 6;
        for (int k = 0; k < n; k++)
        {
            Solid("P" + k, "platform", new Vector3(px, py, 0), new Vector2(2.2f, 0.4f));
            if (k % 3 == 0) AddCoin(new Vector3(px, py + 0.9f, 0));
            if (k >= 2 && k % 4 == 0) AddSpike(new Vector3(px - 1.4f, 0.5f, 0));
            if (k >= 3 && k % 5 == 0) AddMovingPlatform(new Vector3(px, 3.2f, 0), new Vector2(2f, 0.4f), new Vector2(0, 2.5f), 2.5f);
            px += 3.2f;
            py = 1.2f + (k % 2) * 1.0f;   // 单跳可达
            if (px > goalX - 4f) break;
        }

        AddGoal(new Vector3(goalX + 3, 1.5f, 0), i);

        var player = GameObject.FindWithTag("Player").transform;
        float chX = player.position.x - 7f;
        float chSpeed = Mathf.Clamp(6.6f + i * 0.012f, 6.6f, 7.6f);
        AddChaser(new Vector3(chX, 0f, 0), chSpeed);
    }

    // ---------- 组件工厂 ----------
    GameObject CameraAndBg(float w, bool chase)
    {
        var bg = Sprite("Background", "background", new Vector3(w / 2f, 5, 0), -10);
        var sr = bg.GetComponent<SpriteRenderer>();
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(w + 20, 18);

        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = chase ? 6.5f : 5.5f;
        camGO.transform.position = new Vector3(0, 1, -10);
        camGO.AddComponent<CameraFollow>();
        return camGO;
    }

    GameObject CreatePlayer(Vector3 pos)
    {
        var go = Sprite("Player", "player", pos, 10);
        go.tag = "Player";
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 1.1f);
        col.offset = new Vector2(0, 0);

        var pc = go.AddComponent<PlayerController>();
        pc.groundCheck = Child(go, "groundCheck", new Vector3(0, -0.7f, 0));
        pc.wallCheckL  = Child(go, "wallCheckL",  new Vector3(-0.55f, 0.2f, 0));
        pc.wallCheckR  = Child(go, "wallCheckR",  new Vector3( 0.55f, 0.2f, 0));
        return go;
    }

    GameObject Solid(string name, string spriteName, Vector3 pos, Vector2 size)
    {
        var go = Sprite(name, spriteName, pos, 0);
        go.tag = "Ground";
        var sr = go.GetComponent<SpriteRenderer>();
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = size;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = size;
        return go;
    }

    GameObject Sprite(string name, string spriteName, Vector3 pos, int order)
    {
        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteByName(spriteName);
        sr.sortingOrder = order;
        go.transform.position = pos;
        return go;
    }

    Transform Child(GameObject parent, string name, Vector3 localPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPos;
        return go.transform;
    }

    void AddSpike(Vector3 pos)
    {
        var go = Sprite("Spike", "spike", pos, 2);
        go.tag = "Hazard";
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.9f, 0.9f);
        col.offset = new Vector2(0, -0.1f);
        go.AddComponent<Hazard>();
    }

    void AddCoin(Vector3 pos)
    {
        var go = Sprite("Coin", "coin", pos, 3);
        go.tag = "Coin";
        var rb = go.AddComponent<Rigidbody2D>(); rb.isKinematic = true; rb.gravityScale = 0;
        var col = go.AddComponent<CircleCollider2D>(); col.isTrigger = true; col.radius = 0.5f;
        go.AddComponent<Collectible>();
    }

    void AddEnemy(Vector3 pos, int i)
    {
        var go = Sprite("Enemy", "enemy", pos, 3);
        go.tag = "Enemy";
        var rb = go.AddComponent<Rigidbody2D>(); rb.isKinematic = true; rb.gravityScale = 0; rb.freezeRotation = true;
        var col = go.AddComponent<BoxCollider2D>(); col.size = new Vector2(0.6f, 0.6f);
        var e = go.AddComponent<Enemy>();
        e.range = 2.5f;
        e.speed = Mathf.Min(2.5f + i * 0.02f, 5f);
        e.jumping = i >= 12;   // 第12关起怪物会跳
        if (e.jumping)
        {
            e.hopForce = 4.5f + Mathf.Min(i * 0.02f, 2f);
            e.hopInterval = Mathf.Max(1f, 1.6f - i * 0.006f);
        }
    }

    void AddGoal(Vector3 pos, int levelIndex)
    {
        var go = Sprite("Goal", "goal", pos, 3);
        go.tag = "Goal";
        var rb = go.AddComponent<Rigidbody2D>(); rb.isKinematic = true; rb.gravityScale = 0;
        var col = go.AddComponent<BoxCollider2D>(); col.isTrigger = true; col.size = new Vector2(0.8f, 1.5f);
        var g = go.AddComponent<Goal>();
        g.levelIndex = levelIndex;
    }

    Gate AddGate(Vector3 pos, int required)
    {
        var go = Sprite("Gate", "gate", pos, 4);
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.7f, 1.9f);
        var g = go.AddComponent<Gate>();
        g.required = required;
        return g;
    }

    SwitchPlate AddSwitch(Vector3 pos, Gate gate)
    {
        var go = Sprite("Switch", "switch", pos, 4);
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 0.4f);
        var s = go.AddComponent<SwitchPlate>();
        s.gate = gate;
        return s;
    }

    GameObject AddMovingPlatform(Vector3 pos, Vector2 size, Vector2 moveDir, float speed)
    {
        var go = Sprite("Mover", "platform", pos, 0);
        go.tag = "Ground";
        var sr = go.GetComponent<SpriteRenderer>();
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = size;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = size;
        var mp = go.AddComponent<MovingPlatform>();
        mp.move = moveDir;
        mp.speed = speed;
        return go;
    }

    void AddCrusher(Vector3 pos, Vector2 size, float topY, float bottomY)
    {
        var go = Sprite("Crusher", "ground", pos, 5);
        go.tag = "Hazard";
        var sr = go.GetComponent<SpriteRenderer>();
        sr.color = new Color(1f, 0.4f, 0.35f);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = size;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = size;
        var cr = go.AddComponent<Crusher>();
        cr.topY = topY;
        cr.bottomY = bottomY;
    }

    void AddChaser(Vector3 pos, float speed)
    {
        var go = Sprite("Chaser", "ground", pos, 6);
        var sr = go.GetComponent<SpriteRenderer>();
        sr.color = new Color(1f, 0.25f, 0.2f);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(1.2f, 9f);
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.2f, 9f);
        col.offset = new Vector2(0, 4.5f);
        var ch = go.AddComponent<Chaser>();
        ch.speed = speed;
    }

    Sprite SpriteByName(string name)
    {
        switch (name)
        {
            case "player": return spritePlayer;
            case "ground": return spriteGround;
            case "platform": return spritePlatform;
            case "spike": return spriteSpike;
            case "coin": return spriteCoin;
            case "enemy": return spriteEnemy;
            case "background": return spriteBackground;
            case "goal": return spriteGoal;
            case "gate": return spriteGate;
            case "switch": return spriteSwitch;
            default: return spriteGround;
        }
    }
}
