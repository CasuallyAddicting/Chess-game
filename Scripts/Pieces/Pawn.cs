using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Pawn : BasePiece
{
    private List<Cell> EnPassant = new List<Cell>();

    public override void Setup(Color _newTeamColour, Color32 _newSpriteColor, PieceManager _newPieceManager, string _trueIdentity, Color _trueTeamColour, bool _useBetterSprite = false)
    {
        // Base setup
        base.Setup(_newTeamColour, _newSpriteColor, _newPieceManager, _trueIdentity, _trueTeamColour, _useBetterSprite);

        EnPassant.Clear();

        // Pawn Stuff
        Movement = Colour == Color.white ? new Vector3Int(0, 1, 1) : new Vector3Int(0, -1, -1);

        if (_useBetterSprite)
        {
            GetComponent<Image>().sprite = PiecesImages[3 + (_newTeamColour == Color.white ? 6 : 0)];
        }
        else
        {
            GetComponent<Image>().sprite = Resources.Load<Sprite>("Pawn");
        }
    }

    protected override void Move()
    {
        base.Move();

        if (EnPassant.Contains(CurrentCell))
        {
            int x = CurrentCell.BoardPosition.x;
            int y = CurrentCell.BoardPosition.y - Movement.y;

            CurrentCell.Board.AllCells[x, y].CurrentPiece.Kill();
        }
    }

    private bool MatchesState(int targetX, int targetY, CellState targetState, bool _addToHighlitedCells)
    {
        CellState cellState = CurrentCell.Board.ValidateCell(targetX, targetY, this);

        if (cellState == targetState)
        {
            if (_addToHighlitedCells) HighlightedCells.Add(CurrentCell.Board.AllCells[targetX, targetY]);
            return true;
        }

        return false;
    }

    protected override void ShowTrueIdentity()
    {
        base.ShowTrueIdentity();

        CheckForPromotion();
    }

    protected override void CheckPathing()
    {
        EnPassant.Clear();

        // Target position
        int currentX = CurrentCell.BoardPosition.x;
        int currentY = CurrentCell.BoardPosition.y;

        // Top left
        MatchesState(currentX - Movement.z, currentY + Movement.z, CellState.Enemy, true);

        // Foward
        if (MatchesState(currentX, currentY + Movement.y, CellState.Free, true))
        {
            // If the first forward cell is free and current cell is first move, check for next
            if (Moves == 0)
            {
                MatchesState(currentX, currentY + (Movement.y * 2), CellState.Free, true);
            }
        }

        // Top right
        MatchesState(currentX + Movement.z, currentY + Movement.z, CellState.Enemy, true);

        #region En Passant
        if (currentY == 3.5 + (Movement.y * 0.5))
        {
            // Left
            if (MatchesState(currentX - 1, currentY, CellState.Enemy, false))
            {
                BasePiece targetedCell = CurrentCell.Board.AllCells[currentX - 1, currentY].CurrentPiece;
                if (targetedCell.GetType() == typeof(Pawn))
                {
                    if (targetedCell.FirstStepCount == PieceManager.Moves)
                    {
                        MatchesState(currentX - 1, currentY + Movement.y, CellState.Free, true);
                        MatchesState(currentX - 1, currentY + Movement.y, CellState.Enemy, true);
                        EnPassant.Add(CurrentCell.Board.AllCells[currentX - 1, currentY + Movement.y]);
                    }
                }
            }

            // Right
            if (MatchesState(currentX + 1, currentY, CellState.Enemy, false))
            {
                BasePiece targetedCell = CurrentCell.Board.AllCells[currentX + 1, currentY].CurrentPiece;
                if (targetedCell.GetType() == typeof(Pawn))
                {
                    if (targetedCell.FirstStepCount == PieceManager.Moves)
                    {
                        MatchesState(currentX + 1, currentY + Movement.y, CellState.Free, true);
                        MatchesState(currentX + 1, currentY + Movement.y, CellState.Enemy, true);
                        EnPassant.Add(CurrentCell.Board.AllCells[currentX + 1, currentY + Movement.y]);
                    }
                }
            }
        }
        #endregion 
    }
}
