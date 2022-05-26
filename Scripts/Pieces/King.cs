using UnityEngine.UI;
using UnityEngine;

public class King : BasePiece
{
    private Rook LeftRook = null;
    private Rook RightRook = null;

    public override void Setup(Color _newTeamColour, Color32 _newSpriteColour, PieceManager _newPieceManager, string _trueIdentity, Color _trueTeamColour, bool _useBetterSprite = false)
    {
        // Base setup
        base.Setup(_newTeamColour, _newSpriteColour, _newPieceManager, _trueIdentity, _trueTeamColour, _useBetterSprite);

        // King stuff
        Movement = new Vector3Int(1, 1, 1);

        if (_useBetterSprite)
        {
            GetComponent<Image>().sprite = PiecesImages[1 + (_newTeamColour == Color.white ? 6 : 0)];
        }
        else
        {
            GetComponent<Image>().sprite = Resources.Load<Sprite>("King");
        }
        
    }

    protected override void CheckPathing()
    {
        base.CheckPathing();

        // Left
        LeftRook = GetRook(-1, 4);

        // Right
        RightRook = GetRook(1, 3);
    }

    protected override void Move()
    {
        base.Move();

        // Left
        if (CanCastle(LeftRook)) LeftRook.Castle();

        // Right
        if (CanCastle(RightRook)) RightRook.Castle();
    }

    private bool CanCastle(Rook _rook)
    {
        if (_rook == null) return false;

        if (_rook.CastleTriggerCell != CurrentCell) return false;

        return true;
    }

    private Rook GetRook(int _direction, int _count)
    {
        // Has the king moved?
        if (Moves > 0) return null;

        // Position
        int currentX = CurrentCell.BoardPosition.x;
        int currentY = CurrentCell.BoardPosition.y;

        // Go through the cells
        for (int i = 1; i < _count; i++)
        {
            int offsetX = currentX + (i * _direction);
            CellState cellstate = CurrentCell.Board.ValidateCell(offsetX, currentY, this);

            if (cellstate != CellState.Free) return null;
        }

        // Try and get rook
        Cell rookCell = CurrentCell.Board.AllCells[currentX + (_count * _direction), currentY];
        Rook rook = null;

        // Cast
        if (rookCell.CurrentPiece != null)
        {
            if (rookCell.CurrentPiece is Rook) rook = (Rook)rookCell.CurrentPiece;
        }

        // Return if no rook
        if (rook == null) return null;

        // Check colour and movement
        if (rook.Colour != Colour || rook.Moves > 0) return null;

        // Add castle trigger to movement
        HighlightedCells.Add(rook.CastleTriggerCell);

        return rook;
    }
}
