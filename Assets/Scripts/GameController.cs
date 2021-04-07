using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


[System.Serializable]
public class Player {
   public Image panel;
   public Sprite playerIcon;
   public Button button;
   public AudioClip placingSound;
}

[System.Serializable]
public class PlayerColor {
   public Color panelColor;
}

public class GameController : MonoBehaviour
{
    private delegate void ChangeScale(float scale);
    private IEnumerator changeIconSizeRoutine;
    private IEnumerator changeIconRotationRoutine;
    private IEnumerator changeRestartButtonSizeRoutine;
    private IEnumerator changeBoardColorsRoutine;
    private IEnumerator changeBackgroundColorRoutine;

    private Color[] originalBoardColors;
    private Color[] originalBackgroundColor;
    public Image[] boardColorChangeImages;
    public Image[] backgroundColorImages;
    public GridSpace[] buttonList;
    public GameObject gameOverPanel;
    public TMP_Text gameOverText;
    public GameObject restartButton;
    public Player playerX;
    public Player playerO;
    public PlayerColor activePlayerColor;
    public PlayerColor inactivePlayerColor;
    public GameObject startInfo;
    private AudioSource audioSource;

    private string playerSide;
    private int moveCount;
    private GridSpace[,] map;
    private List<GridSpace> comboLine;
    // private bool hasWon = true;
    private int BOARD_SIDE_LENGTH;

    void Awake ()
    {
        SetGameControllerReferenceOnButtons();
        gameOverPanel.SetActive(false);
        moveCount = 0;
        restartButton.SetActive(false);
        SetPlayerColorsInactive();
        audioSource = GetComponent<AudioSource>();
        
        originalBackgroundColor = new Color[backgroundColorImages.Length];
        originalBoardColors = new Color[boardColorChangeImages.Length];

        changeIconSizeRoutine = changeIconSize(ChangeComboLineScale);
        changeRestartButtonSizeRoutine = changeIconSize(ChangeReStartButtonScale);
        changeBoardColorsRoutine = changeColors(boardColorChangeImages, boardColorChangeImages[0].color, Color.magenta);
        changeBackgroundColorRoutine = changeColors(backgroundColorImages, backgroundColorImages[0].color, new Color(0.85f,0.85f,0.85f,1f));
        changeIconRotationRoutine = changeIconRotation();

        originalBackgroundColor = new Color[backgroundColorImages.Length];
        originalBoardColors = new Color[boardColorChangeImages.Length];

        for(int i = 0; i < boardColorChangeImages.Length; i++){
            originalBoardColors[i] = boardColorChangeImages[i].color;
        }   
        for(int i = 0; i < backgroundColorImages.Length; i++){
            originalBackgroundColor[i] = backgroundColorImages[i].color;
        }   
    }

    void Start() {
        int sideLength = (int) Mathf.Sqrt(buttonList.Length);
        map = new GridSpace[sideLength,sideLength];
        BOARD_SIDE_LENGTH = sideLength;
        ResetMap();
        StopCoroutine(changeIconSizeRoutine);
        changeIconSizeRoutine = changeIconSize(ChangePlayersScale);
        StartCoroutine(changeIconSizeRoutine);
    }
    void SetGameControllerReferenceOnButtons ()
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].SetGameControllerReference(this);
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
        StopCoroutine(changeIconSizeRoutine);
        StopCoroutine(changeRestartButtonSizeRoutine);
        ResetPlayersScale();
    }

    public string GetPlayerSide ()
    {
        return playerSide;
    }

    public Sprite GetPlayerSideIcon ()
    {
        if(playerSide == "X"){
            return playerX.playerIcon;
        } else {
            return playerO.playerIcon;
        }
    }

    public void EndTurn ()
    {
        fillMap();
        moveCount++;
        playPlacingSound();

        if(hasPlayerWon()){
            GameOver(playerSide);
            // StopCoroutine(changeIconSizeRoutine);
            // changeIconSizeRoutine = changeIconSize(ChangeComboLineScale);
            // StartCoroutine(changeIconSizeRoutine);

            StartCoroutine(changeBoardColorsRoutine);
            StartCoroutine(changeBackgroundColorRoutine);
            StartCoroutine(changeIconRotationRoutine);
        } 
        else if (moveCount >= 9){
            GameOver("draw");
        } 
        else {
            ChangeSides();
        }

    }
    private void playPlacingSound(){
        if(playerSide == "X"){
            audioSource.PlayOneShot(playerX.placingSound);
        } else {
            audioSource.PlayOneShot(playerO.placingSound);
        }    
    }

    // Copies values from buttonList into a 2D array
    private void fillMap(){
        for(int i = 0; i < buttonList.Length; i++){
            int x = i % BOARD_SIDE_LENGTH;
            int y = i / BOARD_SIDE_LENGTH;            
            map[x, y] = buttonList[i];
        }
    }

    private IEnumerator changeIconSize(ChangeScale changeScaleFunction){
        float maxScale = 1.2f;
        float minScale = 1f;
        float currentScale = 1f;
        bool scaleChangeToggle = false;

        while (true){
            if(scaleChangeToggle){
                currentScale += Time.deltaTime;
                scaleChangeToggle = (currentScale < maxScale);
            } 
            else {
                currentScale -= Time.deltaTime;
                scaleChangeToggle = (currentScale < minScale);
            }
            changeScaleFunction(currentScale);
            yield return new WaitForSeconds(0.005f);
        }
    }

    private IEnumerator changeIconRotation(){
        float currentRotation = 0f;

        while (true){
            currentRotation += Time.deltaTime + 0.5f;
            foreach(GridSpace spot in comboLine){
                spot.GetComponent<Image>().transform.rotation = Quaternion.Euler(0, 0, currentRotation);
            }
            yield return new WaitForSeconds(0.005f);
        }
    }

    private IEnumerator changeColors(Image[] images, Color startColor, Color goalColor){
        Color currentColor = startColor;
        bool toggleDirection = true;

        while(true){
            
            if(toggleDirection){
                currentColor = Color.Lerp(startColor, goalColor, Mathf.PingPong(Time.time, 1));
                toggleDirection = (Math.Abs(currentColor.r - goalColor.r) <= 3 &&
                                    Math.Abs(currentColor.b - goalColor.b) <= 3 &&
                                    Math.Abs(currentColor.g - goalColor.g) <= 3);
            }
            else {
                currentColor = Color.Lerp(goalColor, startColor, Mathf.PingPong(Time.time, 1));
                toggleDirection = (Math.Abs(currentColor.r - startColor.r) <= 3 &&
                                    Math.Abs(currentColor.b - startColor.b) <= 3 &&
                                    Math.Abs(currentColor.g - startColor.g) <= 3);
            }

            foreach(Image img in images){
                img.color = currentColor;
            }
            yield return new WaitForSeconds(0.005f);
        }

    }

    private void ResetIconRotation(){
        foreach(GridSpace spot in comboLine){
            spot.GetComponent<Image>().transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
    private void ResetBoardBackgroundColors(){
        for(int i = 0; i < boardColorChangeImages.Length; i++){
            boardColorChangeImages[i].color = originalBoardColors[i];
        }
    }

    private void ResetBackgroundColors(){
        for(int i = 0; i < backgroundColorImages.Length; i++){
            backgroundColorImages[i].color = originalBackgroundColor[i];
        }
    }

    private void ChangeComboLineScale(float scale){
        foreach(GridSpace spot in comboLine){
            spot.GetComponent<Image>().transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    private void ChangePlayersScale(float scale){
        playerO.panel.transform.localScale = new Vector3(scale, scale, 1f);
        playerX.panel.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void ChangeReStartButtonScale(float scale){
        restartButton.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void ResetPlayersScale(){
        playerO.panel.transform.localScale = new Vector3(1f, 1f, 1f);
        playerX.panel.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    // Check if win condition has been fulfilled
    private bool hasPlayerWon(){
        // Since the map has equally long sides only one loop is needed
        for(int i = 0; i < BOARD_SIDE_LENGTH; i++){
            
            // Count values in columns
            comboLine = findComboLine(new Vector2Int(i, 0), new Vector2Int(0, 1), new List<GridSpace>());
            if(comboLine.Count == BOARD_SIDE_LENGTH){
                return true;
            }
            // Count values in rows
            comboLine = findComboLine(new Vector2Int(0, i), new Vector2Int(1, 0), new List<GridSpace>());
            if(comboLine.Count == BOARD_SIDE_LENGTH){
                return true;
            }
        }
        // Count values in first diagonal
        comboLine = findComboLine(new Vector2Int(0,0), new Vector2Int(1, 1), new List<GridSpace>());
        if(comboLine.Count == BOARD_SIDE_LENGTH){
            return true;
        }

        // Count values in second diagonal
        comboLine = findComboLine(new Vector2Int(map.GetLength(0)-1, 0), new Vector2Int(-1, 1), new List<GridSpace>());
        if(comboLine.Count == BOARD_SIDE_LENGTH){
            return true;
        } 

        return false;
    }

    // Count the playerSide(X or O) values in a line
    private List<GridSpace> findComboLine(Vector2Int currentPosition, Vector2Int displacement, List<GridSpace> line){
        
        if(positionOutsideBounds(currentPosition.x, currentPosition.y)
            || line.Count == BOARD_SIDE_LENGTH){
            return line;
        }

        int newX = currentPosition.x + displacement.x;
        int newY = currentPosition.y + displacement.y;

        if(map[currentPosition.x, currentPosition.y].playerSide == playerSide){  
            line.Add(map[currentPosition.x, currentPosition.y]);
            return findComboLine(new Vector2Int(newX, newY), displacement, line);
        } 
        else {
            return line;
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
        newPlayer.panel.GetComponent<Image>().sprite = newPlayer.playerIcon;
        newPlayer.panel.GetComponent<Image>().color = activePlayerColor.panelColor;

        oldPlayer.panel.GetComponent<Image>().sprite = oldPlayer.playerIcon;
        oldPlayer.panel.GetComponent<Image>().color = inactivePlayerColor.panelColor;
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
            if(winningPlayer == "O"){
                SetGameOverText("Fire Wins!");
            } else {
                SetGameOverText("Water Wins!");
            }

        }
        restartButton.SetActive(true);
        StartCoroutine(changeRestartButtonSizeRoutine);
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
        ResetMap();
        ChangeComboLineScale(1f);

        StopCoroutine(changeIconSizeRoutine);
        changeIconSizeRoutine = changeIconSize(ChangePlayersScale);
        StartCoroutine(changeIconSizeRoutine);
        
        StopCoroutine(changeBoardColorsRoutine);
        ResetBoardBackgroundColors();

        StopCoroutine(changeBackgroundColorRoutine);
        ResetBackgroundColors();

        StopCoroutine(changeIconRotationRoutine);
        ResetIconRotation();
    }

    private void ResetMap(){
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].playerSide = "";
            buttonList[i].GetComponent<Image>().sprite = null;
            buttonList[i].GetComponent<Image>().color = new Color(0,0,0,0);
        }
    }

    void SetBoardInteractable (bool toggle)
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponent<Button>().interactable = toggle;
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
        playerO.panel.color = inactivePlayerColor.panelColor;
    }
}
