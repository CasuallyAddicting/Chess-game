using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using System;

public class PieceManager : EventTrigger
{
    System.Random Rnd = new System.Random();

    [HideInInspector]
    public bool IsKingAlive = true;

    public GameManager GameManager;

    [HideInInspector]
    public int Moves = 0;

    [SerializeReference]
    private GameObject PiecePrefab;

    public List<BasePiece> WhitePieces { get; private set; } = new List<BasePiece>();
    public List<BasePiece> BlackPieces { get; private set; } = new List<BasePiece>();
    [HideInInspector]
    public List<BasePiece> ChangedPieces = new List<BasePiece>();

    public GameObject BlackPiecesDisplayer;
    public GameObject WhitePiecesDisplayer;

    public BasePiece SelectedPiece;

    private string[] PieceOrder = new string[16]
    {
        "P", "P", "P", "P", "P", "P", "P", "P",
        "R", "N", "B", "Q", "K", "B", "N", "R"
    };

    private Dictionary<string, Type> PieceLibrary = new Dictionary<string, Type>()
    {
        {"P",  typeof(Pawn)},
        {"R",  typeof(Rook)},
        {"N", typeof(Knight)},
        {"B",  typeof(Bishop)},
        {"K",  typeof(King)},
        {"Q",  typeof(Queen)}
    };
    public void Setup(Board _board)
    {
        List<BasePiece> allPieces = CreatePieces(_board);

        List<BasePiece> firstHalf = new List<BasePiece>();
        List<BasePiece> secondHalf = new List<BasePiece>();

        for (int i = 0; i < 32; i++)
        {
            (i < 16 ? firstHalf : secondHalf).Add(allPieces[i]);
            (allPieces[i].Colour == Color.white ? WhitePieces : BlackPieces).Add(allPieces[i]);
        }

        // Place pieces
        PlacePieces(1, 0, firstHalf, _board);
        PlacePieces(6, 7, secondHalf, _board);

        // White goes first
        SwitchSides(Color.black);

        WhitePiecesDisplayer.SetActive(false);
        BlackPiecesDisplayer.SetActive(false);
        WhitePiecesDisplayer.GetComponent<TMPro.TextMeshProUGUI>().text = "";
        BlackPiecesDisplayer.GetComponent<TMPro.TextMeshProUGUI>().text = "";
    }

    private BasePiece CreatePiece(Type _pieceType)
    {
        RectTransform canvasRectTransform = GameObject.Find("Body").GetComponent<RectTransform>();
        float CanvasWidth = canvasRectTransform.rect.width;

        // Create new object
        GameObject newPieceObject = Instantiate(PiecePrefab);
        newPieceObject.transform.SetParent(transform);

        // Set scale and position
        newPieceObject.transform.localScale = new Vector3(CanvasWidth / 800, CanvasWidth / 800, 1);
        newPieceObject.transform.localRotation = Quaternion.identity;

        BasePiece newPiece = (BasePiece)newPieceObject.AddComponent(_pieceType);

        return newPiece;
    }

    private List<BasePiece> CreatePieces(Board _board)
    {
        List<BasePiece> newPieces = new List<BasePiece>();

        List<string> remainingPieces = new List<string>();
        List<Color> colours = new List<Color>();

        for (int i = 0; i < PieceOrder.Length * 2; i++)
        {
            remainingPieces.Add(PieceOrder[i % PieceOrder.Length]);
            colours.Add(i < 16 ? Color.black : Color.white);
        }


        for (int i = 0; i < PieceOrder.Length * 2; i++)
        {
            // Get the type, apply  to new object
            string key = PieceOrder[i % PieceOrder.Length];
            Type pieceType = PieceLibrary[key];

            // Store new piece
            BasePiece newPiece = CreatePiece(pieceType);
            newPieces.Add(newPiece);

            // Set True identity and color
            int Index = Rnd.Next(remainingPieces.Count);

            string trueIdentity = remainingPieces[Index];
            Color trueTeamColour = colours[Index];

            remainingPieces.RemoveAt(Index);
            colours.RemoveAt(Index);

            // Setup 
            newPiece.Setup((i < 16 ? Color.white : Color.black), (Color32)(i < 16 ? Color.white : Color.black), this, trueIdentity, trueTeamColour);
        }

        return newPieces;
    }

    private void PlacePieces(int _pawnRow, int _royaltyRow, List<BasePiece> _pieces, Board _board)
    {
        for (int i = 0; i < 8; i++)
        {
            // Place pawns
            _pieces[i].Place(_board.AllCells[i, _pawnRow]);

            // Place royalty
            _pieces[i + 8].Place(_board.AllCells[i, _royaltyRow]);
        }
    }

    public void SetInteractive(List<BasePiece> _allPieces, bool _value)
    {
        foreach (BasePiece piece in _allPieces)
        {
            piece.enabled = _value;
        }
    }

    public void RotatePieces(List<BasePiece> _allPieces, float _rotation)
    {
        foreach (BasePiece piece in _allPieces)
        {
            piece.gameObject.transform.rotation = new Quaternion(0, 0, _rotation, 0);
        }
    }

    private bool HaveMove(Color _team)
    {
        foreach (BasePiece piece in (_team == Color.white ? WhitePieces : BlackPieces))
        {
            if (!piece.gameObject.activeSelf) continue;

            if (piece.HaveMove()) return true;
        }

        foreach (BasePiece piece in ChangedPieces)
        {
            if (!piece.gameObject.activeSelf || piece.Colour != _team) continue;

            if (piece.HaveMove()) return true;
        }

        return false;
    }

    private void GameOver(string _display1, string _display2)
    {
        GameManager.GameOver(_display1, _display2);

        WhitePiecesDisplayer.SetActive(true);
        BlackPiecesDisplayer.SetActive(true);
    }

    public void SwitchSides(Color _colour)
    {
        if (!IsKingAlive)
        {
            string display1 = _colour == Color.white ? "1 - 0" : "0 - 1";
            string display2 = (_colour == Color.white ? "Black" : "White") + " king died.";
            GameOver(display1, display2);
            return;
        }

        if (!HaveMove(_colour == Color.white ? Color.black : Color.white))
        {
            string display1;
            string display2;
            foreach (BasePiece piece in ChangedPieces)
            {
                if (piece.Colour == _colour) continue;
                if (piece.GetType() != typeof(King)) continue;

                if (piece.KingSafe(piece.CurrentCell))
                {
                    display1 = "1/2 - 1/2";
                    display2 = "Stalemate";
                    GameOver(display1, display2);
                    return;
                }

                display1 = _colour == Color.white ? "1 - 0" : "0 - 1";
                display2 = "Checkmate " + (_colour == Color.white ? "White" : "Black") + " is victorious.";
                GameOver(display1, display2);
                return;
            }
        }

        bool isBlackTurn = (_colour == Color.white ? true : false);

        // Set interactivity
        SetInteractive(WhitePieces, !isBlackTurn);
        SetInteractive(BlackPieces, isBlackTurn);

        // Set changed interactivity
        foreach (BasePiece piece in ChangedPieces)
        {
            bool isBlackPiece = (piece.Colour != Color.white ? true : false);
            bool isPartOfTeam = (isBlackPiece == true ? isBlackTurn : !isBlackTurn);

            piece.enabled = isPartOfTeam;

        }

        float rotation = isBlackTurn ? 180 : 0;

        RotatePieces(WhitePieces, rotation);
        RotatePieces(BlackPieces, rotation);
        RotatePieces(ChangedPieces, rotation);
    }

    public void ResetGame()
    {
        // Reset pieces
        ResetPieces();

        // King has risen from dead
        IsKingAlive = true;

        // Reset rotation
        RotatePieces(WhitePieces, 0);
        RotatePieces(BlackPieces, 0);

        // Set interactive
        SetInteractive(WhitePieces, true);
        SetInteractive(BlackPieces, false);

        WhitePiecesDisplayer.SetActive(false);
        BlackPiecesDisplayer.SetActive(false);
        WhitePiecesDisplayer.GetComponent<TMPro.TextMeshProUGUI>().text = "";
        BlackPiecesDisplayer.GetComponent<TMPro.TextMeshProUGUI>().text = "";

        if (GameManager.Board.OrangeHighlightedCell) GameManager.Board.OrangeHighlightedCell.UnHighlight();

        List<Cell> YellowHighlightedCells = GameManager.Board.YellowHighlightedCells;
        if (YellowHighlightedCells == null && YellowHighlightedCells.Count != 2) return;
        
        YellowHighlightedCells[0].UnHighlight();
        YellowHighlightedCells[1].UnHighlight();

        GameManager.Board.YellowHighlightedCells = null;
    }

    private void ResetPieces()
    {
        // Clear everything in ChangedPieces
        foreach (BasePiece piece in ChangedPieces)
        {
            piece.Kill();

            Destroy(piece.gameObject);
        }
        ChangedPieces.Clear();

        //TODO
        #region Set new true identity and reset position
        List<string> remainingPieces = new List<string>();
        List<Color> colours = new List<Color>();

        for (int i = 0; i < PieceOrder.Length * 2; i++)
        {
            remainingPieces.Add(PieceOrder[i % PieceOrder.Length]);
            colours.Add(i < 16 ? Color.black : Color.white);
        }


        // Reset white
        for (int i = 0; i < WhitePieces.Count; i++)
        {
            BasePiece piece = WhitePieces[i];
            piece.Reset();

            // Set True identity and color
            int Index = Rnd.Next(remainingPieces.Count);

            string trueIdentity = remainingPieces[Index];
            Color trueTeamColour = colours[Index];

            remainingPieces.RemoveAt(Index);
            colours.RemoveAt(Index);

            piece.SetNewIdentity(trueIdentity, trueTeamColour);
        }

        // Reset black
        for (int i = 0; i < BlackPieces.Count; i++)
        {
            BasePiece piece = BlackPieces[i];
            piece.Reset();

            // Set True identity and color
            int Index = Rnd.Next(remainingPieces.Count);

            string trueIdentity = remainingPieces[Index];
            Color trueTeamColour = colours[Index];

            remainingPieces.RemoveAt(Index);
            colours.RemoveAt(Index);

            piece.SetNewIdentity(trueIdentity, trueTeamColour);
        }
        #endregion

        Moves = 0;
    }


    int ChangedPieceIndex = 0;
    public void ChangePiece(BasePiece _piece, Cell _cell, string _pieceType, Color _teamColour, Color _spriteColour)
    {
        // Kill Piece
        _piece.Kill();

        // Create
        BasePiece changedPiece = CreatePiece(PieceLibrary[_pieceType]);
        changedPiece.Setup(_teamColour, _spriteColour, this, _pieceType, _teamColour, true);

        // Set Index
        changedPiece.Index = ChangedPieceIndex;
        ChangedPieceIndex++;

        // Place
        changedPiece.Place(_cell);

        // Add
        ChangedPieces.Add(changedPiece);

        if (changedPiece.GetComponent<Image>().color.a == 1 && changedPiece.GetType() == typeof(Pawn)) changedPiece.CheckForPromotion();
    }
}