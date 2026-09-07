using UnityEngine;
using System;

// 一套统一的 IMGUI 主题：暗色面板 + 青色强调 + 圆角渐变按钮(带悬停/按下反馈)。
// 所有界面都走这里，改一处即全局生效。
public static class UiHelper
{
    // ---- 调色板 ----
    static readonly Color Accent  = new Color(0.28f, 0.88f, 0.76f, 1f);   // 青
    static readonly Color Accent2 = new Color(1f, 0.72f, 0.35f, 1f);      // 橙(高亮)

    // ---- 主题缓存 ----
    static GUIStyle _title, _sub, _hint, _btn, _btnSmall, _panel, _pill, _bar, _hud;
    static bool _built;
    static Vector2 _scroll;        // 选关列表等内部滚动
    static Vector2 _outerScroll;   // Centered 面板自身的滚动(防溢出)

    static void Build()
    {
        if (_built) return;
        _built = true;

        // 面板：暗色圆角半透明，9-slice 保持圆角不拉伸
        _panel = new GUIStyle(GUI.skin.box);
        _panel.normal.background = Rounded(40, 40, 14,
            new Color(0.07f, 0.08f, 0.12f, 0.94f),
            new Color(0.04f, 0.05f, 0.08f, 0.94f));
        _panel.border = new RectOffset(14, 14, 14, 14);
        _panel.padding = new RectOffset(26, 26, 24, 24);
        _panel.margin = new RectOffset(0, 0, 0, 0);

        // 按钮：三态背景（常态/悬停/按下），圆角渐变
        _btn = new GUIStyle(GUI.skin.button);
        _btn.normal.background = Rounded(32, 32, 10,
            new Color(0.18f, 0.22f, 0.33f, 1f),
            new Color(0.11f, 0.14f, 0.22f, 1f));
        _btn.hover.background = Rounded(32, 32, 10,
            new Color(0.25f, 0.32f, 0.46f, 1f),
            new Color(0.15f, 0.19f, 0.29f, 1f));
        _btn.active.background = Rounded(32, 32, 10,
            new Color(0.22f, 0.27f, 0.40f, 1f),
            new Color(0.13f, 0.16f, 0.25f, 1f));
        _btn.border = new RectOffset(10, 10, 10, 10);
        _btn.padding = new RectOffset(16, 16, 8, 8);
        _btn.fixedHeight = 46;
        _btn.fontSize = 18;
        _btn.fontStyle = FontStyle.Bold;
        _btn.alignment = TextAnchor.MiddleCenter;
        _btn.normal.textColor = Color.white;
        _btn.hover.textColor = new Color(0.95f, 1f, 1f, 1f);
        _btn.active.textColor = new Color(0.85f, 0.9f, 0.9f, 1f);

        // 紧凑小按钮（选关网格用）
        _btnSmall = new GUIStyle(_btn);
        _btnSmall.fixedHeight = 38;
        _btnSmall.fontSize = 14;

        // 标题
        _title = new GUIStyle(GUI.skin.label);
        _title.fontSize = 27;
        _title.fontStyle = FontStyle.Bold;
        _title.alignment = TextAnchor.MiddleCenter;
        _title.normal.textColor = Color.white;
        _title.padding = new RectOffset(0, 0, 0, 2);

        // 小字（副标题/信息）
        _sub = new GUIStyle(GUI.skin.label);
        _sub.fontSize = 15;
        _sub.alignment = TextAnchor.MiddleCenter;
        _sub.normal.textColor = new Color(0.74f, 0.78f, 0.86f, 1f);
        _sub.padding = new RectOffset(0, 0, 1, 1);

        // 提示（更小更灰）
        _hint = new GUIStyle(GUI.skin.label);
        _hint.fontSize = 13;
        _hint.alignment = TextAnchor.MiddleCenter;
        _hint.normal.textColor = new Color(0.52f, 0.56f, 0.64f, 1f);

        // HUD 药丸小面板
        _pill = new GUIStyle(GUI.skin.box);
        _pill.normal.background = Rounded(32, 32, 9,
            new Color(0.09f, 0.10f, 0.15f, 0.80f),
            new Color(0.06f, 0.07f, 0.10f, 0.80f));
        _pill.border = new RectOffset(9, 9, 9, 9);
        _pill.padding = new RectOffset(16, 16, 10, 10);
        _pill.margin = new RectOffset(0, 0, 0, 0);

        // 分隔线（强调色横棒）
        _bar = new GUIStyle(GUI.skin.box);
        _bar.normal.background = SolidColor(Accent);
        _bar.fixedHeight = 3;
        _bar.margin = new RectOffset(0, 0, 2, 2);
        _bar.border = new RectOffset(0, 0, 0, 0);
        _bar.padding = new RectOffset(0, 0, 0, 0);

        // HUD 内部文字
        _hud = new GUIStyle(GUI.skin.label);
        _hud.fontSize = 15;
        _hud.normal.textColor = Color.white;
        _hud.alignment = TextAnchor.MiddleLeft;
    }

    // ---------- 供界面使用的排版方法 ----------
    public static void Title(string s) { Build(); GUILayout.Label(s, _title); }
    public static void Sub(string s)    { Build(); GUILayout.Label(s, _sub); }
    public static void Hint(string s)   { Build(); GUILayout.Label(s, _hint); }

    /// 金色文字（金币/价格）
    public static void Money(string s)
    {
        Build();
        var st = new GUIStyle(_sub);
        st.fontSize = 15;
        st.fontStyle = FontStyle.Bold;
        st.normal.textColor = new Color(1f, 0.82f, 0.32f, 1f);
        GUILayout.Label(s, st);
    }
    public static void Alert(string s)
    {
        Build();
        var st = new GUIStyle(_sub);
        st.fontSize = 17;
        st.fontStyle = FontStyle.Bold;
        st.normal.textColor = new Color(1f, 0.58f, 0.52f, 1f);
        GUILayout.Label(s, st);
    }
    public static bool Button(string s) { Build(); return GUILayout.Button(s, _btn); }
    public static bool SmallButton(string s, float width = 0f)
    {
        Build();
        if (width > 0f) return GUILayout.Button(s, _btnSmall, GUILayout.Width(width));
        return GUILayout.Button(s, _btnSmall);
    }

    /// HUD 内部文字行
    public static void HUDText(string s) { Build(); GUILayout.Label(s, _hud); }
    public static void Space(float v)   { GUILayout.Space(v); }

    /// 细强调色分隔线
    public static void Divider() { Build(); GUILayout.Box("", _bar); }

    /// 可滚动选关区
    public static void ScrollBox(float height, Action content)
    {
        Build();
        _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(height));
        content();
        GUILayout.EndScrollView();
    }

    /// 顶部即时状态的半透明小药丸
    public static void Pill(Action content)
    {
        Build();
        GUILayout.BeginVertical(_pill);
        {
            GUILayout.Label(string.Empty);   // 撑一点高度
            content();
        }
        GUILayout.EndVertical();
    }

    /// 半透明黑幕（底色）
    public static void Dim(int alpha = 150)
    {
        Build();
        Color prev = GUI.color;
        GUI.color = new Color(0, 0, 0, alpha / 255f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), White);
        GUI.color = prev;
    }

    /// 内容垂直+水平居中，放在圆角面板上。
    /// maxHeight>0 时：面板用固定高度 maxHeight，内容超高则内部滚动，
    ///   这样无论内容多高、窗口多矮，面板都死死卡在屏幕正中央。
    public static void Centered(Action content, float width = 340f, bool panel = true, float maxHeight = 0f)
    {
        Build();
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (panel)
        {
            if (maxHeight > 0f)
            {
                GUILayout.BeginVertical(_panel, GUILayout.Width(width), GUILayout.Height(maxHeight));
                _outerScroll = GUILayout.BeginScrollView(_outerScroll, GUILayout.MaxHeight(maxHeight));
                content();
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical(_panel, GUILayout.Width(width), GUILayout.MinHeight(120));
                content();
                GUILayout.EndVertical();
            }
        }
        else
        {
            GUILayout.BeginVertical(GUILayout.Width(width));
            content();
            GUILayout.EndVertical();
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    // ---------- 纹理工厂 ----------
    static Texture2D _white;
    static Texture2D White => _white ?? (_white = SolidColor(Color.white));

    static Texture2D SolidColor(Color c)
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        t.wrapMode = TextureWrapMode.Clamp;
        t.filterMode = FilterMode.Bilinear;
        return t;
    }

    static Texture2D Rounded(int w, int h, float radius, Color top, Color bottom)
    {
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
        {
            float k = y / (float)(h - 1);
            Color c = Color.Lerp(top, bottom, k);
            for (int x = 0; x < w; x++)
               t.SetPixel(x, y, InRound(x, y, w, h, radius) ? c : Color.clear);
        }
        t.Apply();
        t.wrapMode = TextureWrapMode.Clamp;
        t.filterMode = FilterMode.Bilinear;
        return t;
    }

    static bool InRound(int x, int y, int w, int h, float r)
    {
        if (x >= 0 && x < r && y >= 0 && y < r) return Sqr(x - r, y - r) <= r * r;
        if (x >= w - r && x < w && y >= 0 && y < r) return Sqr(x - (w - 1 - r), y - r) <= r * r;
        if (x >= 0 && x < r && y >= h - r && y < h) return Sqr(x - r, y - (h - 1 - r)) <= r * r;
        if (x >= w - r && x < w && y >= h - r && y < h) return Sqr(x - (w - 1 - r), y - (h - 1 - r)) <= r * r;
        return true;
    }
    static float Sqr(float a, float b) => a * a + b * b;
}
