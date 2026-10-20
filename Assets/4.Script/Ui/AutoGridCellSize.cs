using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class AutoGridCellSize : MonoBehaviour
{
    [SerializeField] private float cellHeight = 125f;

    private GridLayoutGroup grid;
    private RectTransform rectTransform;

    private void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();

        UpdateCellSize();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (grid == null || rectTransform == null)
        {
            return;
        }

        UpdateCellSize();
    }

    private void UpdateCellSize()
    {
        const int columnCount = 2;

        float availableWidth =
            rectTransform.rect.width
            - grid.padding.left
            - grid.padding.right
            - grid.spacing.x * (columnCount - 1);

        float cellWidth =
            availableWidth / columnCount;

        grid.cellSize =
            new Vector2(cellWidth, cellHeight);
    }
}