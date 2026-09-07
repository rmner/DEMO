using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryUI : MonoBehaviour
{
    void OnGUI()
    {
        var sd = SaveManager.Load();
        if (PauseManager.I != null) PauseManager.I.Resume();
        UiHelper.Dim(150);

        UiHelper.Centered(() =>
        {
            UiHelper.Title("恭喜通关！");
            UiHelper.Sub("100 关全部完成");
            UiHelper.Space(2);
            UiHelper.Divider();
            UiHelper.Space(12);
            UiHelper.Sub("最高分  " + sd.bestScore + "  已保存");
            UiHelper.Space(20);
            if (UiHelper.Button("返回主菜单")) SceneManager.LoadScene("MainMenu");
            if (UiHelper.Button("重新开始 · 第 1 关")) { if (GameManager.I != null) GameManager.I.StartLevel(1); }
        });
    }
}
