using UnityEngine;

public class Goal : MonoBehaviour
{
    public int levelIndex = 0;

    void OnTriggerEnter2D(Collider2D c)
    {
        if (c.CompareTag("Player") && GameManager.I != null)
            GameManager.I.LevelComplete(levelIndex);
    }
}
