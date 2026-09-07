using UnityEngine;

// 追逐战追兵（死亡之墙）：一条红色高墙匀速向右推进，碰到玩家即死。
// 玩家必须一路往前跑 / 用冲刺拉开距离，绝不能停下。
public class Chaser : MonoBehaviour
{
    public float speed = 6.8f;

    void Update()
    {
        // 无脑向右推进；关卡始终往右，保持紧迫感
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.flipX = false;
        transform.position += Vector3.right * speed * Time.deltaTime;
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

    // 玩家死亡重生时，把追兵推回出生点后面，避免复活即死
    public void ResetBehind(Vector3 spawn)
    {
        transform.position = new Vector3(spawn.x - 7f, spawn.y, spawn.z);
    }
}
