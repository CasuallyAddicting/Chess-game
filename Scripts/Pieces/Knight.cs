using UnityEngine.UI;
using UnityEngine;

public class Knight : BasePiece
{
    public override void Setup(Color _newTeamColour, Color32 _newSpriteColour, PieceManager _newPieceManager, string _trueIdentity, Color _trueTeamColour, bool _useBetterSprite = false)
    {
        // Base setup
        base.Setup(_newTeamColour, _newSpriteColour, _newPieceManager, _trueIdentity, _trueTeamColour, _useBetterSprite);

        // Knight stuff
        Movement = new Vector3Int(7, 7, 0);

        if (_useBetterSprite)
        {
            GetComponent<Image>().sprite = PiecesImages[2 + (_newTeamColour == Color.white ? 6 : 0)];
        }
        else
        {
            GetComponent<Image>().sprite = Resources.Load<Sprite>("Knight");
        }
        
    }

    private void CreateCellPath(int _flipper)
    {
        // Target position
        int currentX = CurrentCell.BoardPosition.x;
        int currentY = CurrentCell.BoardPosition.y;

        // Left
        MatchesState(currentX - 2, currentY + (1 * _flipper));

        // Upper left
        MatchesState(currentX - 1, currentY + (2 * _flipper));

        // Upper right
        MatchesState(currentX + 1, currentY + (2 * _flipper));

        // Right
        MatchesState(currentX + 2, currentY + (1 * _flipper));
    }

    protected override void CheckPathing()
    {
        // Draw top half
        CreateCellPath(1);

        // Draw bottom half
        CreateCellPath(-1);
    }

    private void MatchesState(int _targetX, int _targetY)
    {
        CellState cellState = CellState.None;
        cellState = CurrentCell.Board.ValidateCell(_targetX, _targetY, this);

        if (cellState != CellState.Friendly && cellState != CellState.OutOfBounds)
        {
            HighlightedCells.Add(CurrentCell.Board.AllCells[_targetX, _targetY]);
        }
    }
}
