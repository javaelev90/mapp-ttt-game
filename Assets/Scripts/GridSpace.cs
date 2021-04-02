using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridSpace : MonoBehaviour
{
    public Button button;
    public TMP_Text buttonText;
    public string playerSide;

    private GameController gameController;

    public void SetGameControllerReference (GameController controller)
    {
        gameController = controller;
    }
    public void SetSpace2 ()
    {
        buttonText.text = gameController.GetPlayerSide();
        button.interactable = false;
        gameController.EndTurn();
    }

    public void SetSpace ()
    {
        buttonText.text = gameController.GetPlayerSide();
        buttonText.color = new Color32(0,0,0,0);
        GetComponent<Image>().sprite = gameController.GetPlayerSideIcon();
        button.interactable = false;
        gameController.EndTurn();
    }
}
