using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager I;          // 单例
    public int score;
    public int currentLevel = 1;          // 当前关卡（1..100）
    public int maxLevel = 100;
    public string message = "";
    Transform player;
    Transform spawn;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }   // 去重
        I = this;
        DontDestroyOnLoad(gameObject);                                 // 跨场景保留
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }     // 订阅
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }    // 退订
    void Start() { RefreshRefs(); }

    void OnSceneLoaded(Scene s, LoadSceneMode m) { RefreshRefs(); }

    void RefreshRefs()
    {
        player = GameObject.FindWithTag("Player")?.transform;          // 每次进场景重新找
        spawn  = GameObject.Find("PlayerSpawn")?.transform;
        message = "";
    }

    public void AddScore(int v)
    {
        score += v;
        var sd = SaveManager.Load();
        if (score > sd.bestScore) { sd.bestScore = score; SaveManager.Save(sd); }
    }

    // 吃到金币：加分数，并存入可花的金币钱包（买东西用）
    public void AddCoin(int v)
    {
        score += v;
        var sd = SaveManager.Load();
        sd.coins += v;
        if (score > sd.bestScore) sd.bestScore = score;
        SaveManager.Save(sd);
    }

    public void Kill()
    {
        message = "你死了！0.5 秒后重生";
        CancelInvoke();
        Invoke(nameof(Respawn), 0.5f);
    }

    public void Respawn()
    {
        if (spawn != null && player != null) player.position = spawn.position;
        var rb = player != null ? player.GetComponent<Rigidbody2D>() : null;
        if (rb != null) rb.velocity = Vector2.zero;
        ResetChaser();
        message = "";
    }

    // 新开一局：清空分数，回到第1关
    public void ResetRun()
    {
        score = 0;
        currentLevel = 1;
    }

    // 从菜单进入指定关卡（"点击时才加载"：只生成这一关）
    public void StartLevel(int level)
    {
        currentLevel = Mathf.Clamp(level, 1, maxLevel);
        if (PauseManager.I != null) PauseManager.I.Resume();
        SceneManager.LoadScene(SceneFor(currentLevel));
    }

    // 过关：解锁 + 记录成绩 + 推进到下一关
    public void LevelComplete(int completedLevel)
    {
        var sd = SaveManager.Load();
        if (completedLevel > sd.unlockedLevel) { sd.unlockedLevel = completedLevel; SaveManager.Save(sd); }
        if (score > sd.bestScore) { sd.bestScore = score; SaveManager.Save(sd); }
        if (PauseManager.I != null) PauseManager.I.Resume();

        if (completedLevel >= maxLevel) SceneManager.LoadScene("Victory");
        else { currentLevel = completedLevel + 1; SceneManager.LoadScene(SceneFor(currentLevel)); }
    }

    // 这一关若已用"关卡生成器"手摆成场景就加载它，否则走程序生成（单个 Level 场景）
    static string SceneFor(int level)
    {
        string hand = "Assets/Scenes/Level" + level + ".unity";
        if (SceneUtility.GetBuildIndexByScenePath(hand) >= 0) return "Level" + level;
        return "Level";
    }

    void ResetChaser()
    {
        var ch = GameObject.Find("Chaser");
        if (ch != null && spawn != null) ch.GetComponent<Chaser>()?.ResetBehind(spawn.position);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) Respawn();   // 原地重生（保留当前关与分数）
    }
}
