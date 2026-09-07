using UnityEngine;

public class Gate : MonoBehaviour
{
    public int required = 1;    // 需要多少次触发（开关/钥匙）才开门
    int count;
    public bool IsOpen { get; private set; }

    public void Trigger() { count++; Check(); }
    void Check() { if (count >= required) Open(); }
    void Open()
    {
        IsOpen = true;
        var col = GetComponent<Collider2D>(); if (col != null) col.enabled = false;   // 移除阻挡
        var sr  = GetComponent<SpriteRenderer>(); if (sr != null) sr.color = new Color(0.4f, 1f, 0.5f, 0.55f);
    }
}
