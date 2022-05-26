using UnityEngine.UI;
using UnityEngine;

public class Bishop : BasePiece
{
    public override void Setup(Color _newTeamColour, Color32 _newSpriteColour, PieceManager _newPieceManager, string _trueIdentity, Color _trueTeamColour, bool _useBetterSprite = false)
    {
        // Base setup
        base.Setup(_newTeamColour, _newSpriteColour, _newPieceManager, _trueIdentity, _trueTeamColour, _useBetterSprite);

        // Bishop stuff
        Movement = new Vector3Int(0, 0, 7);

        if (_useBetterSprite)
        {
            GetComponent<Image>().sprite = PiecesImages[0 + (_newTeamColour == Color.white ? 6 : 0)];
        }
        else
        {
            GetComponent<Image>().sprite = Resources.Load<Sprite>("Bishop");
        }
    }
}
