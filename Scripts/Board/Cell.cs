using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    public Image Dot;

    [HideInInspector]
    public Vector2Int BoardPosition = Vector2Int.zero;
    [HideInInspector]
    public Board Board = null;
    [HideInInspector]
    public RectTransform RectTransform = null;

    [HideInInspector]
    public BasePiece CurrentPiece = null;

    private Color OriginalColour = Color.clear;

    public void Setup(Vector2Int _newBoardPosition, Board _newBoard)
    {
        BoardPosition = _newBoardPosition;
        Board = _newBoard;

        RectTransform = GetComponent<RectTransform>();
    }

    public void Highlight(string _colour)
    {
        if (OriginalColour == Color.clear) OriginalColour = GetComponent<Image>().color;

        if (_colour == "orange")
        {
            GetComponent<Image>().color = (BoardPosition.x + BoardPosition.y) % 2 == 0 ? new Color32(191, 121, 69, 255) : new Color32(233, 177, 126, 255); // dark, light
            Board.PieceManager.SelectedPiece = CurrentPiece;
        }
        else
        {
            GetComponent<Image>().color = (BoardPosition.x + BoardPosition.y) % 2 == 0 ? new Color32(169, 162, 58, 255) : new Color32(204, 209, 106, 255); // dark, light
        }
    }

    public void UnHighlight()
    {
        GetComponent<Image>().color = OriginalColour;
        if (CurrentPiece != null) CurrentPiece.ClearCells();
    }

    public void RemovePiece()
    {
        if (CurrentPiece != null)
        {
            if (CurrentPiece.TrueTeamColour == Color.white)
            {
                Board.PieceManager.WhitePiecesDisplayer.GetComponent<TMPro.TextMeshProUGUI>().text += CurrentPiece.TrueIdentity;
            }
            else
            {
                Board.PieceManager.BlackPiecesDisplayer.GetComponent<TMPro.TextMeshProUGUI>().text += CurrentPiece.TrueIdentity;
            }
            CurrentPiece.Kill();
        }
    }
}