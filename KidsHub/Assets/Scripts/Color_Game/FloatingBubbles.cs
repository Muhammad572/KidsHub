// using UnityEngine;

// public class FloatingBubbles : MonoBehaviour
// {
//     public GameObject bubblePrefab;
//     public float spawnInterval = 1.5f;
//     public Vector2 spawnRangeX = new Vector2(-3f, 3f);
//     public Vector2 speedRange = new Vector2(0.5f, 1.5f);

//     private float timer;

//     private void Awake()
//     {
//         if (bubblePrefab == null)
//             bubblePrefab = Resources.Load<GameObject>("BubblePrefab");
//     }

//     private void Update()
//     {
//         timer += Time.deltaTime;
//         if (timer >= spawnInterval)
//         {
//             timer = 0;
//             SpawnBubble();
//         }
//     }
//     private void SpawnBubble()
//     {
//         if (bubblePrefab == null)
//         {
//             Debug.LogWarning("⚠️ FloatingBubbles: No bubblePrefab assigned!");
//             return;
//         }

//         Vector3 pos = new Vector3(Random.Range(spawnRangeX.x, spawnRangeX.y), -6f, 0f);
//         GameObject bubble = Instantiate(bubblePrefab, pos, Quaternion.identity, transform);

//         float speed = Random.Range(speedRange.x, speedRange.y);
//         Rigidbody2D rb = bubble.AddComponent<Rigidbody2D>();
//         rb.gravityScale = -speed;
//         rb.mass = 0.1f;
//         rb.linearDamping = 1f; // smoother float
//         Destroy(bubble, 8f);
//     }
// }

using UnityEngine;
using DG.Tweening;

public class FloatingBubbles : MonoBehaviour
{
    public GameObject bubblePrefab;
    public float spawnInterval = 1.5f;
    public Vector2 spawnRangeX = new Vector2(-4f, 4f);
    public Vector2 speedRange = new Vector2(0.5f, 1.5f);

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnBubble();
        }
    }

    private void SpawnBubble()
    {
        if (bubblePrefab == null)
        {
            Debug.LogWarning("⚠️ FloatingBubbles: No bubblePrefab assigned!");
            return;
        }

        // random X in world space
        float x = Random.Range(spawnRangeX.x, spawnRangeX.y);
        Vector3 pos = new Vector3(x, -6f, 0f);

        GameObject bubble = Instantiate(bubblePrefab, pos, Quaternion.identity);
        bubble.transform.SetParent(transform, true);

        // after spawning the bubble
        bubble.transform
            .DOLocalMoveX(bubble.transform.localPosition.x + Random.Range(-1f, 1f), 3f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        bubble.transform
            .DOScale(Random.Range(0.8f, 1.2f), 2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        float speed = Random.Range(speedRange.x, speedRange.y);

        // ✅ safely add and configure Rigidbody2D
        Rigidbody2D rb = bubble.GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = bubble.AddComponent<Rigidbody2D>();

        rb.gravityScale = -speed;
        rb.constraints = RigidbodyConstraints2D.None;
        rb.mass = 0.1f;

#if UNITY_6000_0_OR_NEWER
        rb.linearDamping = 1f; // ✅ Unity 6+ property
#else
        rb.drag = 1f; // ✅ fallback for older versions
#endif

        Destroy(bubble, 8f);
    }
}
