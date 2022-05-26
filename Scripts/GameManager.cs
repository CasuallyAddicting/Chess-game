using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Board Board;
    public PieceManager PieceManager;

    [SerializeReference] GameObject Notifs;
    [SerializeReference] GameObject Confirmation;

    public void SwitchScene(string _sceneName)
    {
        SceneManager.LoadScene(_sceneName);
    }

    public void ShowNotifs(string _display1, string _display2, float _rotation = 0)
    {
        GameObject newNotifs = Instantiate(Notifs, GameObject.Find("Parent Canvas").transform);
        newNotifs.GetComponent<Notifs>().Show(_display1, _display2, this, _rotation);
    }
    public void ShowConfirmation(string _display1, string _display2, float _rotation = 0)
    {
        GameObject newConfirmation = Instantiate(Confirmation, GameObject.Find("Parent Canvas").transform);
        newConfirmation.GetComponent<Notifs>().Show(_display1, _display2, this, _rotation);
    }

    public void RequestResetGame()
    {
        ShowConfirmation("Restart Game", "Are you sure ?");
    }

    public void ResetGame()
    {
        PieceManager.ResetGame();
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Over the board"))
        {
            Board.Create();

            PieceManager.Setup(Board);
        }
    }

    public void GameOver(string _display1, string _display2)
    {
        PieceManager.SetInteractive(PieceManager.WhitePieces, false);
        PieceManager.SetInteractive(PieceManager.BlackPieces, false);
        PieceManager.SetInteractive(PieceManager.ChangedPieces, false);

        ShowNotifs(_display1, _display2);
    }
}
