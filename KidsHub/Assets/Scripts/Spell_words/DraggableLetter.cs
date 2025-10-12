using UnityEngine;
using System.Collections;

public class DraggableLetter : MonoBehaviour
{
    public char letter;
    private Vector3 startPosition;
    private bool isDragging = false;
    private bool isLocked = false;
    private Camera mainCam;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        mainCam = Camera.main;

        // Ensure visible layer
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = 5; // make sure letters appear above slots
    }

    public void SetStartPosition()
    {
        startPosition = transform.position;
    }

    private void OnMouseDown()
    {
        if (isLocked) return;
        isDragging = true;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void OnMouseDrag()
    {
        if (!isDragging || isLocked) return;

        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        transform.position = mousePos;
    }

    private void OnMouseUp()
    {
        if (isLocked) return;
        isDragging = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    public void LockLetter()
    {
        isLocked = true;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        StartCoroutine(DisableColliderNextFrame());
    }

    private IEnumerator DisableColliderNextFrame()
    {
        yield return null; // wait one frame
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    public void ResetPosition()
    {
        Debug.Log($"🔄 ResetPosition called for {name} back to {startPosition}");
        transform.position = startPosition;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // make sure it's unlocked again for dragging
        isLocked = false;

        // ensure collider stays active
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        // force z to front so raycasts work
        var pos = transform.position;
        pos.z = 0;
        transform.position = pos;
    }
}
