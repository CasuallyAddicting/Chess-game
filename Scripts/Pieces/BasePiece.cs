using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

public enum CellState
{
    None,
    Friendly,
    Enemy,
    Free,
    OutOfBounds
}

public class BasePiece : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    System.Random Rnd = new System.Random();

    protected Sprite[] PiecesImages;

    [HideInInspector]
    public int Moves { get; protected set; } = 0;
    [HideInInspector]
    public Color Colour { get; protected set; } = Color.clear;
    [HideInInspector]
    public string TrueIdentity { get; protected set; }
    [HideInInspector]
    public Color TrueTeamColour { get; protected set; } = Color.clear;
    [HideInInspector]
    public int FirstStepCount { get; protected set; }

    protected Cell OriginalCell = null;
    public Cell CurrentCell = null;

    protected RectTransform RectTransform = null;
    protected PieceManager PieceManager;

    protected Cell TargetCell = null;

    protected Vector3Int Movement = Vector3Int.one;
    protected List<Cell> HighlightedCells = new List<Cell>();
    protected List<Cell> LegalCells = new List<Cell>();

    public int Index = 100;

    protected float CanvasWidth;

    private bool RemoveSelected = false;

    public virtual void Setup(Color _newTeamColour, Color32 _newSpriteColour, PieceManager _newPieceManager, string _trueIdentity, Color _trueTeamColour, bool _useBetterSprite = false)
    {
        RectTransform canvasRectTransform = GameObject.Find("Body").GetComponent<RectTransform>();
        CanvasWidth = canvasRectTransform.rect.width;

        PieceManager = _newPieceManager;

        // If it is first batch, can do EnPassant or Castling
        Moves = PieceManager.Moves;
        Colour = _newTeamColour;
        TrueIdentity = _trueIdentity;
        TrueTeamColour = _trueTeamColour;

        if (!_useBetterSprite)
        {
            int Alpha = (Moves > 0 ? 255 : 120);
            GetComponent<Image>().color = new Color32(_newSpriteColour.r, _newSpriteColour.g, _newSpriteColour.b, (byte)Alpha);
        }

        RectTransform = GetComponent<RectTransform>();
        PiecesImages = Resources.LoadAll<Sprite>($"PiecesImage");

        if (Moves != 0) FirstStepCount = PieceManager.Moves;
    }

    public void SetNewIdentity(string _trueIdentity, Color _trueTeamColour)
    {
        TrueIdentity = _trueIdentity;
        TrueTeamColour = _trueTeamColour;
    }

    public virtual void Place(Cell _newCell)
    {
        // Cell Stuff
        CurrentCell = _newCell;
        OriginalCell = _newCell;
        CurrentCell.CurrentPiece = this;

        // Object Stuff
        transform.position = _newCell.transform.position;
        gameObject.SetActive(true);
    }

    public virtual void Reset()
    {
        Kill();

        Place(OriginalCell);

        Moves = 0;
    }

    public virtual void Kill()
    {
        // Clear current cell
        CurrentCell.CurrentPiece = null;

        // Remove piece
        if (gameObject != null)
        {
            gameObject.SetActive(false);
        }

        if (TrueIdentity == "K" && this.enabled == false) PieceManager.IsKingAlive = false;
    }

    public void CheckForPromotion()
    {
        // Target position
        int currentX = CurrentCell.BoardPosition.x;
        int currentY = CurrentCell.BoardPosition.y;

        CellState cellstate = CurrentCell.Board.ValidateCell(currentX, currentY + Movement.y, this);

        if (cellstate == CellState.OutOfBounds)
        {
            Color spriteColor = GetComponent<Image>().color;

            string[] randomPieces = { "P", "R", "N", "B", "Q" };
            string ramdomPiece = randomPieces[Rnd.Next(randomPieces.Length)];

            PieceManager.ChangePiece(this, CurrentCell, ramdomPiece, Colour, spriteColor);
        }
    }

    protected virtual void ShowTrueIdentity()
    {
        if (this.enabled && GetComponent<Image>().color.a < 1)
        {
            PieceManager.ChangePiece(this, CurrentCell, TrueIdentity, TrueTeamColour, (Color32)TrueTeamColour);
        }
    }

    #region Movement

    public bool HaveMove()
    {
        HighlightedCells.Clear();
        LegalCells.Clear();

        // Test for cells
        CheckPathing();
        CheckLegalMoves();

        if (LegalCells.Count > 0) return true;
        return false;
    }

    private void CreateCellPath(int _xDirection, int _yDirection, int _movement)
    {
        // Targeted position
        int currentX = CurrentCell.BoardPosition.x;
        int currentY = CurrentCell.BoardPosition.y;

        // Check each cell
        for (int i = 1; i <= _movement; i++)
        {
            currentX += _xDirection;
            currentY += _yDirection;

            // Get the state of target cell
            CellState cellState;
            cellState = CurrentCell.Board.ValidateCell(currentX, currentY, this);

            // If enemy, add to list, break
            if (cellState == CellState.Enemy)
            {
                HighlightedCells.Add(CurrentCell.Board.AllCells[currentX, currentY]);
                break;
            }

            if (cellState != CellState.Free) break;

            // Add to list
            HighlightedCells.Add(CurrentCell.Board.AllCells[currentX, currentY]);
        }
    }

    protected virtual void CheckPathing()
    {
        // Horizontal
        CreateCellPath(1, 0, Movement.x);
        CreateCellPath(-1, 0, Movement.x);

        // Vertical
        CreateCellPath(0, 1, Movement.y);
        CreateCellPath(0, -1, Movement.y);

        // Upper diagonal
        CreateCellPath(1, 1, Movement.z);
        CreateCellPath(-1, 1, Movement.z);

        // Lower diagonal
        CreateCellPath(-1, -1, Movement.z);
        CreateCellPath(1, -1, Movement.z);
    }

    private BasePiece GetRealKing()
    {
        foreach (BasePiece piece in PieceManager.ChangedPieces)
        {
            // Ignore if different team
            if (piece.Colour != Colour) continue;
            if (piece.GetType() != typeof(King)) continue;

            return piece;
        }
        return null;
    }

    public bool KingSafe(Cell _highlightedCell = null)
    {
        Board board = CurrentCell.Board;
        Vector2Int highlightedPosition = _highlightedCell.BoardPosition;

        // If highlighted have enemy king, return true
        if (board.ValidateCell(highlightedPosition.x, highlightedPosition.y, this) == CellState.Enemy)
        {
            if (_highlightedCell.CurrentPiece.GetType() == typeof(King))
            {
                if (_highlightedCell.CurrentPiece.GetComponent<Image>().color.a == 1) return true;
            }
        }

        // Give a random value
        Vector2Int orgin = Vector2Int.one;

        // If this is real king, set orgin to highlighted cell
        // else set orgin to real king's position
        if (GetType() == typeof(King) && GetComponent<Image>().color.a == 1)
        {
            orgin = _highlightedCell.BoardPosition;
        }
        else
        {
            //error script
            // Get Real king
            foreach (BasePiece piece in PieceManager.ChangedPieces)// Every piece in [ ChangedPieces ] showed its idenitty
            {
                // If in same team
                if (piece.Colour == Colour && piece.GetType() == typeof(King))
                {
                    orgin = piece.CurrentCell.BoardPosition;
                    break;
                }
            }
        }

        #region Add directions
        List<Vector2Int> directions = new List<Vector2Int>
        {

            // 8 directions
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(1, 0),
            new Vector2Int(1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, 1),

            // Knight Directions
            new Vector2Int(1, 2),
            new Vector2Int(2, 1),
            new Vector2Int(2, -1),
            new Vector2Int(1, -2),
            new Vector2Int(-1, -2),
            new Vector2Int(-2, -1),
            new Vector2Int(-2, 1),
            new Vector2Int(-1, 2)
        };
        #endregion

        #region Normal 8 directions
        // Loop through 8 directions
        for (int i = 0; i < 8; i++)
        {
            // Set direction
            Vector2Int direction = directions[i];

            // Loop forward
            for (int j = 1; j < 8; j++)
            {
                Vector2Int targetPosition = orgin + (direction * j);
                CellState cellstate = CurrentCell.Board.ValidateCell(targetPosition.x, targetPosition.y, this);

                if (cellstate == CellState.OutOfBounds) break; // If theres no cell, switch direction                   |Out Of Bounds

                Cell targetCell = board.AllCells[targetPosition.x, targetPosition.y];
                BasePiece targetedPiece = targetCell.CurrentPiece;

                if (targetedPiece == this) continue; // If [this] is on the target cell, ignore and go forward          |Free
                if (targetCell == _highlightedCell) break; // If the cell is the highlighted cell, switch direction     |Friendly
                if (cellstate == CellState.Friendly) break; // If the piece in the cell is friendly, switch direction   |Friendly
                if (cellstate == CellState.Free) continue; // If theres no piece in the cell, go forward                |Free

                if (cellstate == CellState.Enemy) // If the piece on the target cell is enemy                           |Enemy
                {
                    if (targetedPiece.GetType() == typeof(Queen)) return false;

                    if (targetedPiece.GetType() == typeof(King) && j == 1) return false; // distance is 1

                    if (targetedPiece.GetType() == typeof(Bishop) && (i % 2 == 1)) return false; // >< directions

                    if (targetedPiece.GetType() == typeof(Rook) && (i % 2 == 0)) return false; // + directions

                    if (targetedPiece.GetType() == typeof(Pawn))
                    {
                        // If direction is + , friendly
                        if (i % 2 == 0) break;

                        if (j > 1) break;

                        if (targetedPiece.Movement.y > 0 && (i == 3 || i == 5)) return false;
                        if (targetedPiece.Movement.y < 0 && (i == 7 || i == 1)) return false;
                    }

                    break; // Break because enemy blocked the path
                }
            }
        }
        #endregion

        #region Knight 8 directions
        for (int i = 8; i < 16; i++)
        {
            Vector2Int direction = directions[i];
            Vector2Int targetPosition = orgin + direction;
            CellState cellstate = board.ValidateCell(targetPosition.x, targetPosition.y, this);

            if (cellstate == CellState.Enemy)
            {
                Cell targetCell = board.AllCells[targetPosition.x, targetPosition.y];
                if (targetCell == _highlightedCell) continue; // If the enemy's position is where we going
                if (targetCell.CurrentPiece.GetType() == typeof(Knight))
                {
                    return false;
                }
            }
        }
        #endregion

        return true;
    }

    protected void CheckLegalMoves()
    {
        BasePiece king = GetRealKing();

        // If king's identity haven't showed
        if (king == null)
        {
            LegalCells = HighlightedCells;
            return;
        }

        List<Cell> legalMoves = new List<Cell>();
        // Loop through all highlighted cells and set Legal moves
        foreach (Cell highlightedCell in HighlightedCells)
        {
            if (KingSafe(highlightedCell)) legalMoves.Add(highlightedCell);
        }
        LegalCells = legalMoves;
    }

    protected void ShowCells()
    {
        foreach (Cell cell in LegalCells)
        {
            cell.Dot.enabled = true;
        }
    }

    public void ClearCells()
    {
        foreach (Cell cell in LegalCells)
        {
            cell.Dot.enabled = false;
        }

        LegalCells.Clear();
        HighlightedCells.Clear();
    }

    protected virtual void Move()
    {
        // If there is enemy piece, remove it
        TargetCell.RemovePiece();

        // Clear current
        CurrentCell.CurrentPiece = null;

        // Switch cell
        CurrentCell = TargetCell;
        CurrentCell.CurrentPiece = this;

        // Move on board
        transform.position = CurrentCell.transform.position;
        TargetCell = null;

        PieceManager.Moves++;
        Moves++;

        if (Moves == 1) FirstStepCount = PieceManager.Moves;
    }

    public void MoveTo (Vector3 _position)
    {
        // Check for overlapping avaliable squares
        foreach (Cell cell in LegalCells)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(cell.RectTransform, Input.mousePosition))
            {
                // If the mose is within a valid cell, get it, and break.
                TargetCell = cell;
                break;
            }

            // If the mouse is not within any highlighted cell, we don't have a valid move.
            TargetCell = null;
        }

        Cell originalCell = CurrentCell;

        // If cell not highlighted
        if (!TargetCell)
        {
            // Return to original position
            transform.position = CurrentCell.gameObject.transform.position;

            foreach (Cell cell in CurrentCell.Board.AllCells)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(cell.RectTransform, _position))
                {
                    // Check if move is allowed
                    if (!RectTransformUtility.RectangleContainsScreenPoint((RectTransform)CurrentCell.gameObject.transform, _position))
                    {
                        PieceManager.GameManager.ShowNotifs("Invalid Move", "This move is not allowed!", Colour == Color.white ? 0 : 180);
                    }
                    return;
                }
            }

            CurrentCell.UnHighlight();

            List<Cell> yellowHighlightedCells = CurrentCell.Board.YellowHighlightedCells;
            if (yellowHighlightedCells != null && yellowHighlightedCells.Count == 2)
            {
                yellowHighlightedCells[0].Highlight("yellow");
                yellowHighlightedCells[1].Highlight("yellow");
            }

            ClearCells();
            PieceManager.SelectedPiece = null;
            return;
        }

        // Move to new cell
        Move();

        // Hide
        ClearCells();

        ShowTrueIdentity();

        // End turn
        PieceManager.SwitchSides(Colour);

        OriginalCell.UnHighlight();

        CurrentCell.Board.OrangeHighlightedCell = null;
        PieceManager.SelectedPiece = null;

        List<Cell> toHighlight = new List<Cell>
        {
            CurrentCell,
            originalCell
        };

        CurrentCell.Board.Highlight(toHighlight, "yellow");
    }
    #endregion

    #region Events

    public void OnDrag(PointerEventData _eventData)
    {

        // Follow cursor
        transform.position += (Vector3)_eventData.delta;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        List<Cell> toHighlight = new List<Cell>();
        toHighlight.Add(CurrentCell);

        if (PieceManager.SelectedPiece != this)
        {
            CurrentCell.Board.Highlight(toHighlight, "orange");

            HighlightedCells.Clear();
            LegalCells.Clear();

            // Test for cells
            CheckPathing();
            CheckLegalMoves();

            // Show valid cells
            ShowCells();

            RemoveSelected = false;
        }
        else
        {
            RemoveSelected = true;
        }
    }

    public void OnPointerUp (PointerEventData eventData)
    {
        if (PieceManager.SelectedPiece != this) return;

        if (RemoveSelected == true)
        {
            if (transform.position == CurrentCell.transform.position)
            {
                CurrentCell.UnHighlight();

                List<Cell> yellowHighlightedCells = CurrentCell.Board.YellowHighlightedCells;
                if (yellowHighlightedCells != null && yellowHighlightedCells.Count == 2)
                {
                    yellowHighlightedCells[0].Highlight("yellow");
                    yellowHighlightedCells[1].Highlight("yellow");
                }

                ClearCells();
                PieceManager.SelectedPiece = null;
                return;
            }
        }

        if (RectTransformUtility.RectangleContainsScreenPoint(CurrentCell.RectTransform, Input.mousePosition))
        {
            transform.position = CurrentCell.gameObject.transform.position;
            return;
        }

        PieceManager.SelectedPiece.MoveTo(Input.mousePosition);
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (PieceManager.SelectedPiece != this) return;

        if (GameObject.Find("PF_Notifs(Clone)") != null) return;
        if (GameObject.Find("PF_Confirmations(Clone)") != null) return;

        PieceManager.SelectedPiece.MoveTo(Input.mousePosition);
    }
    #endregion
}
