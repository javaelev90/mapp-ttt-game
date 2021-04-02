using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridSpace : MonoBehaviour
{
    public Button button;
    public string buttonText;
    public string playerSide;

    private GameController gameController;

    public void SetGameControllerReference (GameController controller)
    {
        gameController = controller;
    }
    public void SetSpace2 ()
    {
        // buttonText.text = gameController.GetPlayerSide();
        button.interactable = false;
        gameController.EndTurn();
    }

    public void SetSpace ()
    {
        playerSide = gameController.GetPlayerSide();
        GetComponent<Image>().sprite = gameController.GetPlayerSideIcon();
        GetComponent<Image>().color = new Color32(255,255,255,255);
        button.interactable = false;
        gameController.EndTurn();
    }
}
