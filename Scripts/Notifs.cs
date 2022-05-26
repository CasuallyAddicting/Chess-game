using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Notifs : MonoBehaviour
{
    [SerializeReference] GameObject Displayer1;
    [SerializeReference] GameObject Displayer2;

    private GameManager GameManager;

    public void Show(string _display1, string _display2, GameManager _gameManager, float _rotation)
    {
        Displayer1.GetComponent<TextMeshProUGUI>().text = _display1;
        Displayer2.GetComponent<TextMeshProUGUI>().text = _display2;

        gameObject.transform.rotation = new Quaternion(0, 0, _rotation, 0);

        gameObject.SetActive(true);

        GameManager = _gameManager;
    }

    public void ConfirmationClicked()
    {
        string title = Displayer1.GetComponent<TextMeshProUGUI>().text;

        switch (title)
        {
            case "Restart Game":
                GameManager.ResetGame();
                break;
        }

    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
