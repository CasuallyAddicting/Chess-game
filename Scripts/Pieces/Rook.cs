using UnityEngine.UI;
using UnityEngine;

public class Rook : BasePiece
{
    [HideInInspector]
    public Cell CastleTriggerCell = null;
    [HideInInspector]
    public Cell CastleCell = null;

    public override void Setup(Color _newTeamColour, Color32 _newSpriteColour, PieceManager _newPieceManager, string _trueIdentity, Color _trueTeamColour, bool _useBetterSprite = false)
    {
        // Base setup
        base.Setup(_newTeamColour, _newSpriteColour, _newPieceManager, _trueIdentity, _trueTeamColour, _useBetterSprite);

        // Rook stuff
        Movement = new Vector3Int(7, 7, 0);

        if (_useBetterSprite)
        {
            GetComponent<Image>().sprite = PiecesImages[5 + (_newTeamColour == Color.white ? 6 : 0)];
        }
        else
        {
            GetComponent<Image>().sprite = Resources.Load<Sprite>("Rook");
        }
    }

    public override void Place(Cell _newCell)
    {
        base.Place(_newCell);

        // Trigger cell
        int triggerOffset = CurrentCell.BoardPosition.x < 4 ? 2 : -1;
        CastleTriggerCell = SetCell(triggerOffset);

        // Castle cell
        int castleOffset = CurrentCell.BoardPosition.x < 4 ? 3 : -2;
        CastleCell = SetCell(castleOffset);
    }

    public void Castle()
    {
        // Set new target
        TargetCell = CastleCell;

        // Move
        Move();

        ShowTrueIdentity();
    }

    private Cell SetCell(int _offset)
    {
        // New position
        Vector2Int newPosition = CurrentCell.BoardPosition;
        newPosition.x += _offset;

        // Return
        return CurrentCell.Board.AllCells[newPosition.x, newPosition.y];
    }
}
