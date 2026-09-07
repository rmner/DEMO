using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smooth = 5f;
    public Vector2 offset = new Vector2(0, 1.5f);

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 p = target.position + new Vector3(offset.x, offset.y, -10);
        transform.position = Vector3.Lerp(transform.position, p, smooth * Time.deltaTime);
    }
}
