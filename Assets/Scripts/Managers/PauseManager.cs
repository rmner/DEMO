using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager I;
    public bool isPaused;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        string s = SceneManager.GetActiveScene().name;
        if (s == "MainMenu" || s == "Victory" || s == "Shop") return;   // 菜单/结算/商城不响应暂停
        if (Input.GetKeyDown(KeyCode.Escape)) Toggle();
    }

    public void Toggle()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void Resume() { isPaused = false; Time.timeScale = 1f; }

    public void ReturnToMenu()
    {
        Resume();
        SceneManager.LoadScene("MainMenu");
    }
}
