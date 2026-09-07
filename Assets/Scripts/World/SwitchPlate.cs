using UnityEngine;

public class SwitchPlate : MonoBehaviour
{
    public Gate gate;
    bool used;

    void OnCollisionEnter2D(Collision2D c)
    {
        if (used || gate == null) return;
        if (c.gameObject.CompareTag("Player"))
        {
            used = true;
            gate.Trigger();
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.green;
        }
    }
}
