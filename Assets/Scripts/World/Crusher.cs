using UnityEngine;

// 跳楼机式下压墙：升得慢、落得快，砸到底部停一下，循环往复。
// 玩家被压到就死（冲刺无敌帧除外）。要掐准时机从它下面穿过去。
public class Crusher : MonoBehaviour
{
    public float topY = 6.5f;       // 升起后的中心高度（底部可通行）
    public float bottomY = 2.5f;    // 落到底后的中心高度（挡住了）
    public float downSpeed = 16f;   // 下落快
    public float upSpeed = 5f;      // 回升慢
    public float waitTime = 0.5f;   // 底部停留时长

    enum Phase { Up, Down, Wait }
    Phase phase = Phase.Up;
    float waitTimer;

    void Update()
    {
        Vector3 pos = transform.position;
        switch (phase)
        {
            case Phase.Up:
                pos.y = Mathf.MoveTowards(pos.y, topY, upSpeed * Time.deltaTime);
                if (pos.y >= topY) phase = Phase.Down;
                break;
            case Phase.Down:
                pos.y = Mathf.MoveTowards(pos.y, bottomY, downSpeed * Time.deltaTime);
                if (pos.y <= bottomY) { waitTimer = waitTime; phase = Phase.Wait; }
                break;
            case Phase.Wait:
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f) phase = Phase.Up;
                break;
        }
        transform.position = pos;
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Player") && GameManager.I != null)
        {
            var pc = c.gameObject.GetComponent<PlayerController>();
            if (pc != null && pc.Invincible) return;
            GameManager.I.Kill();
        }
    }
}
