using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 2.5f;
    public float range = 3f;
    float dir = 1f;
    Vector3 start;
    SpriteRenderer sr;
    float groundY;

    [Header("跳跃怪")]
    public bool jumping = false;       // 是否会在原地周期性小跳
    public float hopForce = 5f;
    public float hopGravity = 18f;
    public float hopInterval = 1.4f;
    float hopTimer;
    bool hopping;
    float vy;

    void Start()
    {
        start = transform.position;
        groundY = start.y;
        sr = GetComponent<SpriteRenderer>();
        hopTimer = hopInterval;   // 第一次跳跃前先给一点时间
    }

    void Update()
    {
        // 水平来回巡逻
        transform.position += Vector3.right * dir * speed * Time.deltaTime;
        if (Mathf.Abs(transform.position.x - start.x) > range) dir *= -1;
        if (sr != null) sr.flipX = dir < 0;

        // 跳跃怪：周期性小跳，落回原高度
        if (jumping)
        {
            hopTimer -= Time.deltaTime;
            if (!hopping && hopTimer <= 0f)
            {
                hopping = true;
                vy = hopForce;
                hopTimer = hopInterval;
            }
            if (hopping)
            {
                vy -= hopGravity * Time.deltaTime;
                transform.position += Vector3.up * vy * Time.deltaTime;
                if (transform.position.y <= groundY)
                {
                    transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
                    vy = 0f;
                    hopping = false;
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Player") && GameManager.I != null)
        {
            var pc = c.gameObject.GetComponent<PlayerController>();
            if (pc != null && pc.Invincible) return;   // 冲刺无敌帧不受伤
            GameManager.I.Kill();
        }
    }
}
