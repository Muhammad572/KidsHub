using UnityEngine;

public class WavyHeader : MonoBehaviour
{
    private Vector3 startPos;
    public float amplitude = 10f;
    public float speed = 1f;

    void Start() => startPos = transform.localPosition;

    void Update()
    {
        transform.localPosition = startPos + Vector3.up * Mathf.Sin(Time.time * speed) * amplitude;
    }
}
