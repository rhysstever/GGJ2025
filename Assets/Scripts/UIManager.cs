using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region Singleton Code
    // A public reference to this script
    public static UIManager instance = null;

    // Awake is called even before start
    private void Awake() {
        // If the reference for this script is null, assign it this script
        if(instance == null)
            instance = this;
        // If the reference is to something else (it already exists)
        // than this is not needed, thus destroy it
        else if(instance != this)
            Destroy(gameObject);
    }
    #endregion

    [SerializeField]    // Menu UI Parents
    private GameObject mainMenuUIParent, playerJoinUIParent, gameUIParent, gameEndUIParent;
    [SerializeField]    // Buttons
    private Button mainMenuToPlayerJoinButton, playerJoinToGameButton, addCPUButton, gameEndToMainMenuButton;
    [SerializeField]    // Text
    private TMP_Text gameEndTitle, gameEndPlayerStats;
    [SerializeField]    // GameObject Parents
    private GameObject player1JoinParent, player2JoinParent, playerSequenceParent;
    [SerializeField]    // Prefabs
    private GameObject playerListPrefab, cPUPlayerListPrefab;
    [SerializeField]    // Arrow Sprites
    private GameObject arrowUpPrefab, arrowDownPrefab, arrowLeftPrefab, arrowRightPrefab;
    [SerializeField]    // Game Timer Bars
    private GameObject gameTimerBarBackground, gameTimerBarValueObject;

    private Dictionary<InputDirection, GameObject> inputDirectionArrowMap;
    private float sequenceArrowXOffset;
    private float gameTimerBarMaxValue;

    // Start is called before the first frame update
    void Start() {
        SetupButtons();

        inputDirectionArrowMap = new Dictionary<InputDirection, GameObject>();
        inputDirectionArrowMap.Add(InputDirection.Up, arrowUpPrefab);
        inputDirectionArrowMap.Add(InputDirection.Down, arrowDownPrefab);
        inputDirectionArrowMap.Add(InputDirection.Left, arrowLeftPrefab);
        inputDirectionArrowMap.Add(InputDirection.Right, arrowRightPrefab);

        sequenceArrowXOffset = 90.0f;
        gameTimerBarMaxValue = gameTimerBarBackground.transform.localScale.x;

        playerJoinUIParent.SetActive(false);
        gameUIParent.SetActive(false);
        gameEndUIParent.SetActive(false);
    }

    #region Public Methods
    /// <summary>
    /// Change the UI based on a new game state
    /// </summary>
    /// <param name="newGameState">The new game state</param>
    public void ChangeUI(GameState newGameState) {
        switch(newGameState) {
            case GameState.MainMenu:
                gameEndUIParent.SetActive(false);
                mainMenuUIParent.SetActive(true);
                EventSystem.current.SetSelectedGameObject(mainMenuToPlayerJoinButton.gameObject);
                break;
            case GameState.PlayerJoin:
                mainMenuUIParent.SetActive(false);
                playerJoinUIParent.SetActive(true);
                UpdatePlayerJoinToGameButton();
                EventSystem.current.SetSelectedGameObject(addCPUButton.gameObject);
                break;
            case GameState.Game:
                playerJoinUIParent.SetActive(false);
                gameUIParent.SetActive(true);
                for(int i = 0; i < playerSequenceParent.transform.childCount; i++) {
                    ResetIndicator(i);
                }
                break;
            case GameState.GameEnd:
                gameUIParent.SetActive(false);
                gameEndUIParent.SetActive(true);
                DisplayPlayerStats();
                EventSystem.current.SetSelectedGameObject(gameEndToMainMenuButton.gameObject);
                break;
        }
    }

    public void DisplaySequence(int playerIndex) {
        List<InputDirection> sequence = GameManager.instance.Sequences[GameManager.instance.PlayerTotals[playerIndex]];

        Transform parentTransform = playerSequenceParent.transform.GetChild(playerIndex).GetChild(1);
        // Reset all children 
        for(int j = parentTransform.transform.childCount - 1; j >= 0; j--) {
            Destroy(parentTransform.GetChild(j).gameObject);
        }

        for(int i = 0; i < sequence.Count; i++) {
            GameObject arrowObjectPrefab = inputDirectionArrowMap[sequence[i]];
            Vector2 position = new Vector2(sequenceArrowXOffset * i, 0.0f);
            GameObject arrowObject = Instantiate(arrowObjectPrefab, parentTransform);
            arrowObject.transform.localPosition = position;
        }
    }

    /// <summary>
    /// Advance the position of the arrow
    /// </summary>
    /// <param name="playerIndex">The int index of the player</param>
    public void AdvanceSequenceIndicator(int playerIndex) {
        Vector2 pos = playerSequenceParent.transform.GetChild(playerIndex).GetChild(0).position;
        pos.x += sequenceArrowXOffset;
        playerSequenceParent.transform.GetChild(playerIndex).GetChild(0).position = pos;
    }

    /// <summary>
    /// Create a UI element when a player joins (player or CPU)
    /// </summary>
    /// <param name="index">The int index of the player joined</param>
    /// <param name="inputComponent">The component of the CPU's input. Possibly null if a player has joined</param>
    public void DisplayJoinedPlayer(int index, ComputerInput inputComponent) {
        Transform parent = player1JoinParent.transform;
        if(index == 1) {
            parent = player2JoinParent.transform;
        }

        if(inputComponent != null) {
            // If inputComponent exists, a CPU has been added to the game
            GameObject cPUPlayerListItem = Instantiate(
                cPUPlayerListPrefab,
                Vector2.zero,
                Quaternion.identity,
                parent.transform);
            cPUPlayerListItem.transform.localPosition = Vector2.zero;
            cPUPlayerListItem.GetComponent<CPUPlayerUI>().SetupValues(
                GameManager.instance.PlayerNames[index],
                inputComponent);
        } else {
            // If inputComponent is null, a non-CPU player has joined
            GameObject playerTextObject = Instantiate(
                playerListPrefab,
                Vector2.zero,
                Quaternion.identity,
                parent.transform);
            playerTextObject.transform.localPosition = Vector2.zero;
            playerTextObject.GetComponent<TMP_Text>().text = GameManager.instance.PlayerNames[index];
        }

        UpdatePlayerJoinToGameButton();
    }

    /// <summary>
    /// Updates the PlayerJoinToGame Button based on if there are enough players joined
    /// </summary>
    public void UpdatePlayerJoinToGameButton() {
        playerJoinToGameButton.interactable = GameManager.instance.GetPlayerCount() >= GameManager.instance.MinPlayerCount;
    }

    /// <summary>
    /// Update the Game End Title Text
    /// </summary>
    /// <param name="winningPlayerIndex">The index of the winning player</param>
    public void UpdateGameEndText(int winningPlayerIndex) {
        gameEndTitle.text = string.Format("Player {0} Wins!", winningPlayerIndex + 1);
    }

    public void ResetIndicator(int playerIndex) {
        playerSequenceParent.transform.GetChild(playerIndex).GetChild(0).localPosition = new Vector2(0.0f, 0.0f);
    }

    public void UpdateGameTimerBar(float timeLeftPercentage) {
        Vector3 currentScale = gameTimerBarValueObject.transform.localScale;
        float max = gameTimerBarMaxValue;
        float gameTimerBarValue = gameTimerBarMaxValue * timeLeftPercentage;
        currentScale.x = gameTimerBarValue;
        gameTimerBarValueObject.transform.localScale = currentScale;
    }
    #endregion Public Methods

    /// <summary>
    /// Set up button onClicks
    /// </summary>
    private void SetupButtons() {
        mainMenuToPlayerJoinButton.onClick.AddListener(() => GameManager.instance.ChangeGameState(GameState.PlayerJoin));
        playerJoinToGameButton.onClick.AddListener(() => GameManager.instance.ChangeGameState(GameState.Game));
        addCPUButton.onClick.AddListener(GameManager.instance.AddCPUInput);
        gameEndToMainMenuButton.onClick.AddListener(() => GameManager.instance.ChangeGameState(GameState.MainMenu));
    }

    private void DisplayPlayerStats() {
        gameEndPlayerStats.text = "";
        for(int i = 0; i < GameManager.instance.GetPlayerCount(); i++) {
            string playerName = GameManager.instance.PlayerNames[i];
            int playerTotal = GameManager.instance.PlayerTotals[i];
            string row = string.Format("{0}: {1}\n\n", playerName, playerTotal);
            gameEndPlayerStats.text += row;
        }
    }
}
