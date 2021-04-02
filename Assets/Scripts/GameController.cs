using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[System.Serializable]
public class Player {
   public Image panel;
   public TMP_Text text;
   public Button button;
}

[System.Serializable]
public class PlayerColor {
   public Color panelColor;
   public Color textColor;
}

public class GameController : MonoBehaviour
{
    public TMP_Text[] buttonList;
    public GameObject gameOverPanel;
    public TMP_Text gameOverText;
    public GameObject restartButton;
    public Player playerX;
    public Player playerO;
    public PlayerColor activePlayerColor;
    public PlayerColor inactivePlayerColor;
    public GameObject startInfo;

    private string playerSide;
    private int moveCount;
    private string[,] map;
    private int BOARD_SIDE_LENGTH;

    void Awake ()
    {
        SetGameControllerReferenceOnButtons();
        gameOverPanel.SetActive(false);
        moveCount = 0;
        restartButton.SetActive(false);
    }

    void Start() {
        int sideLength = (int) Mathf.Sqrt(buttonList.Length);
        map = new string[sideLength,sideLength];
        BOARD_SIDE_LENGTH = sideLength;
    }
    void SetGameControllerReferenceOnButtons ()
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<GridSpace>().SetGameControllerReference(this);
        }
    }

    public void SetStartingSide (string startingSide)
    {
        playerSide = startingSide;
        if (playerSide == "X")
        {
            SetPlayerColors(playerX, playerO);
        } 
        else
        {
            SetPlayerColors(playerO, playerX);
        }

        StartGame();
    }

    void StartGame ()
    {
        SetBoardInteractable(true);
        SetPlayerButtons (false);
        startInfo.SetActive(false);
    }

    public string GetPlayerSide ()
    {
        return playerSide;
    }

    public void EndTurn ()
    {
        fillMap();
        moveCount++;
        
        if(hasPlayerWon()){
            GameOver(playerSide);
        } 
        else if (moveCount >= 9){
            GameOver("draw");
        } 
        else {
            ChangeSides();
        }

    }

    // Copies values from buttonList into a 2D array
    private void fillMap(){
        for(int i = 0; i < buttonList.Length; i++){
            int x = i % BOARD_SIDE_LENGTH;
            int y = i / BOARD_SIDE_LENGTH;            
            map[x, y] = buttonList[i].text;
        }
    }

    // Check if win condition has been fulfilled
    private bool hasPlayerWon(){
        
        // Since the map has equally long sides only one loop is needed
        for(int i = 0; i < BOARD_SIDE_LENGTH; i++){
            
            // Count values in columns
            if(countLine(new Vector2Int(i, 0), new Vector2Int(0, 1), 0) == BOARD_SIDE_LENGTH){
                return true;
            }
            // Count values in rows
            else if(countLine(new Vector2Int(0, i), new Vector2Int(1, 0), 0) == BOARD_SIDE_LENGTH){
                return true;
            }
        }
        // Count values in first diagonal
        if(countLine(new Vector2Int(0,0), new Vector2Int(1, 1), 0) == BOARD_SIDE_LENGTH){
            return true;
        }
        // Count values in second diagonal
        else if(countLine(new Vector2Int(map.GetLength(0)-1, 0), new Vector2Int(-1, 1), 0) == BOARD_SIDE_LENGTH){
            return true;
        } 

        return false;
    }

    // Count the playerSide(X or O) values in a line
    private int countLine(Vector2Int currentPosition, Vector2Int displacement, int comboCount){

        if(positionOutsideBounds(currentPosition.x, currentPosition.y)
            || comboCount == BOARD_SIDE_LENGTH){
            return comboCount;
        }

        int newX = currentPosition.x + displacement.x;
        int newY = currentPosition.y + displacement.y;

        if(map[currentPosition.x, currentPosition.y] == playerSide){  
            return countLine(new Vector2Int(newX, newY), displacement, ++comboCount);
        } 
        else {
            return comboCount;
        }
    } 

    private bool positionOutsideBounds(int x, int y){
        return x > BOARD_SIDE_LENGTH - 1
            || y > BOARD_SIDE_LENGTH - 1
            || x < 0
            || y < 0;
    }

    void ChangeSides ()
    {
        playerSide = (playerSide == "X") ? "O" : "X";
        if (playerSide == "X")
        {
            SetPlayerColors(playerX, playerO);
        } 
        else
        {
            SetPlayerColors(playerO, playerX);
        }
    }

    void SetPlayerColors (Player newPlayer, Player oldPlayer)
    {
        newPlayer.panel.color = activePlayerColor.panelColor;
        newPlayer.text.color = activePlayerColor.textColor;
        oldPlayer.panel.color = inactivePlayerColor.panelColor;
        oldPlayer.text.color = inactivePlayerColor.textColor;
    }

    void GameOver (string winningPlayer)
    {
        SetBoardInteractable(false);
        if (winningPlayer == "draw")
        {
            SetGameOverText("It's a Draw!");
            SetPlayerColorsInactive();
        } 
        else
        {
            SetGameOverText(winningPlayer + " Wins!");
        }
        restartButton.SetActive(true);
    }

    void SetGameOverText (string value)
    {
        gameOverPanel.SetActive(true);
        gameOverText.text = value;
    }

    public void RestartGame ()
    {
        moveCount = 0;
        gameOverPanel.SetActive(false);
        restartButton.SetActive(false);
        SetPlayerButtons (true);
        SetPlayerColorsInactive();
        startInfo.SetActive(true);

        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList [i].text = "";
        }
    }

    void SetBoardInteractable (bool toggle)
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<Button>().interactable = toggle;
        }
    }

    void SetPlayerButtons (bool toggle)
    {
        playerX.button.interactable = toggle;
        playerO.button.interactable = toggle;  
    }

    void SetPlayerColorsInactive ()
    {
        playerX.panel.color = inactivePlayerColor.panelColor;
        playerX.text.color = inactivePlayerColor.textColor;
        playerO.panel.color = inactivePlayerColor.panelColor;
        playerO.text.color = inactivePlayerColor.textColor;
    }
}
