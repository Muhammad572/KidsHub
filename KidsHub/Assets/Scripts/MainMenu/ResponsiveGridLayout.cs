using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(GridLayoutGroup))]
public class ResponsiveGridLayout : MonoBehaviour
{
    public enum FitMode { FitToWidth, FitToHeight, FitToParent, Manual }

    [Header("Grid Settings")]
    public FitMode fitMode = FitMode.FitToParent;
    public int columns = 3;
    public int rows = 3;
    public Vector2 spacing = new Vector2(5, 5);
    public Vector2 padding = new Vector2(10, 10);

    [Header("Size Adjustment")]
    [Range(0.5f, 2f)] public float scaleFactor = 1f;
    public bool autoAdjustRows = false;
    public bool maintainAspect = true;
    public Vector2 minCellSize = new Vector2(50, 50);
    public Vector2 maxCellSize = new Vector2(500, 500);

    private RectTransform rectTransform;
    private GridLayoutGroup gridLayout;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        gridLayout = GetComponent<GridLayoutGroup>();
    }

    void Update()
    {
        UpdateGrid();
    }

    void UpdateGrid()
    {
        if (rectTransform == null || gridLayout == null) return;

        int childCount = transform.childCount;
        if (autoAdjustRows && columns > 0)
            rows = Mathf.CeilToInt((float)childCount / columns);

        float width = rectTransform.rect.width - padding.x - (spacing.x * (columns - 1));
        float height = rectTransform.rect.height - padding.y - (spacing.y * (rows - 1));

        float cellWidth = width / columns;
        float cellHeight = height / rows;

        // Fit mode logic
        switch (fitMode)
        {
            case FitMode.FitToWidth:
                cellHeight = cellWidth;
                break;
            case FitMode.FitToHeight:
                cellWidth = cellHeight;
                break;
            case FitMode.FitToParent:
                if (maintainAspect)
                {
                    float minSide = Mathf.Min(cellWidth, cellHeight);
                    cellWidth = cellHeight = minSide;
                }
                break;
            case FitMode.Manual:
                // Leave as is, use manually set sizes
                break;
        }

        // Apply scale factor
        cellWidth *= scaleFactor;
        cellHeight *= scaleFactor;

        // Clamp to limits
        cellWidth = Mathf.Clamp(cellWidth, minCellSize.x, maxCellSize.x);
        cellHeight = Mathf.Clamp(cellHeight, minCellSize.y, maxCellSize.y);

        gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
        gridLayout.spacing = spacing;
    }
}
