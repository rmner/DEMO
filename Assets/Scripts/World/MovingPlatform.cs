using UnityEngine;

// 移动平台：垂直电梯 / 横向滑台。玩家站在上面时会被一起带着走。
// 注意：它不放 Rigidbody2D（保持静态碰撞体），这样 PlayerController 的
//      地面检测（只认 attachedRigidbody == null）仍能把平台当成"地面"。
public class MovingPlatform : MonoBehaviour
{
    [Header("运动")]
    public Vector2 move = new Vector2(0, 4f);   // 从起点往返 +move
    public float speed = 2f;
    public bool startAtB = false;               // 是否从终点开始

    public LayerMask solidMask = ~0;

    Vector3 posA, posB;
    bool forward;
    PlayerController pc;
    Rigidbody2D playerRb;

    void Start()
    {
        posA = transform.position;
        posB = transform.position + (Vector3)move;
        transform.position = startAtB ? posB : posA;
        forward = !startAtB;
        BindPlayer();
    }

    void BindPlayer()
    {
        if (pc != null && playerRb != null) return;
        var p = GameObject.FindWithTag("Player");
        if (p == null) return;
        pc = p.GetComponent<PlayerController>();
        playerRb = p.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        BindPlayer();

        Vector3 prev = transform.position;
        Vector3 next;

        if (forward) next = Vector3.MoveTowards(prev, posB, speed * Time.deltaTime);
        else         next = Vector3.MoveTowards(prev, posA, speed * Time.deltaTime);

        if (next == posB) forward = false;
        else if (next == posA) forward = true;

        transform.position = next;
        Vector3 delta = next - prev;

        // 玩家站在本平台上 → 跟着平台一起位移
        if (delta != Vector3.zero && PlayerOnMe())
        {
            if (playerRb != null) playerRb.position += (Vector2)delta;
            else if (pc != null) pc.transform.position += delta;
        }
    }

    bool PlayerOnMe()
    {
        if (pc == null || pc.groundCheck == null) return false;
        var col = GetComponent<Collider2D>();
        if (col == null) return false;
        foreach (var c in Physics2D.OverlapCircleAll(pc.groundCheck.position, pc.groundRadius, solidMask))
            if (c == col) return true;   // 判断玩家脚底检测圈是否落在本平台碰撞体上
        return false;
    }
}
