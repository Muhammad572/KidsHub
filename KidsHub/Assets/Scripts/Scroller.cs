using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 0.2f; // speed of scrolling
    public bool scrollVertical = false; // choose vertical or horizontal

    private Material mat;
    private Vector2 offset = Vector2.zero;

    void Start()
    {
        // Get the material from the Renderer
        if (GetComponent<Renderer>() != null)
        {
            mat = GetComponent<Renderer>().material;
        }
    }

    void Update()
    {
        if (mat == null) return;

        // Calculate new offset each frame
        if (scrollVertical)
        {
            offset.y += scrollSpeed * Time.deltaTime;
        }
        else
        {
            offset.x += scrollSpeed * Time.deltaTime;
        }

        // Apply offset to material
        mat.mainTextureOffset = offset;
    }
}
