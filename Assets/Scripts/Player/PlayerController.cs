using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动")]
    public float moveSpeed = 8f;
    public float accel = 45f;
    public float airAccelFactor = 0.65f;
    public float groundFriction = 55f;

    [Header("跳跃")]
    public float jumpForce = 14f;
    public int maxJumps = 2;                 // 2 = 可二段跳
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;
    public float jumpCut = 0.4f;             // 松键跳矮

    [Header("蹬墙跳")]
    public float wallSlideMax = 2.5f;
    public float wallJumpX = 9f;
    public float wallJumpY = 14f;
    public float wallJumpLock = 0.2f;

    [Header("冲刺")]
    public float dashSpeed = 22f;
    public float dashDuration = 0.16f;
    public float dashCooldown = 0.5f;
    public KeyCode dashKey = KeyCode.LeftShift;

    [Header("能力（商城购买后解锁）")]
    public float wallClimbSpeed = 3f;   // 爬墙速度
    bool abDash, abDouble, abWall;      // 是否拥有 冲刺/二段跳/爬墙

    [Header("检测")]
    public Transform groundCheck;
    public Transform wallCheckL, wallCheckR;
    public float groundRadius = 0.12f;
    public float wallRadius = 0.18f;
    public LayerMask solidMask = ~0;

    [Header("特效")]
    public ParticleSystem dust;

    Rigidbody2D rb;
    float facing = 1f;
    float coyote, buffer, lockTimer, wallJumpDir;
    int jumpsLeft;
    bool grounded, touchingWall;
    int wallSide;                 // -1 左墙 / 1 右墙
    bool canCutJump;
    float dashTimer, dashCd, normalGravity;
    Vector2 dashDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        normalGravity = rb.gravityScale;

        // 读取已拥有的能力/皮肤（商城购买后解锁）
        var sd = SaveManager.Load();
        abDash   = sd.ownedItems != null && sd.ownedItems.Contains("AB_DASH");
        abDouble = sd.ownedItems != null && sd.ownedItems.Contains("AB_DBJUMP");
        abWall   = sd.ownedItems != null && sd.ownedItems.Contains("AB_WALL");
        if (!abDouble) maxJumps = 1;   // 没买二段跳 → 只能单跳
        ApplyEquippedSkin(sd.equippedSkin);
    }

    void ApplyEquippedSkin(string skinId)
    {
        if (string.IsNullOrEmpty(skinId)) return;
        var cat = Resources.Load<ShopCatalog>("Data/ShopCatalog");
        if (cat == null || cat.items == null) return;
        var it = cat.items.Find(x => x.id == skinId);
        if (it == null) return;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = it.tint;
    }

    void Update()
    {
        grounded = CheckGround();
        touchingWall = CheckWalls(out wallSide);
        if (grounded) jumpsLeft = maxJumps - 1;

        float x = Input.GetAxis("Horizontal");
        if (Mathf.Abs(x) > 0.01f) facing = Mathf.Sign(x);
        Flip();

        if (Input.GetButtonDown("Jump")) buffer = jumpBufferTime;
        else buffer -= Time.deltaTime;
        coyote = grounded ? coyoteTime : coyote - Time.deltaTime;
        lockTimer = Mathf.Max(0f, lockTimer - Time.deltaTime);

        // 冲刺：冷却好后按 Shift（或 L）触发（需已购买"冲刺"）
        dashCd = Mathf.Max(0f, dashCd - Time.deltaTime);
        if (abDash && (Input.GetKeyDown(dashKey) || Input.GetKeyDown(KeyCode.L)) && dashCd <= 0f)
            StartDash(x);

        // 蹬墙跳：空中 + 贴墙 + 按跳 → 反方向弹开（需已购买"爬墙"）
        if (abWall && !grounded && touchingWall && Input.GetButtonDown("Jump"))
        {
            rb.velocity = new Vector2(-wallSide * wallJumpX, wallJumpY);
            lockTimer = wallJumpLock;
            wallJumpDir = wallSide;
            canCutJump = true;
            buffer = 0f;
        }
    }

    void FixedUpdate()
    {
        if (dashTimer > 0f)
        {
            dashTimer -= Time.fixedDeltaTime;
            rb.velocity = new Vector2(dashDir.x * dashSpeed, 0f);
            if (dashTimer <= 0f) rb.gravityScale = normalGravity;
            return;
        }

        float x = lockTimer > 0f ? 0f : Input.GetAxis("Horizontal");
        float target = x * moveSpeed;
        float acc = grounded ? accel : accel * airAccelFactor;
        float newX = Mathf.MoveTowards(rb.velocity.x, target, acc * Time.fixedDeltaTime);

        if (lockTimer > 0f) newX = wallJumpDir * wallJumpX;
        else if (grounded && Mathf.Abs(x) < 0.01f)
            newX = Mathf.MoveTowards(newX, 0f, groundFriction * Time.fixedDeltaTime);

        rb.velocity = new Vector2(newX, rb.velocity.y);

        if (buffer > 0f)
        {
            if (coyote > 0f) { Jump(); coyote = 0f; buffer = 0f; }
            else if (jumpsLeft > 0) { Jump(); jumpsLeft--; buffer = 0f; }
        }

        // 变高跳：松键压短（只压一次）
        if (!Input.GetButton("Jump") && rb.velocity.y > 0f && canCutJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCut);
            canCutJump = false;
        }

        // 爬墙/滑墙（买了"爬墙"才生效）：朝墙按住 → 沿墙爬升，否则缓滑
        if (touchingWall && !grounded && abWall)
        {
            float x1 = Input.GetAxis("Horizontal");
            if (Mathf.Abs(x1) > 0.1f && Mathf.Sign(x1) == wallSide)
                rb.velocity = new Vector2(rb.velocity.x, wallClimbSpeed);
            else
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideMax));
        }

        UpdateDust();
    }

    void Jump() { rb.velocity = new Vector2(rb.velocity.x, jumpForce); canCutJump = true; }

    void StartDash(float x)
    {
        float dx = Mathf.Abs(x) > 0.05f ? Mathf.Sign(x) : facing;
        dashDir = new Vector2(dx, 0f);
        dashTimer = dashDuration;
        dashCd = dashCooldown;
        rb.gravityScale = 0f;    // 冲刺时短暂失重，水平高速滑行
        canCutJump = false;
    }

    // 冲刺无敌帧：冲刺期间不会被尖刺/怪物/跳楼机墙/追兵撞死
    public bool Invincible { get { return dashTimer > 0f; } }

    bool CheckGround()
    {
        foreach (var h in Physics2D.OverlapCircleAll(groundCheck.position, groundRadius, solidMask))
            if (h.attachedRigidbody == null) return true;   // 只认静态碰撞体（地面/平台/墙）
        return false;
    }

    bool CheckWalls(out int side)
    {
        bool left = false, right = false;
        foreach (var h in Physics2D.OverlapCircleAll(wallCheckL.position, wallRadius, solidMask))
            if (h.attachedRigidbody == null) { left = true; break; }
        foreach (var h in Physics2D.OverlapCircleAll(wallCheckR.position, wallRadius, solidMask))
            if (h.attachedRigidbody == null) { right = true; break; }
        side = left ? -1 : (right ? 1 : 0);
        return left || right;
    }

    void Flip()
    {
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * facing;
        transform.localScale = s;
    }

    void UpdateDust()
    {
        if (dust == null) return;
        if (grounded && Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f && lockTimer <= 0f)
        { if (!dust.isPlaying) dust.Play(); }
        else if (dust.isPlaying) dust.Stop();
    }
}
