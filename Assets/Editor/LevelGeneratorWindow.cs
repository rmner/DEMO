#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// 关卡生成器：把"第 N 关"用代码生成到独立场景里（编辑模式、带真实碰撞框），
// 你就能在编辑器里拖动/微调/Ctrl+S 保存；运行时会加载场景里的物体，不再按代码重建。
public class LevelGeneratorWindow : EditorWindow
{
    const string SpriteFolder = "Assets/Sprites/";
    int level = 1;

    [MenuItem("Tools/Metroidvania Demo/关卡生成器（可手摆）")]
    public static void Open() { GetWindow<LevelGeneratorWindow>("关卡生成器"); }

    void OnGUI()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("生成一个可在编辑器里手摆的关卡场景", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "生成后你会在 Scene 视图看到真实的碰撞框，可拖动物体、Ctrl+S 保存；\n" +
            "运行时加载场景里的物体，不再按公式重建。过关会自动跳到下一关。",
            EditorStyles.helpBox);

        EditorGUILayout.Space(10);
        level = EditorGUILayout.IntSlider("关卡号（1~100）", level, 1, 100);
        EditorGUILayout.Space(10);

        if (GUILayout.Button("生成 / 覆盖  第 " + level + " 关  场景", GUILayout.Height(36)))
            Generate(level);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("提示：给某一关勾上它，游戏流程就会用这关；没生成的关仍走程序生成。", EditorStyles.miniLabel);
    }

    void Generate(int idx)
    {
        // 确保关卡用到的自定义 Tag 存在
        DemoSceneBuilder.EnsureTag("Ground");
        DemoSceneBuilder.EnsureTag("Coin");
        DemoSceneBuilder.EnsureTag("Hazard");
        DemoSceneBuilder.EnsureTag("Goal");
        DemoSceneBuilder.EnsureTag("Enemy");

        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var go = new GameObject("LevelBuilder");
        var rb = go.AddComponent<LevelRuntimeBuilder>();
        rb.debugLevel = idx;
        rb.handMode = true;
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

        rb.PopulateInEditor();   // 编辑模式生成该关物体（带真实碰撞框）

        string path = "Assets/Scenes/Level" + idx + ".unity";
        RegisterScene(path);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, path);
        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        Debug.Log("已生成可手摆的第 " + idx + " 关场景：" + path);
    }

    void RegisterScene(string path)
    {
        var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in list)
            if (s.path == path) return;   // 已注册，不用重复
        list.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = list.ToArray();
    }

    static Sprite Load(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + name + ".png");
}
#endif
