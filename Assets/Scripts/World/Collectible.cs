using UnityEngine;

public class Collectible : MonoBehaviour
{
    public int value = 1;
    public float spinSpeed = 120f;

    void Update() { transform.Rotate(0, 0, spinSpeed * Time.deltaTime); }

    void OnTriggerEnter2D(Collider2D c)
    {
        if (c.CompareTag("Player") && GameManager.I != null)
        {
            GameManager.I.AddCoin(value);   // 金币入钱包，商城能花
            Destroy(gameObject);
        }
    }
}
