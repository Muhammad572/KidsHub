using UnityEngine;

public class ClickRayTest : MonoBehaviour
{
    Camera cam;

    void Start()
    {
        cam = Camera.main;
        Debug.Log("🎯 ClickRayTest active on " + gameObject.name);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPoint = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(worldPoint.x, worldPoint.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (hit.collider != null)
                Debug.Log($"🟩 Ray hit: {hit.collider.name}");
            else
                Debug.Log("❌ Ray hit nothing");
        }
    }
}
