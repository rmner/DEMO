using UnityEngine;

public class HUD : MonoBehaviour
{
    void OnGUI()
    {
        if (GameManager.I == null) return;

        // ---- 暂停覆盖层（居中） ----
        if (PauseManager.I != null && PauseManager.I.isPaused)
        {
            UiHelper.Dim(175);
            UiHelper.Centered(() =>
            {
                UiHelper.Title("已暂停");
                UiHelper.Space(2);
                UiHelper.Divider();
                UiHelper.Space(16);
                if (UiHelper.Button("继续游戏")) PauseManager.I.Resume();
                if (UiHelper.Button("返回主菜单")) PauseManager.I.ReturnToMenu();
            });
            return;
        }

        // ---- 顶部居中：即时状态药丸 ----
        GUILayout.Space(16);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        UiHelper.Pill(() =>
        {
            UiHelper.HUDText("第    " + GameManager.I.currentLevel + " / " + GameManager.I.maxLevel + " 关");
            UiHelper.HUDText("分数  " + GameManager.I.score);
        });
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        // ---- 居中提示（死亡 / 追兵警告） ----
        if (!string.IsNullOrEmpty(GameManager.I.message))
        {
            GUILayout.Space(12);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            UiHelper.Alert(GameManager.I.message);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        // ---- 底部居中：操作提示 + 能力状态 ----
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        UiHelper.Hint("A/D 移动 · 空格 跳 · Shift 冲刺 · R 重生 · Esc 暂停");
        UiHelper.Hint(AbilityLine());
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(14);
    }

    string AbilityLine()
    {
        var sd = SaveManager.Load();
        bool owned = sd.ownedItems != null;
        string d  = (owned && sd.ownedItems.Contains("AB_DASH")) ? "冲刺✓" : "冲刺✗";
        string dj = (owned && sd.ownedItems.Contains("AB_DBJUMP")) ? "二段跳✓" : "二段跳✗";
        string w  = (owned && sd.ownedItems.Contains("AB_WALL")) ? "爬墙✓" : "爬墙✗";
        return "能力  " + d + "  " + dj + "  " + w + "   （未解锁的要去商城买）";
    }
}
