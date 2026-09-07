using UnityEngine;

public class Hazard : MonoBehaviour
{
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
