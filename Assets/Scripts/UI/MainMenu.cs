using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void OnGUI()
    {
        var sd = SaveManager.Load();
        UiHelper.Dim(150);

        // 主菜单用固定高度面板：无论窗口多矮、内容多高，都死守在屏幕正中央（内容超高则内部滚动）
        float panelH = Mathf.Min(Screen.height * 0.84f, 640f);

        UiHelper.Centered(() =>
        {
            UiHelper.Title("跃境 · JUMP REALM");
            UiHelper.Sub("· 100 关 · 类银河恶魔城 ·");
            UiHelper.Space(2);
            UiHelper.Divider();
            UiHelper.Space(10);

            int unlocked = Mathf.Clamp(sd.unlockedLevel, 0, 100);
            bool beaten = sd.unlockedLevel >= 100;
            int cur = Mathf.Clamp(unlocked + 1, 1, 100);

            UiHelper.Sub("最高分  " + sd.bestScore);
            UiHelper.Sub("进度  " + unlocked + " / 100" + (beaten ? "  已通关！" : ""));
            UiHelper.Space(14);

            if (UiHelper.Button("新游戏 · 第 1 关")) { if (GameManager.I != null) GameManager.I.StartLevel(1); }
            if (!beaten && UiHelper.Button("继续游戏 · 第 " + cur + " 关")) { if (GameManager.I != null) GameManager.I.StartLevel(cur); }
            if (beaten && UiHelper.Button("通关回顾 · 终章")) { if (PauseManager.I != null) PauseManager.I.Resume(); SceneManager.LoadScene("Victory"); }
            if (UiHelper.Button("商城 · 用金币买东西")) { if (PauseManager.I != null) PauseManager.I.Resume(); SceneManager.LoadScene("Shop"); }

            if (unlocked >= 1)
            {
                UiHelper.Space(12);
                UiHelper.Sub("选关（已解锁）");
                float listH = Mathf.Clamp(panelH - 460f, 80f, 220f);
                UiHelper.ScrollBox(listH, () =>
                {
                    int col = 0;
                    GUILayout.BeginHorizontal();
                    for (int i = 1; i <= unlocked; i++)
                    {
                        if (UiHelper.SmallButton("第 " + i + " 关", 130)) { if (GameManager.I != null) GameManager.I.StartLevel(i); }

                        // 关键：每两格关掉当前行；仅当还没到最后一个格子时才开下一行，
                        // 避免偶数个格子时留下一个未关闭的 BeginHorizontal。
                        col++;
                        if (col % 2 == 0)
                        {
                            GUILayout.EndHorizontal();
                            if (i < unlocked) GUILayout.BeginHorizontal();
                        }
                    }
                    if (col % 2 != 0) GUILayout.EndHorizontal();   // 末行是奇数个格子时收尾
                });
            }

            UiHelper.Space(10);
            UiHelper.Divider();
            UiHelper.Space(6);
            UiHelper.Hint("A/D 移动 · 空格 跳 / 二段跳 / 蹬墙跳");
            UiHelper.Hint("Shift(或 L) 冲刺 · R 重生 · Esc 暂停");
        }, width: 360f, maxHeight: panelH);
    }
}
