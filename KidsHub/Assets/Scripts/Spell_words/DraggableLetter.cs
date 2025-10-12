using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

        // ✅ Ensure visible layer
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = 5; // slots should be lower (e.g. 0)
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
        isDragging = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        transform.position = startPosition;
    }
}
