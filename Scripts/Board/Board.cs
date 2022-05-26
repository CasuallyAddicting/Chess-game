using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Board : MonoBehaviour
{
    private float CanvasWidth;
    private float CanvasHeight;

    public PieceManager PieceManager;

    public GameObject CellPrefab;

    [HideInInspector]
    public Cell[,] AllCells = new Cell[8, 8];

    public List<Cell> YellowHighlightedCells;
    [HideInInspector]
    public Cell OrangeHighlightedCell;

    private void Awake()
    {
        RectTransform canvasRectTransform = GameObject.Find("Body").GetComponent<RectTransform>();
        float CanvasScale = GameObject.Find("Parent Canvas").GetComponent<RectTransform>().rect.height / 1334;
        CanvasWidth = canvasRectTransform.rect.width / CanvasScale;
        CanvasHeight = 1086;
    }

    public void Create()
    {
        #region Create
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                // Create the cell
                GameObject newCell = Instantiate(CellPrefab, transform);

                // Position
                RectTransform rectTransform = newCell.GetComponent<RectTransform>();

                float cellWidth = (CanvasWidth / 8);

                float posX = x * cellWidth + cellWidth / 2;
                float posY = y * cellWidth + cellWidth / 2 + (CanvasHeight - CanvasWidth) / 2;

                rectTransform.anchoredPosition = new Vector2(posX, posY);

                // Scale
                rectTransform.localScale = new Vector3(CanvasWidth / 800, CanvasWidth / 800, 1);

                // Setup
                AllCells[x, y] = newCell.GetComponent<Cell>();
                AllCells[x, y].Setup(new Vector2Int(x, y), this);
            }
        }
        #endregion

        #region Colour
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                // Colour
                AllCells[x, y].GetComponent<Image>().color = (x + y) % 2 == 0 ?  new Color32(181, 136, 99, 255) : new Color32(240, 217, 181, 255);
            }
        }
        #endregion
    }

    public CellState ValidateCell(int _targetX, int _targetY, BasePiece _checkingPiece)
    {
        // Bounds check
        if (_targetX < 0 || _targetX > 7) return CellState.OutOfBounds;
        if (_targetY < 0 || _targetY > 7) return CellState.OutOfBounds;

        // Get cell
        Cell targetCell = AllCells[_targetX, _targetY];

        try
        {
            // If the cell has a piece
            if (targetCell.CurrentPiece != null)
            {
                // If friendly
                if (_checkingPiece.Colour == targetCell.CurrentPiece.Colour) return CellState.Friendly;

                // If enemy
                if (_checkingPiece.Colour != targetCell.CurrentPiece.Colour) return CellState.Enemy;
            }
        }
        catch { }
        return CellState.Free;
    }

    public void Highlight(List<Cell> _cells, string _colour)
    {
        if (_colour == "orange")
        {
            if (OrangeHighlightedCell != null) OrangeHighlightedCell.UnHighlight();
            _cells[0].Highlight("orange");
            OrangeHighlightedCell = _cells[0];
        }
        else
        {
            if (YellowHighlightedCells != null && YellowHighlightedCells.Count == 2)
            {
                YellowHighlightedCells[0].UnHighlight();
                YellowHighlightedCells[1].UnHighlight();
            }
            _cells[0].Highlight("yellow");
            _cells[1].Highlight("yellow");

            YellowHighlightedCells = _cells;
        }
    }
}
