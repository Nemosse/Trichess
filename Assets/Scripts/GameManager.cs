using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using UnityEngine.SceneManagement;

// using System.Diagnostics;
// using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using Unity.VisualScripting;


public class GameManager : MonoBehaviour
{
    [Header("GameSetting")]
    [SerializeField] private bool gameOver;
    public bool isPlayable = false;
    private Vector3 lastMousePosition;
    public static GameManager instance;
    public List<PieceField> movableFields;

    public PieceField player1KingPieceField;
    public PieceField player2KingPieceField;
    public PieceField player3KingPieceField;
    public string machineIP;

    public bool test = true;
    public bool LanMode = false;


    [Header("Turn")]
    [SerializeField] private int turn = 1;
    [SerializeField] private Players currentTurnPlayer = Players.Player1;
    [SerializeField] private PieceField selectedPiece = null;
    [SerializeField] public int currentPastTurn = 0;

    [Header("BoardPieceField")]
    [SerializeField] private GameObject[] blueBoard = new GameObject[32];
    [SerializeField] private GameObject[] greenBoard = new GameObject[32];
    [SerializeField] private GameObject[] redBoard = new GameObject[32];
    // [SerializeField] private List<PieceField> movableField;

    [Header("UI")]
    [SerializeField] private GameObject pawnPromotePanel;
    public bool togglePromotePawn;
    [SerializeField] private TextMeshProUGUI currentPlayerUI;
    [SerializeField] private GameObject gameResetTextUI;
    public GameObject lobbyUI;
    private TextMeshProUGUI gameResetTextUISub;
    public float fadeInDuration = 0.0f; // Time to fade in
    public float stayDuration = 4.0f;   // Time to stay visible
    public float fadeOutDuration = 3.0f; // Time to fade out
    private Players promotePawnColor;
    private PieceField pawnPromotePieceField = null;



    private float startTime;
    private static bool isFadingIn = false;
    private static bool isFadingOut = false;
    private static bool fadingStart = false;
    private Color originalColor;

    [SerializeField] private TextMeshProUGUI gameOverTextUI;
    private TextMeshProUGUI checkmatedTextUi;

    [Header("Button")]
    [SerializeField] public Button backwardButton;
    [SerializeField] public Button forwardButton;
    [SerializeField] public Button passTurnButton;

    [Header("PawnPromote")]
    public Pieces blueQueen;
    public Pieces blueKnight;
    public Pieces blueRook;
    public Pieces blueBishop;
    public Pieces greenQueen;
    public Pieces greenKnight;
    public Pieces greenRook;
    public Pieces greenBishop;
    public Pieces redQueen;
    public Pieces redKnight;
    public Pieces redRook;
    public Pieces redBishop;

    [Header("Server")]
    public Server server;
    public bool gameStarted;
    public bool[] playerManualList = new bool[3] { true, true, true };
    public PieceField pieceFieldServerCheck;
    public List<string> boardState = new List<string>();
    public bool dataGatheringCompleted = true;
    public bool dataGatheringBoardPlayerCompleted = true;
    public string sendBoardTurnDatasSring = "";
    public int gsCurrentClient;
    public string gsFrom = "";
    public string gsTo = "";
    public PieceField gsMover;
    public PieceField gsMoveTo;
    public string gsMovableField = "";
    public List<string> movableFieldsList = new List<string>();
    public string movableFieldsListJson = "";
    public string kingMovableFieldJson = "";
    public string blockableFieldJson = "";
    public string whoCheckedJson = "";
    public Players gsMyPiecePlayer = Players.Empty;
    public int gsPromotionInt = -1;
    bool pawnPromoteActive = false;

    void Start()
    {

        lastMousePosition = Input.mousePosition;
        currentPlayerUI.text = "Lobby";

        IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

        foreach (IPAddress ipAddress in localIPs)
        {
            if (ipAddress.AddressFamily == AddressFamily.InterNetwork) // IPv4 addresses
            {
                machineIP = $"ws://{ipAddress}:8181/game";
            }
        }

        lobbyUI.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = machineIP;

    }

    void Awake()
    {
        instance = this;

        passTurnButton.interactable = false;

        lobbyUI.transform.GetChild(5).gameObject.SetActive(false);

        lobbyUI.transform.GetChild(3).gameObject.SetActive(false);
        pawnPromotePanel.SetActive(false);

        checkmatedTextUi = gameOverTextUI.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        gameOverTextUI.gameObject.SetActive(false);

        backwardButton.interactable = false;
        forwardButton.interactable = false;
        gameResetTextUISub = gameResetTextUI.GetComponent<TextMeshProUGUI>();


        originalColor = gameResetTextUISub.color;
        gameResetTextUISub.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        // Debug.Log(MoveStepCheck(GetPieceField(BoardPosition.BC2), new Direction[] { Direction.Forward, Direction.Left }).name);



        for (int i = 0; i < blueBoard.Length; i++)
        {
            // var temp = blueBoard[i].transform.GetChild(1).GetComponent<SpriteRenderer>();
            // blueBoard[i].transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color(temp.color.r, temp.color.g, temp.color.b, 0f);

            UpdatePieceFieldImage(blueBoard[i].transform.GetChild(1).GetComponent<PieceField>());
        }

        for (int i = 0; i < greenBoard.Length; i++)
        {
            // var temp = greenBoard[i].transform.GetChild(1).GetComponent<SpriteRenderer>();
            // greenBoard[i].transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color(temp.color.r, temp.color.g, temp.color.b, 0f);

            UpdatePieceFieldImage(greenBoard[i].transform.GetChild(1).GetComponent<PieceField>());
        }

        for (int i = 0; i < redBoard.Length; i++)
        {
            // var temp = redBoard[i].transform.GetChild(1).GetComponent<SpriteRenderer>();
            // redBoard[i].transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color(temp.color.r, temp.color.g, temp.color.b, 0f);

            UpdatePieceFieldImage(redBoard[i].transform.GetChild(1).GetComponent<PieceField>());
        }

        player1KingPieceField = GetPieceField(BoardPosition.BE1);
        player2KingPieceField = GetPieceField(BoardPosition.GE1);
        player3KingPieceField = GetPieceField(BoardPosition.RE1);
    }

    void Update()
    {
        IfMouseMove();

        if (isFadingIn)
        {
            float elapsedTime = Time.time - startTime;
            float alpha = elapsedTime / fadeInDuration;
            gameResetTextUI.gameObject.SetActive(true);

            if (alpha < 1)
            {
                gameResetTextUISub.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            }
            else
            {
                isFadingIn = false;
                startTime = Time.time;

            }
        }
        else if (!isFadingIn && !isFadingOut && fadingStart)
        {
            if (Time.time - startTime >= stayDuration)
            {
                isFadingOut = true;
                startTime = Time.time;
            }
        }
        else if (isFadingOut)
        {
            float elapsedTime = Time.time - startTime;
            float alpha = 1f - elapsedTime / fadeOutDuration;

            if (alpha > 0)
            {
                gameResetTextUISub.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            }
            else
            {
                isFadingOut = false;
                gameResetTextUI.gameObject.SetActive(false);
            }
        }
    }

    #region Utility

    public void ResetTheGame()
    {
        ResetHistory();

        startTime = Time.time;
        fadingStart = true;
        isFadingIn = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private static void ResetHistory()
    {
        History.history = new List<Record>();

    }

    public PieceField GetPieceField(BoardPosition boardPosition)
    {
        GameObject[] boardToLook;
        int resetIndex = 0;


        if ((int)boardPosition < 32 && (int)boardPosition >= 0)
        {
            boardToLook = blueBoard;
        }
        else if ((int)boardPosition < 64 && (int)boardPosition >= 32)
        {
            boardToLook = greenBoard;
            resetIndex = 32;
        }
        else if ((int)boardPosition <= 95 && (int)boardPosition >= 64)
        {
            boardToLook = redBoard;
            resetIndex = 64;
        }
        else
        {
            boardToLook = null;
        }

        if (boardToLook != null)
        {
            // Debug.Log("Founded Board");
            for (int i = 0; i < boardToLook.Length; i++)
            {
                if (boardToLook[(int)boardPosition - resetIndex].transform.GetChild(1).GetComponent<PieceField>().GetBoardPosition() == boardPosition)
                {
                    // Debug.Log("Founded Space");
                    // Debug.Log(boardToLook[(int)boardPosition - resetIndex].name);
                    return boardToLook[(int)boardPosition - resetIndex].transform.GetChild(1).GetComponent<PieceField>();

                }

            }
            // Debug.Log("Not in space");
            return null;
        }
        else
        {
            return null;
        }

    }

    public void ChangeCurrentTurnPlayer()
    {
        turn += 1;
        passTurnButton.interactable = true;

        if (gameOver)
        {
            currentTurnPlayer = Players.Empty;
            passTurnButton.interactable = false;
        }
        else
        {
            if (currentTurnPlayer == Players.Player3)
            {
                currentTurnPlayer = Players.Player1;
            }
            else
            {
                currentTurnPlayer = (Players)((int)currentTurnPlayer + 1);
            }

        }

        if (LanMode)
        {
            passTurnButton.interactable = false;
            isPlayable = false;
            for (int i = 0; i < Server.instance.connectedClients.Count; i++)
            {
                if (Server.instance.connectedClients[i].isLocal == true && Server.instance.connectedClients[i].Player == currentTurnPlayer)
                {
                    if (!togglePromotePawn)
                    {
                        isPlayable = true;
                        passTurnButton.interactable = true;
                    }
                }
            }

            for (int i = 0; i < Server.instance.connectedClients.Count; i++)
            {
                if (Server.instance.connectedClients[i].isLocal == false && Server.instance.connectedClients[i].Player == currentTurnPlayer)
                {
                    Server.instance.connectedClients[i].gameService.SendBoardTurnDatasTurn();
                }
            }
        }
        else
        {
            passTurnButton.interactable = true;
        }

        togglePromotePawn = false;
        UpdateCurrentPlayerTurnUI();
    }

    public Players GetCurrentTurnPlayer()
    {
        return currentTurnPlayer;
    }

    public void IfMouseMove()
    {
        Vector3 currentMousePosition = Input.mousePosition;

        if (currentMousePosition != lastMousePosition)
        {
            lastMousePosition = currentMousePosition;
            // Debug.Log("mouse move");
            UpdateHoverPieceField();
        }
    }


    public void SetSelectedPiece(PieceField pieceField)
    {
        selectedPiece = pieceField;
    }

    public PieceField GetSelectedPiece()
    {
        return selectedPiece;
    }

    #endregion

    #region UIUpdate

    public void StartGame()
    {
        lobbyUI.SetActive(false);
        if (LanMode)
        {

            for (int i = 0; i < Server.instance.connectedClients.Count; i++)
            {
                if (Server.instance.connectedClients[i].isLocal == false)
                {
                    Server.instance.connectedClients[i].gameService.BroadcastGameStart();
                    break;
                }
            }

            isPlayable = false;
            for (int i = 0; i < Server.instance.connectedClients.Count; i++)
            {
                if (Server.instance.connectedClients[i].isLocal == true && Server.instance.connectedClients[i].Player == currentTurnPlayer)
                {
                    passTurnButton.interactable = true;
                    isPlayable = true;
                }
            }

            for (int i = 0; i < Server.instance.connectedClients.Count; i++)
            {
                if (Server.instance.connectedClients[i].isLocal == false && Server.instance.connectedClients[i].Player == currentTurnPlayer)
                {
                    Server.instance.connectedClients[i].gameService.SendBoardTurnDatasTurn();
                }
            }

        }
        else
        {
            passTurnButton.interactable = true;
            isPlayable = true;
        }

        gameStarted = true;

        UpdateCurrentPlayerTurnUI();
    }

    public void SwitchLanOrLocalButtonUI(int i)
    {
        if (i == 0) //click on lan
        {
            server.StartServer();
            playerManualList = new bool[3];

            lobbyUI.transform.GetChild(1).GetComponent<Button>().interactable = false; //make lan false
            lobbyUI.transform.GetChild(2).GetComponent<Button>().interactable = true; // make local true
            lobbyUI.transform.GetChild(3).gameObject.SetActive(true);

            lobbyUI.transform.GetChild(4).GetChild(1).gameObject.SetActive(false);
            lobbyUI.transform.GetChild(4).GetChild(2).gameObject.SetActive(false);
            lobbyUI.transform.GetChild(4).GetChild(3).gameObject.SetActive(false);
            lobbyUI.transform.GetChild(4).GetChild(1).GetChild(1).gameObject.SetActive(false);
            lobbyUI.transform.GetChild(4).GetChild(2).GetChild(1).gameObject.SetActive(false);
            lobbyUI.transform.GetChild(4).GetChild(3).GetChild(1).gameObject.SetActive(false);
            LanMode = true;
            lobbyUI.transform.GetChild(0).gameObject.SetActive(false);
            lobbyUI.transform.GetChild(5).gameObject.SetActive(true);
        }
        else if (i == 1) //click on local
        {
            server.StopServer();
            playerManualList = new bool[3] { true, true, true };

            lobbyUI.transform.GetChild(1).GetComponent<Button>().interactable = true; //make lan true
            lobbyUI.transform.GetChild(2).GetComponent<Button>().interactable = false; //make local false
            lobbyUI.transform.GetChild(3).gameObject.SetActive(false);

            lobbyUI.transform.GetChild(4).GetChild(1).gameObject.SetActive(true);
            lobbyUI.transform.GetChild(4).GetChild(2).gameObject.SetActive(true);
            lobbyUI.transform.GetChild(4).GetChild(3).gameObject.SetActive(true);
            lobbyUI.transform.GetChild(4).GetChild(1).GetChild(1).gameObject.SetActive(false);
            lobbyUI.transform.GetChild(4).GetChild(2).GetChild(1).gameObject.SetActive(false);
            lobbyUI.transform.GetChild(4).GetChild(3).GetChild(1).gameObject.SetActive(false);
            LanMode = false;
            lobbyUI.transform.GetChild(0).gameObject.SetActive(true);
            lobbyUI.transform.GetChild(5).gameObject.SetActive(false);
        }
    }

    public void UpdatePieceFieldImage(PieceField pieceField)
    {
        // Debug.Log("Hello");
        if (pieceField.GetPiece() == null)
        {
            pieceField.transform.parent.GetChild(0).GetComponent<SpriteRenderer>().sprite = null;
            pieceField.transform.parent.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(pieceField.transform.parent.GetChild(0).GetComponent<SpriteRenderer>().color.r, pieceField.transform.parent.GetChild(0).GetComponent<SpriteRenderer>().color.g, pieceField.transform.parent.GetChild(0).GetComponent<SpriteRenderer>().color.b, 0f);
            // Debug.Log("test");
        }
        else
        {
            pieceField.transform.parent.GetChild(0).GetComponent<SpriteRenderer>().sprite = pieceField.GetPiece().GetIcon();
            // pieceField.transform.parent.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(pieceField.transform.parent.GetChild(0).GetComponent<SpriteRenderer>().color.r, pieceField.transform.parent.GetChild(0).GetComponent<SpriteRenderer>().color.g, pieceField.transform.parent.GetChild(0).GetComponent<SpriteRenderer>().color.b, 1f);
            if (pieceField.GetPiece().GetPlayerOwner() == Players.Player1)
            {
                pieceField.transform.parent.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 255f, 1f);
            }
            else if (pieceField.GetPiece().GetPlayerOwner() == Players.Player2)
            {
                pieceField.transform.parent.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(0f, 255f, 0f, 1f);
            }
            else if (pieceField.GetPiece().GetPlayerOwner() == Players.Player3)
            {
                pieceField.transform.parent.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(255f, 0f, 0f, 1f);
            }
            // Debug.Log("test1");
        }
    }

    public void UpdateHoverPieceField()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            hit.transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color(hit.collider.transform.GetChild(1).GetComponent<SpriteRenderer>().color.r, hit.collider.transform.GetChild(1).GetComponent<SpriteRenderer>().color.g, hit.collider.transform.GetChild(1).GetComponent<SpriteRenderer>().color.b, 0.1f);
            Debug.Log("hit");
        }
    }

    public void UpdateAuraMovableField()
    {

        ResetMovableAuraField();


        for (int i = 0; i < movableFields.Count; i++)
        {
            var temp = movableFields[i].transform.GetComponent<SpriteRenderer>();
            temp.color = new Color(255f, 255f, 0f, 0.25f);
        }
    }

    public void UpdateMovableField(PieceField pieceField)
    {
        if (pieceField != null)
        {
            if (pieceField.GetPiece() != null)
            {
                movableFields = new List<PieceField>();
                movableFields = MovablePieceField(pieceField);
            }
            else
            {
                ResetMovableField();
                ResetMovableAuraField();
            }
        }

        UpdateAuraMovableField();
    }

    public void ResetMovableAuraField()
    {
        for (int i = 0; i < 96; i++)
        {
            var temp = GetPieceField((BoardPosition)i).transform.GetComponent<SpriteRenderer>();
            temp.color = new Color(255f, 255f, 255f, 0f);
        }
    }

    public void ResetMovableField()
    {
        movableFields = new List<PieceField>();
    }

    public void UpdateCurrentPlayerTurnUI()
    {
        if (gameOver)
        {
            currentPlayerUI.text = "   Game Over!";
        }
        else if (currentPastTurn != 0)
        {
            currentPlayerUI.text = $"   Turn : {turn - currentPastTurn}";
        }
        else
        {
            currentPlayerUI.text = $"   Player's Turn : <color={GetPlayerColor((int)currentTurnPlayer)}>Player {(int)currentTurnPlayer}</color>";
        }

    }

    public void UpdateButton()
    {
        if (History.history.Count > 0 && currentPastTurn != History.history.Count)
        {
            backwardButton.interactable = true;
        }
        else
        {
            backwardButton.interactable = false;
        }

        if (currentPastTurn != 0)
        {
            forwardButton.interactable = true;
        }
        else
        {
            forwardButton.interactable = false;
        }
    }

    public void BackwardTurn()
    {
        passTurnButton.interactable = false;
        currentPastTurn += 1;

        Record tempRecord = History.history[History.history.Count - currentPastTurn];

        for (int i = 0; i < 96; i++)
        {
            GetPieceField((BoardPosition)i).gameObject.GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, 0f);
        }
        Pieces moverPiece = tempRecord.GetVariablePiece(0);
        Pieces takenPiece = tempRecord.GetVariablePiece(1);

        PieceField oldPieceMoverField = tempRecord.GetVariablePieceField(0);
        PieceField newPieceMoverField = tempRecord.GetVariablePieceField(1);
        PieceField oldRookCastleField = tempRecord.GetVariablePieceField(2);
        PieceField newRookCastleField = tempRecord.GetVariablePieceField(3);
        bool firstMove = tempRecord.GetFirstMove();

        if (oldPieceMoverField != null && newPieceMoverField != null)
        {
            oldPieceMoverField.SetPiece(moverPiece);
            newPieceMoverField.SetPiece(takenPiece);
            oldPieceMoverField.GetPiece().SetCastlable(firstMove);
            UpdatePieceFieldImage(oldPieceMoverField);
            UpdatePieceFieldImage(newPieceMoverField);
            oldPieceMoverField.gameObject.GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 0f, 1f);
            newPieceMoverField.gameObject.GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 0f, 1f);
        }

        if (oldRookCastleField != null && newRookCastleField != null)
        {
            oldRookCastleField.SetPiece(newRookCastleField.GetPiece());
            newRookCastleField.SetPiece(null);
            oldRookCastleField.GetPiece().SetCastlable(firstMove);
            UpdatePieceFieldImage(oldRookCastleField);
            UpdatePieceFieldImage(newRookCastleField);
            oldRookCastleField.gameObject.GetComponent<SpriteRenderer>().color = new Color(0f, 255f, 0f, 1f);
            newRookCastleField.gameObject.GetComponent<SpriteRenderer>().color = new Color(0f, 255f, 0f, 1f);
        }

        gameOverTextUI.gameObject.SetActive(false);

        isPlayable = false;
        UpdateCurrentPlayerTurnUI();
        UpdateButton();
    }

    public void ForwardTurn()
    {
        Record tempRecord = History.history[History.history.Count - currentPastTurn];

        for (int i = 0; i < 96; i++)
        {
            GetPieceField((BoardPosition)i).gameObject.GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, 0f);
        }
        Pieces moverPiece = tempRecord.GetVariablePiece(0);

        PieceField oldPieceMoverField = tempRecord.GetVariablePieceField(0);
        PieceField newPieceMoverField = tempRecord.GetVariablePieceField(1);
        PieceField oldRookCastleField = tempRecord.GetVariablePieceField(2);
        PieceField newRookCastleField = tempRecord.GetVariablePieceField(3);
        Pieces pawnPromotionPiece = tempRecord.GetVariablePiece(2);
        bool firstMove = tempRecord.GetFirstMove();

        if (oldPieceMoverField != null && pawnPromotionPiece != null)
        {
            newPieceMoverField.SetPiece(pawnPromotionPiece);
        }
        else if (oldPieceMoverField != null && pawnPromotionPiece == null)
        {
            newPieceMoverField.SetPiece(moverPiece);
        }

        if (oldPieceMoverField != null && newPieceMoverField != null)
        {
            oldPieceMoverField.SetPiece(null);
            newPieceMoverField.GetPiece().SetCastlable(firstMove);
            UpdatePieceFieldImage(oldPieceMoverField);
            UpdatePieceFieldImage(newPieceMoverField);
            oldPieceMoverField.gameObject.GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 0f, 1f);
            newPieceMoverField.gameObject.GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 0f, 1f);
        }

        if (oldRookCastleField != null && newRookCastleField != null)
        {
            newRookCastleField.SetPiece(oldRookCastleField.GetPiece());
            oldRookCastleField.SetPiece(null);
            newRookCastleField.GetPiece().SetCastlable(firstMove);
            UpdatePieceFieldImage(oldRookCastleField);
            UpdatePieceFieldImage(newRookCastleField);
            oldRookCastleField.gameObject.GetComponent<SpriteRenderer>().color = new Color(0f, 255f, 0f, 1f);
            newRookCastleField.gameObject.GetComponent<SpriteRenderer>().color = new Color(0f, 255f, 0f, 1f);
        }

        currentPastTurn -= 1;

        if (currentPastTurn == 0)
        {
            if (gameOver == false)
            {

                isPlayable = true;
                if (LanMode)
                {
                    passTurnButton.interactable = false;
                    for (int i = 0; i < Server.instance.connectedClients.Count; i++)
                    {
                        if (Server.instance.connectedClients[i].isLocal == true && Server.instance.connectedClients[i].Player == currentTurnPlayer)
                        {
                            passTurnButton.interactable = true;
                            break;
                        }
                    }
                }
                else
                {
                    passTurnButton.interactable = true;
                }
            }
            else
            {
                gameOverTextUI.gameObject.SetActive(true);
            }
            if (oldPieceMoverField != null && newPieceMoverField != null)
            {
                oldPieceMoverField.gameObject.GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, 0f);
                newPieceMoverField.gameObject.GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, 0f);
            }

            if (oldRookCastleField != null && newRookCastleField != null)
            {
                oldRookCastleField.gameObject.GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, 0f);
                newRookCastleField.gameObject.GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, 0f);
            }
        }
        UpdateCurrentPlayerTurnUI();
        UpdateButton();
    }

    public void UpdatePromotePawnUIColor()
    {
        Debug.Log(promotePawnColor);

        if (promotePawnColor == Players.Player1)
        {
            pawnPromotePanel.GetComponent<Image>().color = new Color(120f / 255f, 200f / 255f, 250f / 255f, 1f);

            for (int i = 0; i < pawnPromotePanel.transform.childCount; i++)
            {
                pawnPromotePanel.transform.GetChild(i).GetComponent<Image>().color = new Color(150f / 255f, 210f / 255f, 255f / 255f, 1f);
            }
        }
        else if (promotePawnColor == Players.Player2)
        {
            pawnPromotePanel.GetComponent<Image>().color = new Color(120f / 255f, 250f / 255f, 120f / 255f, 1f);

            for (int i = 0; i < pawnPromotePanel.transform.childCount; i++)
            {
                pawnPromotePanel.transform.GetChild(i).GetComponent<Image>().color = new Color(160f / 255f, 255f / 255f, 150f / 255f, 1f);
            }
        }
        else if (promotePawnColor == Players.Player3)
        {
            pawnPromotePanel.GetComponent<Image>().color = new Color(250f / 255f, 120 / 255f, 120 / 255f, 1f);

            for (int i = 0; i < pawnPromotePanel.transform.childCount; i++)
            {
                pawnPromotePanel.transform.GetChild(i).GetComponent<Image>().color = new Color(255f / 255f, 150f / 255f, 160f / 255f, 1f);
            }
        }
    }
    #endregion

    #region StepAndMove
    public PieceField MoveStepCheck(PieceField pieceField, Direction[] moves, bool reverse = false, Players player = Players.Empty)
    {
        if (player == Players.Empty)
        {
            if (pieceField.GetPiece() != null)
            {
                player = pieceField.GetPiece().GetPlayerOwner();
            }
        }

        PieceField current = pieceField;

        if (pieceField != null)
        {
            if (pieceField.GetPiece() != null)
            {
                if (pieceField.GetPiece().GetPieceType() == PieceTypes.Pawn && pieceField.GetColor() != pieceField.GetPiece().GetPlayerOwner() && pieceField.GetPiece().GetPlayerOwner() == player)
                {
                    reverse = true;
                }
            }
        }

        foreach (Direction originalMove in moves)
        {
            Direction move = originalMove;
            if (pieceField != null)
            {
                if (reverse)
                {
                    switch (originalMove)
                    {
                        case Direction.Forward:
                            move = Direction.Backward;
                            break;
                        case Direction.Backward:
                            move = Direction.Forward;
                            break;
                        case Direction.Left:
                            move = Direction.Right;
                            break;
                        case Direction.Right:
                            move = Direction.Left;
                            break;
                    }
                }
            }
            PieceField nextPosition = null;

            nextPosition = current.MovableSpaceCheck(move);

            if (nextPosition == null)
            {
                return null;
            }

            if (nextPosition != null)
            {
                if (nextPosition.GetColor() != current.GetColor())
                {
                    reverse = true;
                }
            }

            current = nextPosition;
        }


        return current;

    }

    public bool IsLegalMove(PieceField startingPosition, PieceField endingPosition)
    {
        List<PieceField> movablePieceField = MovablePieceField(startingPosition);

        for (int i = 0; i < movablePieceField.Count; i++)
        {
            if (endingPosition == movablePieceField[i])
            {
                return true;
            }
        }

        return false;

    }

    public List<PieceField> MovablePieceField(PieceField piece)
    {
        List<PieceField> possibleField = new List<PieceField>();
        // try
        // {
        if (piece.GetPiece() != null)
        {
            (bool isKingInCheck, List<PieceField> kingMovableField, bool isBlockable, List<PieceField> blockableField, List<PieceField> nonMovablePiece, List<PieceField> nonMovablePiece_movableField, List<Players> playerCheck) = IsKingInCheck(piece.GetPiece().GetPlayerOwner());

            switch (piece.GetPiece().GetPieceType())
            {
                case PieceTypes.Pawn:
                    Direction[][] PawnMoveSet = MoveSet.GetMoveSet(PieceTypes.Pawn);
                    for (int i = 0; i < PawnMoveSet.Length; i++)
                    {
                        PieceField tempField = MoveStepCheck(piece, PawnMoveSet[i]);
                        if (tempField != null)
                        {
                            try
                            {
                                if ((piece.GetRow() == 1 && i == 1 && GetPieceField(piece.GetBoardPosition() + 1).GetPiece() == null && tempField.GetPiece() == null) // Starting move is the first move in row 2 you can move 2 field
                                || (i >= 2 && tempField.GetPiece() != null && tempField.GetPiece().GetPlayerOwner() != piece.GetPiece().GetPlayerOwner())) // can move diagonal when there are enemy piece in diagnoal field
                                {
                                    if (isKingInCheck == false ||
                                    (isKingInCheck == true && isBlockable == true && blockableField.Count > 0
                                    && blockableField.Contains(tempField)))
                                    {
                                        possibleField.Add(tempField);
                                    }
                                }
                                else if (tempField.GetPiece() == null && i == 0)
                                {
                                    if (isKingInCheck == false ||
                                    (isKingInCheck == true && isBlockable == true && blockableField.Count > 0
                                    && blockableField.Contains(tempField)))
                                    {
                                        possibleField.Add(tempField);
                                    }

                                }
                            }
                            catch (Exception) { };
                        }

                    }
                    break;
                case PieceTypes.Knight:
                    Direction[][] KnightMoveSet = MoveSet.GetMoveSet(PieceTypes.Knight);
                    for (int i = 0; i < KnightMoveSet.Length; i++)
                    {
                        PieceField tempField = MoveStepCheck(piece, KnightMoveSet[i]);
                        try
                        {
                            if ((isKingInCheck == false ||
                            (isKingInCheck == true && isBlockable == true && blockableField.Count > 0
                            && blockableField.Contains(tempField)))
                            && (tempField.GetPiece() == null
                            || (tempField.GetPiece() != null
                            && tempField.GetPiece().GetPlayerOwner() != piece.GetPiece().GetPlayerOwner()))
                            || (isKingInCheck == true && isBlockable == true && blockableField.Count > 0
                            && blockableField.Contains(tempField)))
                            {
                                possibleField.Add(tempField);
                            }
                        }
                        catch (Exception) { };

                    }
                    break;
                case PieceTypes.King:
                    Direction[][] KingMoveSet = MoveSet.GetMoveSet(PieceTypes.King);
                    for (int i = 0; i < KingMoveSet.Length; i++)
                    {
                        PieceField tempField = MoveStepCheck(piece, KingMoveSet[i]);
                        try
                        {
                            if ((tempField.GetPiece() == null || (tempField.GetPiece() != null
                            && tempField.GetPiece().GetPlayerOwner() != piece.GetPiece().GetPlayerOwner()))
                            && ((isKingInCheck == false && kingMovableField.Count > 0
                            && kingMovableField.Contains(tempField))
                            || (isKingInCheck == true && kingMovableField.Count > 0
                            && kingMovableField.Contains(tempField))))
                            {
                                possibleField.Add(tempField);
                            }

                            if (piece.GetPiece().GetCastlable() == true && isKingInCheck == false)
                            {
                                if (piece.GetPiece().GetPlayerOwner() == Players.Player1 && piece.GetBoardPosition() == BoardPosition.BE1)
                                {
                                    if (GetPieceField(BoardPosition.BD1).GetPiece() == null && GetPieceField(BoardPosition.BC1).GetPiece() == null
                                    && GetPieceField(BoardPosition.BB1).GetPiece() == null && GetPieceField(BoardPosition.BA1).GetPiece() != null
                                    && GetPieceField(BoardPosition.BA1).GetPiece().GetPieceType() == PieceTypes.Rook && GetPieceField(BoardPosition.BA1).GetPiece().GetCastlable()
                                    && GetPieceField(BoardPosition.BA1).GetPiece().GetPlayerOwner() == piece.GetPiece().GetPlayerOwner())
                                    {
                                        possibleField.Add(GetPieceField(BoardPosition.BC1));
                                    }

                                    if (GetPieceField(BoardPosition.BF1).GetPiece() == null && GetPieceField(BoardPosition.BG1).GetPiece() == null
                                    && GetPieceField(BoardPosition.BH1).GetPiece() != null
                                    && GetPieceField(BoardPosition.BH1).GetPiece().GetPieceType() == PieceTypes.Rook && GetPieceField(BoardPosition.BH1).GetPiece().GetCastlable()
                                    && GetPieceField(BoardPosition.BA1).GetPiece().GetPlayerOwner() == piece.GetPiece().GetPlayerOwner())
                                    {
                                        possibleField.Add(GetPieceField(BoardPosition.BG1));
                                    }
                                }
                                else if (piece.GetPiece().GetPlayerOwner() == Players.Player2 && piece.GetBoardPosition() == BoardPosition.GE1)
                                {
                                    if (GetPieceField(BoardPosition.GD1).GetPiece() == null && GetPieceField(BoardPosition.GC1).GetPiece() == null
                                    && GetPieceField(BoardPosition.GB1).GetPiece() == null && GetPieceField(BoardPosition.GA1).GetPiece() != null
                                    && GetPieceField(BoardPosition.GA1).GetPiece().GetPieceType() == PieceTypes.Rook && GetPieceField(BoardPosition.GA1).GetPiece().GetCastlable()
                                    && GetPieceField(BoardPosition.GA1).GetPiece().GetPlayerOwner() == piece.GetPiece().GetPlayerOwner())
                                    {
                                        possibleField.Add(GetPieceField(BoardPosition.GC1));
                                    }

                                    if (GetPieceField(BoardPosition.GF1).GetPiece() == null && GetPieceField(BoardPosition.GG1).GetPiece() == null
                                    && GetPieceField(BoardPosition.GH1).GetPiece() != null
                                    && GetPieceField(BoardPosition.GH1).GetPiece().GetPieceType() == PieceTypes.Rook && GetPieceField(BoardPosition.GH1).GetPiece().GetCastlable()
                                    && GetPieceField(BoardPosition.GA1).GetPiece().GetPlayerOwner() == piece.GetPiece().GetPlayerOwner())
                                    {
                                        possibleField.Add(GetPieceField(BoardPosition.GG1));
                                    }
                                }
                                else if (piece.GetPiece().GetPlayerOwner() == Players.Player3 && piece.GetBoardPosition() == BoardPosition.RE1)
                                {
                                    if (GetPieceField(BoardPosition.RD1).GetPiece() == null && GetPieceField(BoardPosition.RC1).GetPiece() == null
                                    && GetPieceField(BoardPosition.RB1).GetPiece() == null && GetPieceField(BoardPosition.RA1).GetPiece() != null
                                    && GetPieceField(BoardPosition.RA1).GetPiece().GetPieceType() == PieceTypes.Rook && GetPieceField(BoardPosition.RA1).GetPiece().GetCastlable()
                                    && GetPieceField(BoardPosition.RA1).GetPiece().GetPlayerOwner() == piece.GetPiece().GetPlayerOwner())
                                    {
                                        possibleField.Add(GetPieceField(BoardPosition.RC1));
                                    }

                                    if (GetPieceField(BoardPosition.RF1).GetPiece() == null && GetPieceField(BoardPosition.RG1).GetPiece() == null
                                    && GetPieceField(BoardPosition.RH1).GetPiece() != null
                                    && GetPieceField(BoardPosition.RH1).GetPiece().GetPieceType() == PieceTypes.Rook && GetPieceField(BoardPosition.RH1).GetPiece().GetCastlable()
                                    && GetPieceField(BoardPosition.RA1).GetPiece().GetPlayerOwner() == piece.GetPiece().GetPlayerOwner())
                                    {
                                        possibleField.Add(GetPieceField(BoardPosition.RG1));
                                    }
                                }
                            }
                        }
                        catch (Exception) { };

                    }
                    break;
                default:
                    Direction[][] LineMoveSet = MoveSet.GetMoveSet(piece.GetPiece().GetPieceType());

                    for (int i = 0; i < LineMoveSet.Length; i++)
                    {
                        bool onLoop = true;

                        PieceField tempField = piece;
                        Players originalColor = tempField.GetPiece().GetPlayerOwner();

                        Direction[] moveDirection = LineMoveSet[i];

                        bool reverse = false;

                        while (onLoop == true)
                        {
                            PieceField oldTempField = tempField;

                            tempField = MoveStepCheck(tempField, moveDirection, reverse, originalColor);

                            if (tempField != null && oldTempField.GetColor() != tempField.GetColor())
                            {
                                reverse = true;
                            }

                            if (tempField == null)
                            {
                                break;
                            }
                            else
                            {
                                try
                                {
                                    if (tempField.GetPiece() == null || (tempField.GetPiece() != null && tempField.GetPiece().GetPlayerOwner() != piece.GetPiece().GetPlayerOwner()))
                                    {
                                        if ((isKingInCheck == false ||
                                        (isKingInCheck == true && isBlockable == true && blockableField.Count > 0
                                        && blockableField.Contains(tempField)))
                                        && (tempField.GetPiece() == null
                                        || (tempField.GetPiece() != null
                                        && tempField.GetPiece().GetPlayerOwner() != piece.GetPiece().GetPlayerOwner()))
                                        || (isKingInCheck == true && isBlockable == true && blockableField.Count > 0
                                        && blockableField.Contains(tempField)))
                                        {
                                            possibleField.Add(tempField);
                                            if (tempField.GetPiece() != null
                                            && tempField.GetPiece().GetPlayerOwner() != piece.GetPiece().GetPlayerOwner())
                                            {
                                                break;
                                            }

                                        }
                                    }
                                    else if (tempField != null && tempField.GetPiece().GetPlayerOwner() == piece.GetPiece().GetPlayerOwner())
                                    {
                                        break;
                                    }
                                    else if (possibleField[possibleField.Count - 1] != null)
                                    {
                                        break;
                                    }
                                }
                                catch (Exception) { };
                            }
                        }


                    }
                    break;
            }
            if (nonMovablePiece.Contains(piece))
            {
                List<PieceField> dupPossibleField = new List<PieceField>();
                dupPossibleField.AddRange(possibleField);
                List<PieceField> tempPossibleField = new List<PieceField>();

                for (int i = 0; i < dupPossibleField.Count; i++)
                {
                    if (nonMovablePiece_movableField.Contains(dupPossibleField[i]))
                    {
                        tempPossibleField.Add(dupPossibleField[i]);
                    }
                }

                if (tempPossibleField.Count > 0)
                {
                    possibleField = new List<PieceField>();
                    possibleField.AddRange(tempPossibleField);
                }
                else
                {
                    return new List<PieceField>();
                }

            }
        }

        return possibleField;
    }

    public void MoveAction(PieceField startingPosition, PieceField endingPosition, int time = 0)
    {
        if (IsLegalMove(startingPosition, endingPosition))
        {
            PieceField tempStartingPos = startingPosition;
            PieceField tempEndingPos = endingPosition;

            Pieces moverPiece = startingPosition.GetPiece();
            Pieces takenPiece = endingPosition.GetPiece();

            PieceField mover = startingPosition;
            PieceField taken = endingPosition;

            Pieces pawnPromotionPiece = null;

            PieceField oldRook = null;
            PieceField newRook = null;

            PieceField tempOldRook = oldRook;
            PieceField tempNewRook = newRook;

            if (taken.GetPiece() != null)
            {
                if (taken.GetPiece().GetPieceType() == PieceTypes.King)
                {
                    GameOver(mover.GetPiece().GetPlayerOwner(), taken.GetPiece().GetPlayerOwner());
                }
            }

            if (mover.GetPiece().GetPieceType() == PieceTypes.Pawn && endingPosition.GetRow() == 0
            && endingPosition.GetColor() != mover.GetPiece().GetPlayerOwner())
            {
                //promotePawn
                promotePawnColor = mover.GetPiece().GetPlayerOwner();
                togglePromotePawn = true;
                isPlayable = false;
                endingPosition.SetPiece(mover.GetPiece());
                pawnPromotePieceField = endingPosition;
                pawnPromoteActive = true;
                backwardButton.interactable = false;
                forwardButton.interactable = false;
                passTurnButton.interactable = false;

                if (LanMode)
                {
                    for (int i = 0; i < Server.instance.connectedClients.Count; i++)
                    {
                        if (Server.instance.connectedClients[i].Player == mover.GetPiece().GetPlayerOwner()
                        && !Server.instance.connectedClients[i].isLocal)
                        {
                            Server.instance.connectedClients[i].gameService.SendNeedPromotion(endingPosition.GetBoardPosition().ToString());
                            break;
                        }
                    }
                }
                StartCoroutine(WaitForPawnPromote(() => { Debug.Log("Pawn promotion is complete!"); }));

            }
            else
            {
                endingPosition.SetPiece(mover.GetPiece());
                if (mover.GetPiece().GetPieceType() == PieceTypes.King)
                {
                    if (mover.GetPiece().GetPlayerOwner() == Players.Player1)
                    {
                        player1KingPieceField = endingPosition;
                    }
                    else if (mover.GetPiece().GetPlayerOwner() == Players.Player2)
                    {
                        player2KingPieceField = endingPosition;
                    }
                    else if (mover.GetPiece().GetPlayerOwner() == Players.Player3)
                    {
                        player3KingPieceField = endingPosition;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }

            if (mover.GetPiece().GetPieceType() == PieceTypes.King
            && mover.GetColumn() == 4
            && mover.GetRow() == 0
            && mover.GetPiece().GetCastlable() == true)
            {
                if (endingPosition.GetColumn() == 2
                && GetPieceField((BoardPosition)((int)endingPosition.GetBoardPosition() - 8)).GetPiece().GetPieceType() == PieceTypes.Rook
                && GetPieceField((BoardPosition)((int)endingPosition.GetBoardPosition() - 8)).GetPiece().GetCastlable() == true)
                {
                    oldRook = GetPieceField((BoardPosition)((int)endingPosition.GetBoardPosition() - 8));
                    newRook = GetPieceField((BoardPosition)((int)endingPosition.GetBoardPosition() + 4));

                    tempOldRook = oldRook;
                    tempNewRook = newRook;

                    PieceField rook = GetPieceField((BoardPosition)((int)endingPosition.GetBoardPosition() - 8));
                    GetPieceField((BoardPosition)((int)endingPosition.GetBoardPosition() + 4)).SetPiece(rook.GetPiece());
                    GetPieceField((BoardPosition)((int)endingPosition.GetBoardPosition() + 4)).GetPiece().SetCastlable(false);
                    GetPieceField((BoardPosition)((int)endingPosition.GetBoardPosition() - 8)).SetPiece(null);
                    UpdatePieceFieldImage(GetPieceField((BoardPosition)((int)endingPosition.GetBoardPosition() + 4)));
                    UpdatePieceFieldImage(GetPieceField((BoardPosition)((int)endingPosition.GetBoardPosition() - 8)));

                }
                else if (endingPosition.GetColumn() == 6
                && GetPieceField((BoardPosition)((int)endingPosition.GetBoardPosition() + 4)).GetPiece().GetPieceType() == PieceTypes.Rook
                && GetPieceField((BoardPosition)((int)endingPosition.GetBoardPosition() + 4)).GetPiece().GetCastlable() == true)
                {
                    oldRook = GetPieceField((BoardPosition)((int)endingPosition.GetBoardPosition() + 4));
                    newRook = GetPieceField((BoardPosition)((int)endingPosition.GetBoardPosition() - 4));

                    tempOldRook = oldRook;
                    tempNewRook = newRook;

                    PieceField rook = GetPieceField((BoardPosition)((int)endingPosition.GetBoardPosition() + 4));
                    GetPieceField((BoardPosition)((int)endingPosition.GetBoardPosition() - 4)).SetPiece(rook.GetPiece());
                    GetPieceField((BoardPosition)((int)endingPosition.GetBoardPosition() - 4)).GetPiece().SetCastlable(false);
                    GetPieceField((BoardPosition)((int)endingPosition.GetBoardPosition() + 4)).SetPiece(null);
                    UpdatePieceFieldImage(GetPieceField((BoardPosition)((int)endingPosition.GetBoardPosition() - 4)));
                    UpdatePieceFieldImage(GetPieceField((BoardPosition)((int)endingPosition.GetBoardPosition() + 4)));

                }
            }
            startingPosition.SetPiece(null);
            ResetMovableField();
            ResetMovableAuraField();

            UpdatePieceFieldImage(endingPosition);
            UpdatePieceFieldImage(startingPosition);


            bool beforeCaslte = endingPosition.GetPiece().GetCastlable();
            endingPosition.GetPiece().SetCastlable(false);

            SetSelectedPiece(null);

            for (int i = 0; i < 3; i++)
            {
                if ((Players)(i + 1) == moverPiece.GetPlayerOwner())
                {
                    continue;
                }
                else
                {
                    (bool isKingInCheck, List<PieceField> kingMovableField, bool isBlockable, List<PieceField> blockableField, List<PieceField> nonMovablePiece, List<PieceField> nonMovablePiece_movableField, List<Players> playerCheck) = IsKingInCheck((Players)(i + 1));

                    List<PieceField> allPieceMovableFields = ReturnAllPieceMovableField((Players)(i + 1));
                    if (isKingInCheck == true && kingMovableField.Count <= 0 && ((isBlockable == false && blockableField.Count <= 0) || allPieceMovableFields.Count == 0))
                    {
                        if (playerCheck.Count == 1)
                        {
                            GameOver(playerCheck[0], (Players)(i + 1));
                        }
                        else if (playerCheck.Count == 2)
                        {

                            List<Players> tempPlayersCheck = new List<Players>();

                            tempPlayersCheck.AddRange(playerCheck);
                            tempPlayersCheck = tempPlayersCheck.Where(item => item != currentTurnPlayer).ToList();

                            if (tempPlayersCheck.Count == 1)
                            {
                                GameOver(tempPlayersCheck[0], (Players)(i + 1));
                            }

                        }

                    }

                }
            }


            History.Push(new Record(tempStartingPos, moverPiece, tempEndingPos, takenPiece, beforeCaslte, tempOldRook, tempNewRook, pawnPromotionPiece));
            if (!togglePromotePawn)
            {
                UpdateButton();
            }


            if (!pawnPromoteActive)
            {
                ChangeCurrentTurnPlayer();
            }
        }
        else
        {
            throw new Exception() { };
        }
    }
    #endregion

    #region KingSection
    public PieceField FindKingPieceField(Players players)
    {
        switch (players)
        {
            case Players.Player1:
                return player1KingPieceField;
            case Players.Player2:
                return player2KingPieceField;
            case Players.Player3:
                return player3KingPieceField;
            default:
                return null;
        }
    }

    public (bool, List<PieceField>, bool, List<PieceField>, List<PieceField>, List<PieceField>, List<Players>) IsKingInCheck(Players players)
    {
        bool kingInCheck = false;
        List<PieceField> safeFieldForKing = new List<PieceField>();
        List<PieceField> blockableField = new List<PieceField>();
        List<PieceField> nonMovablePiece = new List<PieceField>();
        List<PieceField> nonMovablePiece_movableField = new List<PieceField>();
        List<Players> playerCheck = new List<Players>();

        var (knightOrPawnCheck, _playerCheck1) = IsKnightOrPawnCheckingKing(players);
        var (rookOrBishopCheck, isBlockable, _nonMovablePiece, _nonMovablePiece_movableField, _playerCheck2) = IsKingInLineOfCheck(players);
        nonMovablePiece_movableField.AddRange(_nonMovablePiece_movableField);

        playerCheck.AddRange(_playerCheck1);
        playerCheck.AddRange(_playerCheck2);
        if (playerCheck.Count > 0)
        {
            playerCheck = playerCheck.Distinct().ToList();
        }

        nonMovablePiece.AddRange(_nonMovablePiece);
        if (knightOrPawnCheck.Count > 0)
        {
            kingInCheck = true;

            if (knightOrPawnCheck.Count > 1)
            {
                isBlockable = false;
            }
            else
            {
                var (tempRookOrBishopCheck, tempIsBlockable, _, __, ___) = IsKingInLineOfCheck(players, knightOrPawnCheck[0]);
                if (tempRookOrBishopCheck.Count == 0)
                {
                    blockableField.AddRange(knightOrPawnCheck);
                }
                else
                {
                    isBlockable = tempIsBlockable;
                    if (isBlockable)
                    {
                        for (int i = 0; i < knightOrPawnCheck.Count; i++)
                        {
                            if (tempRookOrBishopCheck.Contains(knightOrPawnCheck[i]))
                            {
                                blockableField.Add(knightOrPawnCheck[i]);
                            }
                        }
                    }

                }



            }
        }

        if (rookOrBishopCheck.Count > 0)
        {
            kingInCheck = true;

            if (isBlockable)
            {
                if (blockableField.Count == 0)
                {
                    blockableField.AddRange(rookOrBishopCheck);
                }
                else
                {
                    isBlockable = false;
                }
            }
        }

        var moveSet = MoveSet.GetMoveSet(PieceTypes.King);
        var kingPieceField = FindKingPieceField(players);

        foreach (var directions in moveSet)
        {
            var tempKingMoveField = MoveStepCheck(kingPieceField, directions);
            if (tempKingMoveField != null)
            {
                if ((!blockableField.Contains(tempKingMoveField) && tempKingMoveField.GetPiece() == null) || (blockableField.Contains(tempKingMoveField) && tempKingMoveField.GetPiece() != null && tempKingMoveField.GetPiece().GetPlayerOwner() != players))
                {
                    var KMFSafeFromKnightOrPawn = IsKnightOrPawnCheckingKing(players, tempKingMoveField).Item1;
                    var KMFSafeFromRookOrBishop = IsKingInLineOfCheck(players, tempKingMoveField).Item1;

                    if (KMFSafeFromKnightOrPawn.Count == 0 && KMFSafeFromRookOrBishop.Count == 0)
                    {
                        safeFieldForKing.Add(tempKingMoveField);
                    }
                }
            }

        }

        return (kingInCheck, safeFieldForKing, isBlockable, blockableField, nonMovablePiece, nonMovablePiece_movableField, playerCheck);
    }

    public (List<PieceField>, List<Players>) IsKnightOrPawnCheckingKing(Players players, PieceField pieceField = null)
    {
        List<PieceField> checkerPieceField = new List<PieceField>();
        List<Players> checkerPlayer = new List<Players>();

        Direction[][] moveDirections = new Direction[MoveSet.GetMoveSet(PieceTypes.Knight).Length + MoveSet.GetMoveSet(PieceTypes.Pawn).Length][];
        MoveSet.GetMoveSet(PieceTypes.Knight).CopyTo(moveDirections, 0);
        MoveSet.GetMoveSet(PieceTypes.Pawn).CopyTo(moveDirections, MoveSet.GetMoveSet(PieceTypes.Knight).Length);

        if (pieceField == null)
        {
            switch (players)
            {
                case Players.Player1:
                    pieceField = player1KingPieceField;
                    break;
                case Players.Player2:
                    pieceField = player2KingPieceField;
                    break;
                case Players.Player3:
                    pieceField = player3KingPieceField;
                    break;
                default:
                    throw new Exception();
            }
        }

        if (pieceField != null)
        {
            for (int i = 0; i < moveDirections.Length; i++)
            {
                Direction[] directions = moveDirections[i];
                PieceField testField = MoveStepCheck(pieceField, directions);
                bool isReverse = false;
                if (testField != null)
                {
                    if (testField.GetColor() != pieceField.GetColor())
                    {
                        isReverse = true;
                    }
                }

                PieceField tempField = MoveStepCheck(pieceField, directions, isReverse);
                if (tempField != null)
                {
                    var piece = tempField.GetPiece();
                    if (piece != null &&
                        ((i > 0 && i < MoveSet.GetMoveSet(PieceTypes.Knight).Length && piece.GetPieceType() == PieceTypes.Knight) || (i >= (MoveSet.GetMoveSet(PieceTypes.Knight).Length + 2) && piece.GetPieceType() == PieceTypes.Pawn)) && piece.GetPlayerOwner() != players &&
                        !checkerPieceField.Contains(tempField))
                    {
                        checkerPieceField.Add(tempField);
                    }

                    if (checkerPlayer.Count > 0)
                    {
                        if (!checkerPlayer.Contains(tempField.GetPiece().GetPlayerOwner()))
                        {
                            checkerPlayer.Add(tempField.GetPiece().GetPlayerOwner());
                        }
                    }

                }
            }
        }

        return (checkerPieceField, checkerPlayer);
    }

    public (List<PieceField>, bool, List<PieceField>, List<PieceField>, List<Players>) IsKingInLineOfCheck(Players players, PieceField pieceField = null)
    {
        List<PieceField> checkerPieceField = new List<PieceField>();
        List<PieceField> nonMovableField = new List<PieceField>();
        List<PieceField> nonMovablePiece_movableField = new List<PieceField>();
        List<Players> checkerPlayer = new List<Players>();
        Direction[][] moveDirections = ConcatArrays(
            MoveSet.GetMoveSet(PieceTypes.Rook),
            MoveSet.GetMoveSet(PieceTypes.Bishop)
        );
        bool isBlockable = true;

        if (pieceField == null)
        {
            switch (players)
            {
                case Players.Player1:
                    pieceField = player1KingPieceField;
                    break;
                case Players.Player2:
                    pieceField = player2KingPieceField;
                    break;
                case Players.Player3:
                    pieceField = player3KingPieceField;
                    break;
                default:
                    throw new Exception();
            }
        }

        foreach (Direction[] directions in moveDirections)
        {
            List<PieceField> tempFieldArray = new List<PieceField>();
            List<PieceField> tempToCheckFurther = new List<PieceField>();
            List<PieceField> tempNonMovablePiece_MovableField = new List<PieceField>();
            bool onLoop = true;
            PieceField tempField = pieceField;
            bool reverse = false;
            PieceField testField = MoveStepCheck(tempField, directions, reverse);
            bool boolToCheckFurther = false;
            List<PieceField> tempNonMovablePiece = new List<PieceField>();

            if (!checkerPieceField.Contains(testField))
            {
                int i = 0;
                while (onLoop)
                {
                    PieceField oldTempField = tempField;

                    tempField = MoveStepCheck(tempField, directions, reverse);

                    if (tempField != null && oldTempField.GetColor() != tempField.GetColor())
                    {
                        reverse = true;
                    }

                    if (tempField == null)
                    {
                        onLoop = false;
                        break;
                    }

                    var piece = tempField.GetPiece();
                    if (piece != null)
                    {
                        if (piece.GetPlayerOwner() == players)
                        {
                            if (tempNonMovablePiece.Count > 0)
                            {
                                onLoop = false;
                                break;
                            }
                            else
                            {
                                tempNonMovablePiece.Add(tempField);
                            }

                        }
                        else
                        {
                            if (((directions.Length == 1 &&
                                (piece.GetPieceType() == PieceTypes.Rook || piece.GetPieceType() == PieceTypes.Queen || (i == 0 && piece.GetPieceType() == PieceTypes.King)))
                                || (directions.Length > 1 &&
                                (piece.GetPieceType() == PieceTypes.Bishop || piece.GetPieceType() == PieceTypes.Queen || (i == 0 && piece.GetPieceType() == PieceTypes.King))))
                                && piece.GetPlayerOwner() != players)
                            {
                                if (boolToCheckFurther)
                                {
                                    tempFieldArray.RemoveAll(pf => tempToCheckFurther.Contains(pf));
                                }
                                else
                                {
                                    tempFieldArray.Add(tempField);
                                }

                                if (!checkerPlayer.Contains(tempField.GetPiece().GetPlayerOwner()))
                                {
                                    checkerPlayer.Add(tempField.GetPiece().GetPlayerOwner());
                                }

                                if (checkerPieceField.Count == 0)
                                {
                                    tempNonMovablePiece_MovableField.Add(tempField);
                                }

                                if (checkerPieceField.Count > 0 && isBlockable)
                                {
                                    boolToCheckFurther = true;
                                    isBlockable = false;
                                }
                                else if (tempNonMovablePiece.Count > 0)
                                {
                                    nonMovableField.AddRange(tempNonMovablePiece);
                                    nonMovablePiece_movableField.AddRange(tempNonMovablePiece_MovableField);
                                }
                                else
                                {
                                    checkerPieceField.AddRange(tempFieldArray);
                                }



                            }
                            else if ((directions.Length == 1 &&
                                (piece.GetPieceType() != PieceTypes.Rook || piece.GetPieceType() != PieceTypes.Queen))
                                || (directions.Length > 1 &&
                                (piece.GetPieceType() != PieceTypes.Bishop || piece.GetPieceType() != PieceTypes.Queen))
                                || (i == 0 && piece.GetPieceType() != PieceTypes.King && piece.GetPlayerOwner() != players))
                            {
                                onLoop = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (boolToCheckFurther == true)
                        {
                            tempToCheckFurther.Add(tempField);
                        }

                        if (tempNonMovablePiece.Count == 0)
                        {
                            tempFieldArray.Add(tempField);
                        }

                        if (checkerPieceField.Count == 0)
                        {
                            tempNonMovablePiece_MovableField.Add(tempField);
                        }

                    }
                    i++;
                }
            }

        }

        return (checkerPieceField, isBlockable, nonMovableField, nonMovablePiece_movableField, checkerPlayer);
    }

    private Direction[][] ConcatArrays(Direction[][] array1, Direction[][] array2)
    {
        Direction[][] result = new Direction[array1.Length + array2.Length][];
        array1.CopyTo(result, 0);
        array2.CopyTo(result, array1.Length);
        return result;
    }
    #endregion

    public void GameOver(Players whoWon, Players whoGotChecked)
    {
        gameOver = true;
        isPlayable = false;

        UpdateGameOverText(whoWon, whoGotChecked);

        gameOverTextUI.gameObject.SetActive(true);
    }

    public void UpdateGameOverText(Players whoWon, Players whoGotChecked)
    {
        if (whoWon != Players.Empty)
        {
            gameOverTextUI.text = $"<color={GetPlayerColor((int)whoWon)}>Player {(int)whoWon}</color>" + " wins!";
        }

        checkmatedTextUi.GetComponent<TextMeshProUGUI>().text = $"<color={GetPlayerColor((int)whoGotChecked)}>Player {(int)whoGotChecked}</color>" + " got checkmated!";
    }

    public List<PieceField> ReturnAllPieceMovableField(Players player)
    {
        List<PieceField> movableFieldsForAllPice = new List<PieceField>();

        for (int i = 0; i < 96; i++)
        {
            var tempField = GetPieceField((BoardPosition)i);

            if (tempField.GetPiece() == null || (tempField.GetPiece() != null && tempField.GetPiece().GetPlayerOwner() != player))
            {
                continue;
            }
            else
            {
                List<PieceField> movableField = MovablePieceField(tempField);

                if (movableField.Count > 0)
                {
                    movableFieldsForAllPice.AddRange(movableField);
                }
            }
        }

        return movableFieldsForAllPice;
    }

    string GetPlayerColor(int player)
    {
        switch (player)
        {
            case 1:
                return "blue";
            case 2:
                return "green";
            case 3:
                return "red";
            default:
                return "white"; // Default color
        }
    }


    public IEnumerator SetPawnPromoteActive()
    {
        if (togglePromotePawn == true)
        {
            if (promotePawnColor != Players.Empty)
            {
                UpdatePromotePawnUIColor();
                pawnPromotePanel.SetActive(true);
            }
            else
            {
                yield break; // End the coroutine if there's nothing to do
            }
        }
        else
        {
            yield break; // End the coroutine if promotion is not active
        }
    }

    private IEnumerator WaitForPawnPromote(Action action)
    {
        yield return StartCoroutine(SetPawnPromoteActive());

        while (togglePromotePawn)
        {
            yield return null;
        }

        action.Invoke();
    }

    public void PromotePawnAction(int i)
    {
        switch (i)
        {
            case 0:
                if (promotePawnColor == Players.Player1)
                {
                    pawnPromotePieceField.SetPiece(blueQueen);
                }
                else if (promotePawnColor == Players.Player2)
                {
                    pawnPromotePieceField.SetPiece(greenQueen);
                }
                else if (promotePawnColor == Players.Player3)
                {
                    pawnPromotePieceField.SetPiece(redQueen);
                }
                break;
            case 1:
                if (promotePawnColor == Players.Player1)
                {
                    pawnPromotePieceField.SetPiece(blueKnight);
                }
                else if (promotePawnColor == Players.Player2)
                {
                    pawnPromotePieceField.SetPiece(greenKnight);
                }
                else if (promotePawnColor == Players.Player3)
                {
                    pawnPromotePieceField.SetPiece(redKnight);
                }
                break;
            case 2:
                if (promotePawnColor == Players.Player1)
                {
                    pawnPromotePieceField.SetPiece(blueRook);
                }
                else if (promotePawnColor == Players.Player2)
                {
                    pawnPromotePieceField.SetPiece(greenRook);
                }
                else if (promotePawnColor == Players.Player3)
                {
                    pawnPromotePieceField.SetPiece(redRook);
                }
                break;
            case 3:
                if (promotePawnColor == Players.Player1)
                {
                    pawnPromotePieceField.SetPiece(blueBishop);
                }
                else if (promotePawnColor == Players.Player2)
                {
                    pawnPromotePieceField.SetPiece(greenBishop);
                }
                else if (promotePawnColor == Players.Player3)
                {
                    pawnPromotePieceField.SetPiece(redBishop);
                }
                break;
            default:
                return;
        }
        var newPiece = pawnPromotePieceField.GetPiece();
        if (newPiece == null)
        {
            Debug.LogError("Pawn promotion piece is null");
        }
        else
        {
            History.history[History.history.Count - 1].SetPromotionPawnNewPiece(newPiece);
        }

        var lastRecord = History.history[History.history.Count - 1];
        lastRecord.SetPromotionPawnNewPiece(newPiece);
        History.history[History.history.Count - 1] = lastRecord;
        UpdatePieceFieldImage(pawnPromotePieceField);

        pawnPromotePieceField = null;

        promotePawnColor = Players.Empty;
        togglePromotePawn = false;
        isPlayable = true;
        if (History.history.Count > 0)
        {
            backwardButton.interactable = true;
        }

        if (LanMode)
        {
            passTurnButton.interactable = false;
            for (int m = 0; m < Server.instance.connectedClients.Count; m++)
            {
                if (Server.instance.connectedClients[i].isLocal == true && Server.instance.connectedClients[i].Player == currentTurnPlayer)
                {
                    passTurnButton.interactable = true;
                    break;
                }
            }
        }
        else
        {
            passTurnButton.interactable = true;
        }

        pawnPromoteActive = false;
        pawnPromotePanel.SetActive(false);
        ChangeCurrentTurnPlayer();
    }

    public void PassTurn()
    {
        History.Push(new Record(null, null, null, null, false, null, null, null));
        UpdateButton();
        selectedPiece = null;
        ChangeCurrentTurnPlayer();
    }

    public void GatherPieceFieldData()
    {
        boardState = new List<string>();
        for (int i = 0; i < 96; i++)
        {
            PieceField pieceField = GetPieceField((BoardPosition)i);
            if (pieceField != null && pieceField.GetPiece() != null)
            {
                pieceFieldServerCheck = pieceField;
            }
            // Check if there is a scriptable piece in the slot
            if (pieceFieldServerCheck != null && pieceFieldServerCheck.GetPiece() != null)
            {
                // Add relevant information about the scriptable piece to the board state
                string pieceInfo = $"{{\"Field\": \"{(BoardPosition)i}\", \"Piece\": \"{pieceFieldServerCheck.GetPiece().GetPieceType()}\", \"Owner\": \"{pieceFieldServerCheck.GetPiece().GetPlayerOwner()}\"}}";
                boardState.Add(pieceInfo);
            }
            pieceFieldServerCheck = null;
        }
        dataGatheringCompleted = true;
    }

    public void GatherPieceFieldDataPlayer(int player)
    {
        boardState = new List<string>();
        for (int i = 0; i < 96; i++)
        {
            PieceField pieceField = GetPieceField((BoardPosition)i);
            if (pieceField != null && pieceField.GetPiece() != null)
            {
                pieceFieldServerCheck = pieceField;
            }
            // Check if there is a scriptable piece in the slot
            if (pieceFieldServerCheck != null && pieceFieldServerCheck.GetPiece() != null)
            {
                if ((int)pieceFieldServerCheck.GetPiece().GetPlayerOwner() == player)
                {
                    // Add relevant information about the scriptable piece to the board state
                    string pieceInfo = $"{{\"Field\": \"{(BoardPosition)i}\", \"Piece\": \"{pieceFieldServerCheck.GetPiece().GetPieceType()}\", \"Owner\": \"{pieceFieldServerCheck.GetPiece().GetPlayerOwner()}\"}}";
                    boardState.Add(pieceInfo);
                }
            }
            pieceFieldServerCheck = null;
        }
        dataGatheringBoardPlayerCompleted = true;
    }


    public static int Decode(string position)
    {
        if (position.Length != 3)
        {
            throw new ArgumentException("Invalid position format. It should be a three-character string.");
        }

        char color = position[0];
        char column = position[1];
        char row = position[2];

        int colorValue, columnValue, rowValue;

        switch (color)
        {
            case 'B':
                colorValue = 0;
                break;
            case 'G':
                colorValue = 32;
                break;
            case 'R':
                colorValue = 64;
                break;
            default:
                throw new ArgumentException("Invalid color. It should be 'B', 'G', or 'R'.");
        }

        switch (column)
        {
            case 'A':
                columnValue = 0;
                break;
            case 'B':
                columnValue = 4;
                break;
            case 'C':
                columnValue = 8;
                break;
            case 'D':
                columnValue = 12;
                break;
            case 'E':
                columnValue = 16;
                break;
            case 'F':
                columnValue = 20;
                break;
            case 'G':
                columnValue = 24;
                break;
            case 'H':
                columnValue = 28;
                break;
            default:
                throw new ArgumentException("Invalid column. It should be 'A' through 'H'.");
        }

        if (!char.IsDigit(row) || row < '1' || row > '4')
        {
            throw new ArgumentException("Invalid row. It should be a digit between 1 and 4.");
        }

        rowValue = row - '1';

        return colorValue + columnValue + rowValue;
    }

    public void GameServiceMoveCommand(int i)
    {
        try
        {
            var startingPosition = GetPieceField((BoardPosition)Decode(gsFrom));
            var endingPosition = GetPieceField((BoardPosition)Decode(gsTo));

            List<PieceField> movableFields = MovablePieceField(startingPosition);
            if (movableFields.Contains(endingPosition))
            {
                MoveAction(startingPosition, endingPosition);
                Server.instance.connectedClients[i].gameService.SendMoveCommandSuccess();
            }
            else
            {
                Server.instance.connectedClients[i].gameService.SendMoveCommandFail();
            }

            gsFrom = "";
            gsTo = "";
        }
        catch (Exception)
        {
            Server.instance.connectedClients[i].gameService.SendMoveCommandError();
        };
    }

    public void GameServiceMovableFieldCommand(int i)
    {
        try
        {
            var movableField = GetPieceField((BoardPosition)Decode(gsMovableField));

            movableFieldsList = new List<string>();
            List<PieceField> movableFields = MovablePieceField(movableField);
            if (movableField.GetPiece() != null)
            {
                if (Server.instance.connectedClients[i].Player == movableField.GetPiece().GetPlayerOwner())
                {
                    if (movableFields.Count > 0)
                    {
                        for (int j = 0; j < movableFields.Count; j++)
                        {
                            string pieceInfo = $"{{\"Field\": \"{movableFields[j].GetBoardPosition()}\"}}";
                            movableFieldsList.Add(pieceInfo);
                        }

                        movableFieldsListJson = "[" + string.Join(",", movableFieldsList) + "]";

                        Server.instance.connectedClients[i].gameService.SendMovableFieldCommandSuccess();
                    }
                    else
                    {
                        Server.instance.connectedClients[i].gameService.SendMovableFieldCommandFail();
                    }

                    gsMovableField = "";
                    movableFieldsList = new List<string>();
                }
                else
                {
                    Server.instance.connectedClients[i].gameService.SendMovableFieldCommandFieldIsNotOwn();
                }
            }
            else
            {
                Server.instance.connectedClients[i].gameService.SendMovableFieldCommandFieldIsNull();
            }
        }
        catch (Exception)
        {
            Server.instance.connectedClients[i].gameService.SendMoveCommandError();
        };
    }

    public void CheckingForKingIfChecked(int i)
    {
        (bool isKingInCheck, List<PieceField> kingMovableField, bool isBlockable, List<PieceField> blockableField, List<PieceField> nonMovablePiece, List<PieceField> nonMovablePiece_movableField, List<Players> playerCheck) = IsKingInCheck(Server.instance.connectedClients[i].Player);
        StatusMessage statusMessage = new StatusMessage { };
        if (isKingInCheck == false)
        {
            List<string> kingMovableFieldStr = new List<string>();
            kingMovableFieldJson = "";
            for (int j = 0; j < kingMovableField.Count; j++)
            {
                string pieceInfo = $"{{\"Field\": \"{kingMovableField[j].GetBoardPosition()}\"}}";
                kingMovableFieldStr.Add(pieceInfo);
            }

            kingMovableFieldJson = "[" + string.Join(",", kingMovableFieldStr) + "]";

            // List<string> blockableFieldStr = new List<string>();
            // blockableFieldJson = "";
            // for (int k = 0; k < blockableField.Count; k++)
            // {
            //     string pieceInfo = $"{{\"Field\": \"{kingMovableField[k].GetBoardPosition()}\"}}";
            //     blockableFieldStr.Add(pieceInfo);
            // }
            // blockableFieldJson = "[" + string.Join(",", blockableFieldStr) + "]";

            statusMessage = new StatusMessage
            {
                Status = "Success",
                Message = "You're not in checked. Check KingMovableField for fields that king can move, If have nothing it means your king can't move. ",
                Player = "",
                Password = "",
                YourTurn = null,
                Board = "",
                MovableFields = "",
                NeedPromotion = "",

                KingInCheck = isKingInCheck,
                KingMovableField = kingMovableFieldJson,
                Blockable = null,
                BlockableField = "",
                WhoChecked = ""
            };
            kingMovableFieldJson = "";
        }
        else
        {
            List<string> kingMovableFieldStr = new List<string>();
            kingMovableFieldJson = "";
            for (int j = 0; j < kingMovableField.Count; j++)
            {
                string pieceInfo = $"{{\"Field\": \"{kingMovableField[j].GetBoardPosition()}\"}}";
                kingMovableFieldStr.Add(pieceInfo);
            }

            kingMovableFieldJson = "[" + string.Join(",", kingMovableFieldStr) + "]";

            List<string> blockableFieldStr = new List<string>();
            blockableFieldJson = "";
            for (int k = 0; k < blockableField.Count; k++)
            {
                string pieceInfo = $"{{\"Field\": \"{blockableField[k].GetBoardPosition()}\"}}";
                blockableFieldStr.Add(pieceInfo);
            }
            blockableFieldJson = "[" + string.Join(",", blockableFieldStr) + "]";

            List<string> whoCheckStr = new List<string>();
            whoCheckedJson = "";
            for (int l = 0; l < playerCheck.Count; l++)
            {
                string playerInfo = $"{{\"Player\": \"{playerCheck[l]}\"}}";
                whoCheckStr.Add(playerInfo);
            }
            whoCheckedJson = "[" + string.Join(",", whoCheckStr) + "]";

            statusMessage = new StatusMessage
            {
                Status = "Success",
                Message = "You're in checked. Check KingMovableField for fields that king can move, If have nothing it means your king can't move. Check Blockable for king is blockable. and BlockableField for field that have to be block to protect king. ",
                Player = "",
                Password = "",
                YourTurn = null,
                Board = "",
                MovableFields = "",
                NeedPromotion = "",

                KingInCheck = isKingInCheck,
                KingMovableField = kingMovableFieldJson,
                Blockable = isBlockable,
                BlockableField = blockableFieldJson,
                WhoChecked = whoCheckedJson
            };

            kingMovableFieldJson = "";
            blockableFieldJson = "";
            whoCheckedJson = "";
            gsCurrentClient = -1;
        }

        Server.instance.connectedClients[i].gameService.KingInCheck(statusMessage);
    }
}