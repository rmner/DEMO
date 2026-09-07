#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class DemoSceneBuilder
{
    const string SpriteFolder = "Assets/Sprites/";
    const string GroundTag = "Ground";

    [MenuItem("Tools/Metroidvania Demo/Build All Scenes")]
    public static void Build()
    {
        PrepareSprites();
        EnsureTag(GroundTag); EnsureTag("Coin"); EnsureTag("Hazard"); EnsureTag("Goal"); EnsureTag("Enemy");
        EnsureShopCatalog();

        BuildMainMenu();
        BuildShop();
        BuildLevelScene();
        BuildVictory();

        // 注册基础 4 个场景，并保留用户手摆的关卡场景（LevelN.unity），别被清掉
        var scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Shop.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Level.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Victory.unity", true),
        };
        foreach (var s in EditorBuildSettings.scenes)
            if (IsHandScene(s.path)) scenes.Add(s);
        EditorBuildSettings.scenes = scenes.ToArray();

        AssetDatabase.SaveAssets();
        Debug.Log("已生成 主菜单 + 商城 + 关卡(100关动态生成) + 通关，并写入 Build Settings。");
    }

    static void Save(Scene scene, string name)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/" + name + ".unity");
    }

    // 判断是不是"手摆的关卡场景"（Level12.unity 这种，而不是 Level.unity）
    static bool IsHandScene(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        return System.Text.RegularExpressions.Regex.IsMatch(path, @"Level\d+\.unity$");
    }

    static void BuildMainMenu()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CameraAndBg();
        EnsureManagers(true, true);
        new GameObject("MainMenu").AddComponent<MainMenu>();
        Save(scene, "MainMenu");
    }

    // 关卡：只放一个 LevelRuntimeBuilder（运行时按关卡号生成具体内容），并把素材 Sprite 填给它
    static void BuildLevelScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var go = new GameObject("LevelBuilder");
        var rb = go.AddComponent<LevelRuntimeBuilder>();
        rb.spritePlayer = Load("player");
        rb.spriteGround = Load("ground");
        rb.spritePlatform = Load("platform");
        rb.spriteSpike = Load("spike");
        rb.spriteCoin = Load("coin");
        rb.spriteEnemy = Load("enemy");
        rb.spriteBackground = Load("background");
        rb.spriteGoal = Load("goal");
        rb.spriteGate = Load("gate");
        rb.spriteSwitch = Load("switch");
        Save(scene, "Level");
    }

    static void BuildVictory()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CameraAndBg();
        EnsureManagers(true, false);
        new GameObject("VictoryUI").AddComponent<VictoryUI>();
        Save(scene, "Victory");
    }

    static void BuildShop()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CameraAndBg();
        new GameObject("Shop").AddComponent<Shop>();
        Save(scene, "Shop");
    }

    // 若还没有商城目录资源，就生成一份默认的（皮肤+能力，去掉道具/武器）
    static void EnsureShopCatalog()
    {
        const string path = "Assets/Resources/Data/ShopCatalog.asset";
        if (AssetDatabase.LoadAssetAtPath<ShopCatalog>(path) != null) return;

        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Data")) AssetDatabase.CreateFolder("Assets/Resources", "Data");

        AssetDatabase.CreateAsset(MakeDefaultCatalog(), path);
        AssetDatabase.SaveAssets();
        Debug.Log("已生成默认商城目录：" + path);
    }

    // 手动重置商城目录为默认（皮肤+能力）。改了商品结构/清空旧的道具项时用这个。
    [MenuItem("Tools/Metroidvania Demo/重置商城目录为默认(皮肤+能力)")]
    public static void ResetShopCatalog()
    {
        const string path = "Assets/Resources/Data/ShopCatalog.asset";
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Data")) AssetDatabase.CreateFolder("Assets/Resources", "Data");

        AssetDatabase.CreateAsset(MakeDefaultCatalog(), path);
        AssetDatabase.SaveAssets();
        Debug.Log("已重置商城目录为默认(皮肤+能力)：" + path);
    }

    static ShopCatalog MakeDefaultCatalog()
    {
        var cat = ScriptableObject.CreateInstance<ShopCatalog>();
        cat.items = new List<ShopItem>
        {
            // ---- 皮肤：不同颜色，同价 ----
            new ShopItem { id = "SK001", type = ShopItemType.Skin, displayName = "烈焰橙", description = "橙色皮肤", price = 100, tint = new Color(1f, 0.6f, 0.2f) },
            new ShopItem { id = "SK002", type = ShopItemType.Skin, displayName = "蔚蓝", description = "蓝色皮肤", price = 100, tint = new Color(0.3f, 0.7f, 1f) },
            new ShopItem { id = "SK003", type = ShopItemType.Skin, displayName = "翠绿", description = "绿色皮肤", price = 100, tint = new Color(0.3f, 0.9f, 0.5f) },
            new ShopItem { id = "SK004", type = ShopItemType.Skin, displayName = "紫罗兰", description = "紫色皮肤", price = 100, tint = new Color(0.8f, 0.4f, 1f) },
            new ShopItem { id = "SK005", type = ShopItemType.Skin, displayName = "桃红", description = "粉色皮肤", price = 100, tint = new Color(1f, 0.5f, 0.7f) },
            // ---- 能力：买了才解锁（没买=封锁）----
            new ShopItem { id = "AB_DASH", type = ShopItemType.Ability, displayName = "冲刺", description = "按住 Shift 高速冲刺，可越过缺口/尖刺，并短暂无敌", price = 120 },
            new ShopItem { id = "AB_DBJUMP", type = ShopItemType.Ability, displayName = "二段跳", description = "空中再按一次跳跃，跳得更高更远", price = 180 },
            new ShopItem { id = "AB_WALL", type = ShopItemType.Ability, displayName = "爬墙", description = "贴墙按住朝墙方向可沿墙爬升，也能蹬墙跳", price = 240 },
        };
        return cat;
    }

    static GameObject CameraAndBg()
    {
        var bg = Sprite("Background", "background", new Vector3(4, 6, 0), -10);
        var sr = bg.GetComponent<SpriteRenderer>();
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(80, 20);

        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.5f;
        camGO.transform.position = new Vector3(0, 1, -10);
        return camGO;
    }

    static void EnsureManagers(bool gm, bool pm)
    {
        if (gm && GameManager.I == null) new GameObject("GameManager").AddComponent<GameManager>();
        if (pm && PauseManager.I == null) new GameObject("PauseManager").AddComponent<PauseManager>();
    }

    static void PrepareSprites()
    {
        string[] names = { "player", "ground", "platform", "spike", "coin", "enemy", "background", "goal", "gate", "switch" };
        foreach (var n in names)
        {
            var importer = AssetImporter.GetAtPath(SpriteFolder + n + ".png") as TextureImporter;
            if (importer == null) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32f;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
    }

    public static void EnsureTag(string tag)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (assets == null || assets.Length == 0) return;
        var so = new SerializedObject(assets[0]);
        var tags = so.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        so.ApplyModifiedProperties();
    }

    static Sprite Load(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + name + ".png");

    static GameObject Sprite(string name, string spriteName, Vector3 pos, int order)
    {
        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Load(spriteName);
        sr.sortingOrder = order;
        go.transform.position = pos;
        return go;
    }
}
#endif
