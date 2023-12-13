using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;

public class Server : MonoBehaviour
{
    private WebSocketServer server;

    public GameObject uiPlayer1;
    public GameObject uiPlayer2;
    public GameObject uiPlayer3;

    public int playerJoinCount = 0;

    public static Server instance;

    public string machineIP;



    public List<ClientData> connectedClients = new List<ClientData>();

    public bool player1Leave;
    public bool player2Leave;
    public int connectedClientsForGameService = -1;

    public int connectedClientsForGameServiceMyPiece = -1;
    public int playerForGameServiceMyPiece = -1;

    void OnEnable()
    {
        instance = this;
    }

    void OnDisable()
    {
        instance = null;
    }

    private void Start()
    {

    }

    private void Update()
    {
    }

    public void StartServer()
    {
        IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

        foreach (IPAddress ipAddress in localIPs)
        {
            if (ipAddress.AddressFamily == AddressFamily.InterNetwork) // IPv4 addresses
            {
                machineIP = $"ws://{ipAddress}:8181/";
            }
        }

        server = new WebSocketServer(machineIP);
        server.AddWebSocketService<GameService>("/game");
        server.Start();
        Debug.Log("Server Started");

    }

    public void StopServer()
    {
        if (server != null)
        {
            server.Stop();
            playerJoinCount = 0;
            player1Leave = false;
            player2Leave = false;
            server = null;
        }
        Debug.Log("Server is stopped");
    }

    private void OnDestroy()
    {
        if (server != null)
        {
            server.Stop();
            server = null;
        }
    }

    public void APIPlayerJoin(Players players)
    {
        playerJoinCount++;
        if (players == Players.Player1)
        {
            MainThreadDispatcher.instance.RunOnMainThread(() =>
            {
                uiPlayer1.SetActive(true);
                uiPlayer1.transform.GetChild(1).gameObject.SetActive(false);
            });
        }
        else if (players == Players.Player2)
        {
            MainThreadDispatcher.instance.RunOnMainThread(() =>
            {
                uiPlayer2.SetActive(true);
                uiPlayer2.transform.GetChild(1).gameObject.SetActive(false);
            });
        }
        else if (players == Players.Player3)
        {
            MainThreadDispatcher.instance.RunOnMainThread(() =>
            {
                uiPlayer3.SetActive(true);
                uiPlayer3.transform.GetChild(1).gameObject.SetActive(false);

            });
        }

        if (playerJoinCount == 3)
        {
            MainThreadDispatcher.instance.RunOnMainThread(() =>
            {
                GameManager.instance.lobbyUI.transform.GetChild(0).gameObject.SetActive(true);
            });
        }
    }

    public void ManualPlayerAdd()
    {
        playerJoinCount++;
        ClientData client = new ClientData();
        if (!player1Leave && !player2Leave)
        {
            client = new ClientData
            {
                Password = "-",
                Player = (Players)playerJoinCount,
                gameService = null,
                isLocal = true
            };
            connectedClients.Add(client);

        }
        else if (player1Leave)
        {
            client = new ClientData
            {
                Password = "-",
                Player = Players.Player1,
                gameService = null,
                isLocal = true
            };
            connectedClients.Insert(0, client);
            player1Leave = false;
        }
        else if (player2Leave)
        {
            client = new ClientData
            {
                Password = "-",
                Player = Players.Player2,
                gameService = null,
                isLocal = true
            };
            if (connectedClients.Count > 1)
            {
                connectedClients.Insert(1, client);
            }
            else
            {
                connectedClients.Add(client);
            }
            player2Leave = false;
        }


        if (client.Player == Players.Player1)
        {
            MainThreadDispatcher.instance.RunOnMainThread(() =>
            {
                uiPlayer1.SetActive(true);
                uiPlayer1.transform.GetChild(1).gameObject.SetActive(true);
            });
        }
        else if (client.Player == Players.Player2)
        {
            MainThreadDispatcher.instance.RunOnMainThread(() =>
            {
                uiPlayer2.SetActive(true);
                uiPlayer2.transform.GetChild(1).gameObject.SetActive(true);
            });
        }
        else if (client.Player == Players.Player3)
        {
            MainThreadDispatcher.instance.RunOnMainThread(() =>
            {
                uiPlayer3.SetActive(true);
                uiPlayer3.transform.GetChild(1).gameObject.SetActive(true);

            });
        }

        if (playerJoinCount == 3)
        {
            MainThreadDispatcher.instance.RunOnMainThread(() =>
            {
                GameManager.instance.lobbyUI.transform.GetChild(0).gameObject.SetActive(true);
            });
        }
    }

    public void ManualPlayerDelete(int playerNo)
    {
        for (int i = 0; i < connectedClients.Count; i++)
        {
            if (connectedClients[i].Player == (Players)playerNo && playerNo == 1)
            {
                player1Leave = true;
                MainThreadDispatcher.instance.RunOnMainThread(() =>
                {
                    uiPlayer1.SetActive(false);
                    uiPlayer1.transform.GetChild(1).gameObject.SetActive(false);
                });
                connectedClients.Remove(connectedClients[i]);
                break;
            }
            else if (connectedClients[i].Player == (Players)playerNo && playerNo == 2)
            {
                player2Leave = true;
                MainThreadDispatcher.instance.RunOnMainThread(() =>
                {
                    uiPlayer2.SetActive(false);
                    uiPlayer2.transform.GetChild(1).gameObject.SetActive(false);
                });
                connectedClients.Remove(connectedClients[i]);
                break;
            }
            else if (connectedClients[i].Player == (Players)playerNo && playerNo == 3)
            {
                MainThreadDispatcher.instance.RunOnMainThread(() =>
                {
                    uiPlayer3.SetActive(false);
                    uiPlayer3.transform.GetChild(1).gameObject.SetActive(false);
                });
                connectedClients.Remove(connectedClients[i]);
                break;
            }

        }

        playerJoinCount -= 1;
        MainThreadDispatcher.instance.RunOnMainThread(() =>
        {
            GameManager.instance.lobbyUI.transform.GetChild(0).gameObject.SetActive(false);
        });

    }

    public string GenerateUniquePassword()
    {
        // Generate a unique 6-digit password
        System.Random random = new System.Random();
        string password;

        do
        {
            password = random.Next(100000, 999999).ToString();
        } while (connectedClients.Exists(client => client.Password == password));

        return password;
    }

    public string SendBoardTurnDatasTurn()
    {
        string boardGameState = GetPieceFieldDatas();
        StatusMessage statusMessage = new StatusMessage
        {
            Status = "Success",
            Message = "Now is your turn",
            Player = "",
            Password = "",
            YourTurn = true,
            Board = boardGameState,
            MovableFields = "",
            NeedPromotion = "",

            KingInCheck = null,
            KingMovableField = "",
            Blockable = null,
            BlockableField = "",
            WhoChecked = ""
        };

        return statusMessage.ToJsonString();
    }

    public string SendBoardTurnDatasBoardServer(int player)
    {
        string boardGameState = GetPieceFieldDatasBoardPlayer(player);
        StatusMessage statusMessage = new StatusMessage
        {
            Status = "Success",
            Message = "Check board for piece. ",
            Player = "",
            Password = "",
            YourTurn = null,
            Board = boardGameState,
            MovableFields = "",
            NeedPromotion = "",

            KingInCheck = null,
            KingMovableField = "",
            Blockable = null,
            BlockableField = "",
            WhoChecked = ""
        };

        return statusMessage.ToJsonString();
    }
    public string GetPieceFieldDatas()
    {
        // Implement your logic to get the game state, considering the 96 space board slots
        // This is just a placeholder, adjust it based on your actual implementation
        GameManager.instance.dataGatheringCompleted = false;


        GameManager.instance.GatherPieceFieldData();


        // Convert the list to a JSON array
        while (!GameManager.instance.dataGatheringCompleted)
        {

        }

        string boardStateJson = "[" + string.Join(",", GameManager.instance.boardState) + "]";

        return boardStateJson;
    }

    public string GetPieceFieldDatasBoardPlayer(int player)
    {
        // Implement your logic to get the game state, considering the 96 space board slots
        // This is just a placeholder, adjust it based on your actual implementation
        GameManager.instance.dataGatheringBoardPlayerCompleted = false;


        GameManager.instance.GatherPieceFieldDataPlayer(player);


        // Convert the list to a JSON array
        while (!GameManager.instance.dataGatheringBoardPlayerCompleted)
        {

        }

        string boardStateJson = "[" + string.Join(",", GameManager.instance.boardState) + "]";

        return boardStateJson;
    }

}

public class ClientData
{
    public string Password { get; set; }
    public Players Player { get; set; }
    // Add other client data as needed
    public GameService gameService { get; set; }
    public bool isLocal { get; set; }

    public ClientData()
    {
        isLocal = false;
    }
}


[System.Serializable]
public class StatusMessage
{
    public string Status;
    public string Message;
    public string Player;
    public string Password;
    public bool? YourTurn;
    public string Board;
    public string MovableFields;
    public string NeedPromotion;

    public bool? KingInCheck;
    public string KingMovableField;
    public bool? Blockable;
    public string BlockableField;
    public string WhoChecked;

    public string ToJsonString()
    {
        string playerJson = Player != "" && Player != null ? $",\"Player\": {Player}" : "";
        string passwordJson = Password != "" && Password != null ? $",\"Password\": {Password}" : "";
        string yourTurnJson = YourTurn != null ? $",\"YourTurn\": {YourTurn}" : "";
        string boardDataJson = Board != "" && Board != null ? $",\"Board\": {Board}" : "";
        string movableFieldsJson = MovableFields != "" && MovableFields != null ? $",\"MovableFields\": {MovableFields}" : "";
        string needPromotionJson = NeedPromotion != "" && NeedPromotion != null ? $",\"NeedPromotion\": \"{NeedPromotion}\"" : "";

        string isKingInCheckJson = KingInCheck != null ? $",\"KingInCheck\": {KingInCheck}" : "";
        string kingMovableFieldJson = KingMovableField != "" && KingMovableField != null ? $",\"KingMovableField\": {KingMovableField}" : "";
        string blockableJson = Blockable != null ? $",\"Blockable\": {Blockable}" : "";
        string blockableFieldJson = BlockableField != "" && BlockableField != null ? $",\"BlockableField\": {BlockableField}" : "";
        string whoCheckedJson = WhoChecked != "" && WhoChecked != null ? $",\"WhoChecked\": {WhoChecked}" : "";

        return $"{{\"Status\":\"{Status}\",\"Message\":\"{Message}\"{playerJson}{passwordJson}{yourTurnJson}{boardDataJson}{movableFieldsJson}{needPromotionJson}{isKingInCheckJson}{kingMovableFieldJson}{blockableJson}{blockableFieldJson}{whoCheckedJson}}}";
    }
}

[System.Serializable]
public class CommandMessage
{
    public string Command;
    public string Password;
    public MoveData Move;
    public string Field; // this is fields that search for movable fields 

    public string VirtualBoard;
    public string Target;

    public string Promotion;

    // public string ToJsonString()
    // {
    //     string moveJson = Move != null ? $",\"Move\":{JsonConvert.SerializeObject(Move)}" : "";

    //     return $"{{\"Command\":\"{Command}\",\"Data\":\"{Password}\"{moveJson}}}";
    // }
}

[System.Serializable]
public class MoveData
{
    public string From;
    public string To;
}
