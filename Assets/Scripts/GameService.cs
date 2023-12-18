using System;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;

public class GameService : WebSocketBehavior
{

    protected override void OnOpen()
    {
        if (!GameManager.instance.gameStarted)
        {
            if (Server.instance.playerJoinCount == 3)
            {
                StatusMessage statusMessage = new StatusMessage
                {
                    Status = "Fail",
                    Message = "The Game is full.",
                    Player = "",
                    Password = "",
                    YourTurn = null,
                    Board = "",
                    MovableFields = "",
                    NeedPromotion = "",

                    KingInCheck = null,
                    KingMovableField = "",
                    Blockable = null,
                    BlockableField = "",
                    WhoChecked = ""
                };
                string json = JsonUtility.ToJson(statusMessage);

                Send(json);
                Sessions.CloseSession(ID);
            }
            else
            {
                List<String> playerState = new List<string>();

                if (Server.instance.playerJoinCount > 0)
                {
                    for (int i = 0; i < Server.instance.connectedClients.Count; i++)
                    {
                        playerState.Add($"There is Player {(int)Server.instance.connectedClients[i].Player} in the lobby.");
                    }
                }

                string message;
                if (playerState.Count != 0)
                {
                    message = string.Join(' ', playerState);
                }
                else
                {
                    message = "";
                }

                var password = Server.instance.GenerateUniquePassword();
                ClientData clientData = new ClientData();
                if (!Server.instance.player1Leave && !Server.instance.player2Leave)
                {
                    clientData = new ClientData { Password = password, Player = (Players)(Server.instance.playerJoinCount + 1), gameService = this, isLocal = false };
                    Server.instance.connectedClients.Add(clientData);
                }
                else if (Server.instance.player1Leave)
                {
                    clientData = new ClientData { Password = password, Player = Players.Player1, gameService = this, isLocal = false };
                    Server.instance.player1Leave = false;
                    Server.instance.connectedClients.Insert(0, clientData);
                }
                else if (Server.instance.player2Leave)
                {
                    clientData = new ClientData { Password = password, Player = Players.Player2, gameService = this, isLocal = false };
                    Server.instance.player2Leave = false;
                    if (Server.instance.connectedClients.Count > 1)
                    {
                        Server.instance.connectedClients.Insert(1, clientData);
                    }
                    else
                    {
                        Server.instance.connectedClients.Add(clientData);
                    }

                }


                StatusMessage statusMessage = new StatusMessage
                {
                    Status = "Success",
                    Message = $"You're successfully joined. You're Player {(int)clientData.Player}! Your password is in data. {message}",
                    Player = $"\"{clientData.Player.ToString()}\"",
                    Password = password,
                    YourTurn = null,
                    Board = "",
                    MovableFields = "",
                    NeedPromotion = "",

                    KingInCheck = null,
                    KingMovableField = "",
                    Blockable = null,
                    BlockableField = "",
                    WhoChecked = ""
                };
                string json = statusMessage.ToJsonString();

                Send(json);
                Server.instance.APIPlayerJoin(clientData.Player);
            }
        }

    }

    protected override void OnMessage(MessageEventArgs e)
    {

        // CommandMessage commandMessage = JsonUtility.FromJson<CommandMessage>(e.Data);
        CommandMessage commandMessage = JsonConvert.DeserializeObject<CommandMessage>(e.Data, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });
        StatusMessage statusMessage = new StatusMessage { };
        string json = statusMessage.ToJsonString();
        bool toggleCustomJson = false;

        if (string.IsNullOrEmpty(commandMessage.Password))
        {
            statusMessage = new StatusMessage
            {
                Status = "Error",
                Message = "You're need to send password too. ",
                Player = "",
                Password = "",
                YourTurn = null,
                Board = "",
                MovableFields = "",
                NeedPromotion = "",

                KingInCheck = null,
                KingMovableField = "",
                Blockable = null,
                BlockableField = "",
                WhoChecked = ""
            };
        }
        else
        {
            if (commandMessage.Command.ToLower() == "reconnect" && GameManager.instance.gameStarted)
            {
                bool isPlayerInTheGame = false;
                // ClientData client = null;

                for (int i = 0; i < Server.instance.connectedClients.Count; i++)
                {
                    if (Server.instance.connectedClients[i].isLocal == false)
                    {
                        if (Server.instance.connectedClients[i].Password == commandMessage.Password)
                        {
                            isPlayerInTheGame = true;
                            Server.instance.connectedClients[i].gameService = this;
                            // client = Server.instance.connectedClients[i];

                            break;
                        }
                    }
                }

                if (!isPlayerInTheGame)
                {
                    statusMessage = new StatusMessage
                    {
                        Status = "Error",
                        Message = "You're not the player from this game.",
                        Player = "",
                        Password = "",
                        YourTurn = null,
                        Board = "",
                        MovableFields = "",
                        NeedPromotion = "",

                        KingInCheck = null,
                        KingMovableField = "",
                        Blockable = null,
                        BlockableField = "",
                        WhoChecked = ""
                    };
                    string _json = JsonUtility.ToJson(statusMessage);

                    Send(_json);
                    Sessions.CloseSession(ID);
                }
                else
                {
                    statusMessage = new StatusMessage
                    {
                        Status = "Success",
                        Message = "You're now rejoined to this game.",
                        Player = "",
                        Password = "",
                        YourTurn = null,
                        Board = "",
                        MovableFields = "",
                        NeedPromotion = "",

                        KingInCheck = null,
                        KingMovableField = "",
                        Blockable = null,
                        BlockableField = "",
                        WhoChecked = ""
                    };


                }


            }
            else if (commandMessage.Command.ToLower() == "reconnect" && !GameManager.instance.gameStarted)
            {
                statusMessage = new StatusMessage
                {
                    Status = "Fail",
                    Message = "Game is not start yet! ",
                    Player = "",
                    Password = "",
                    YourTurn = null,
                    Board = "",
                    MovableFields = "",
                    NeedPromotion = "",

                    KingInCheck = null,
                    KingMovableField = "",
                    Blockable = null,
                    BlockableField = "",
                    WhoChecked = ""
                };
            }
            else if (commandMessage.Command.ToLower() == "checkturn" && GameManager.instance.gameStarted)
            {

                bool foundPassword = false;

                for (int i = 0; i < Server.instance.connectedClients.Count; i++)
                {
                    if (Server.instance.connectedClients[i].Password == commandMessage.Password.ToString())
                    {
                        if (GameManager.instance.currentPastTurn == 0)
                        {
                            if (Server.instance.connectedClients[i].Player == GameManager.instance.GetCurrentTurnPlayer())
                            {
                                Server.instance.connectedClientsForGameService = i;
                                MainThreadDispatcher.instance.RunOnMainThread(() =>
                                {
                                    Server.instance.connectedClients[Server.instance.connectedClientsForGameService].gameService.SendBoardTurnDatasTurn();
                                    Server.instance.connectedClientsForGameService = -1;
                                });

                                toggleCustomJson = true;
                                foundPassword = true;
                                break;
                            }
                        }
                        else
                        {
                            statusMessage = new StatusMessage
                            {
                                Status = "Fail",
                                Message = "It not in current turn. The game is watch back in past turn. ",
                                Player = "",
                                Password = "",
                                YourTurn = false,
                                Board = "",
                                MovableFields = "",
                                NeedPromotion = "",

                                KingInCheck = null,
                                KingMovableField = "",
                                Blockable = null,
                                BlockableField = "",
                                WhoChecked = ""
                            };
                            foundPassword = true;
                            break;
                        }

                    }

                }

                if (!foundPassword)
                {
                    statusMessage = new StatusMessage
                    {
                        Status = "Success",
                        Message = "This is not your turn! ",
                        Player = "",
                        Password = "",
                        YourTurn = false,
                        Board = "",
                        MovableFields = "",
                        NeedPromotion = "",

                        KingInCheck = null,
                        KingMovableField = "",
                        Blockable = null,
                        BlockableField = "",
                        WhoChecked = ""
                    };
                }

            }
            else if (!GameManager.instance.gameStarted)
            {
                statusMessage = new StatusMessage
                {
                    Status = "Error",
                    Message = "The game is not start yet! ",
                    Player = "",
                    Password = "",
                    YourTurn = null,
                    Board = "",
                    MovableFields = "",
                    NeedPromotion = "",

                    KingInCheck = null,
                    KingMovableField = "",
                    Blockable = null,
                    BlockableField = "",
                    WhoChecked = ""
                };
            }
            else if (commandMessage.Command.ToLower() == "move" && GameManager.instance.gameStarted)
            {
                if (commandMessage.Move == null)
                {
                    statusMessage = new StatusMessage
                    {
                        Status = "Error",
                        Message = "You're need to send which piece in field to move in which field. ",
                        Player = "",
                        Password = "",
                        YourTurn = null,
                        Board = "",
                        MovableFields = "",
                        NeedPromotion = "",

                        KingInCheck = null,
                        KingMovableField = "",
                        Blockable = null,
                        BlockableField = "",
                        WhoChecked = ""
                    };
                }
                else
                {
                    bool foundPassword = false;

                    for (int i = 0; i < Server.instance.connectedClients.Count; i++)
                    {
                        if (Server.instance.connectedClients[i].Password == commandMessage.Password.ToString())
                        {
                            if (Server.instance.connectedClients[i].Player == GameManager.instance.GetCurrentTurnPlayer())
                            {
                                if (GameManager.instance.currentPastTurn == 0)
                                {
                                    string fromPosition = commandMessage.Move.From;
                                    string toPosition = commandMessage.Move.To;

                                    if (fromPosition.Length == 3 && toPosition.Length == 3)
                                    {
                                        GameManager.instance.gsCurrentClient = i;
                                        GameManager.instance.gsFrom = fromPosition;
                                        GameManager.instance.gsTo = toPosition;

                                        MainThreadDispatcher.instance.RunOnMainThread(() =>
                                        {
                                            GameManager.instance.GameServiceMoveCommand(GameManager.instance.gsCurrentClient);
                                        });

                                        foundPassword = true;
                                        toggleCustomJson = true;
                                        break;
                                    }
                                    else
                                    {
                                        statusMessage = new StatusMessage
                                        {
                                            Status = "Error",
                                            Message = "Invalid field name. ",
                                            Player = "",
                                            Password = "",
                                            YourTurn = null,
                                            Board = "",
                                            MovableFields = "",
                                            NeedPromotion = "",

                                            KingInCheck = null,
                                            KingMovableField = "",
                                            Blockable = null,
                                            BlockableField = "",
                                            WhoChecked = ""
                                        };
                                        foundPassword = true;
                                    }
                                }
                                else
                                {
                                    statusMessage = new StatusMessage
                                    {
                                        Status = "Fail",
                                        Message = "It not in current turn. The game is watch back in past turn. ",
                                        Player = "",
                                        Password = "",
                                        YourTurn = false,
                                        Board = "",
                                        MovableFields = "",
                                        NeedPromotion = "",

                                        KingInCheck = null,
                                        KingMovableField = "",
                                        Blockable = null,
                                        BlockableField = "",
                                        WhoChecked = ""
                                    };
                                    foundPassword = true;
                                }

                            }
                        }

                    }

                    if (!foundPassword)
                    {
                        statusMessage = new StatusMessage
                        {
                            Status = "Fail",
                            Message = "This is not your turn! ",
                            Player = "",
                            Password = "",
                            YourTurn = null,
                            Board = "",
                            MovableFields = "",
                            NeedPromotion = "",

                            KingInCheck = null,
                            KingMovableField = "",
                            Blockable = null,
                            BlockableField = "",
                            WhoChecked = ""
                        };
                    }
                }
            }
            else if (commandMessage.Command.ToLower() == "movable" && GameManager.instance.gameStarted)
            {
                if (string.IsNullOrEmpty(commandMessage.Field.ToLower()))
                {
                    statusMessage = new StatusMessage
                    {
                        Status = "Error",
                        Message = "You're need to send field too. ",
                        Player = "",
                        Password = "",
                        YourTurn = null,
                        Board = "",
                        MovableFields = "",
                        NeedPromotion = "",

                        KingInCheck = null,
                        KingMovableField = "",
                        Blockable = null,
                        BlockableField = "",
                        WhoChecked = ""
                    };
                }
                else
                {
                    bool foundPassword = false;

                    for (int i = 0; i < Server.instance.connectedClients.Count; i++)
                    {
                        if (Server.instance.connectedClients[i].Password == commandMessage.Password.ToString())
                        {
                            if (GameManager.instance.currentPastTurn == 0)
                            {

                                string field = commandMessage.Field;
                                GameManager.instance.gsMovableField = field;
                                GameManager.instance.gsCurrentClient = i;
                                MainThreadDispatcher.instance.RunOnMainThread(() =>
                                {
                                    GameManager.instance.GameServiceMovableFieldCommand(GameManager.instance.gsCurrentClient);
                                    GameManager.instance.gsMovableField = "";
                                    GameManager.instance.gsCurrentClient = -1;
                                });

                                foundPassword = true;
                                toggleCustomJson = true;
                                break;
                            }
                            else
                            {
                                statusMessage = new StatusMessage
                                {
                                    Status = "Fail",
                                    Message = "It not in current turn. The game is watch back in past turn. ",
                                    Player = "",
                                    Password = "",
                                    YourTurn = false,
                                    Board = "",
                                    MovableFields = "",
                                    NeedPromotion = "",

                                    KingInCheck = null,
                                    KingMovableField = "",
                                    Blockable = null,
                                    BlockableField = "",
                                    WhoChecked = ""
                                };
                                foundPassword = true;
                                break;
                            }
                        }

                    }

                    if (!foundPassword)
                    {
                        statusMessage = new StatusMessage
                        {
                            Status = "Error",
                            Message = "You're not the player in this game. ",
                            Player = "",
                            Password = "",
                            YourTurn = null,
                            Board = "",
                            MovableFields = "",
                            NeedPromotion = "",

                            KingInCheck = null,
                            KingMovableField = "",
                            Blockable = null,
                            BlockableField = "",
                            WhoChecked = ""
                        };
                    }
                }
            }
            else if (commandMessage.Command.ToLower() == "passturn" && GameManager.instance.gameStarted)
            {
                bool foundPassword = false;
                bool currentPlayerTurn = false;
                for (int i = 0; i < Server.instance.connectedClients.Count; i++)
                {
                    if (Server.instance.connectedClients[i].Password == commandMessage.Password.ToString())
                    {
                        if (Server.instance.connectedClients[i].Player == GameManager.instance.GetCurrentTurnPlayer())
                        {
                            if (GameManager.instance.currentPastTurn == 0)
                            {
                                GameManager.instance.gsCurrentClient = i;
                                MainThreadDispatcher.instance.RunOnMainThread(() =>
                                {
                                    GameManager.instance.PassTurn();
                                    GameManager.instance.gsCurrentClient = -1;
                                });

                                statusMessage = new StatusMessage
                                {
                                    Status = "Success",
                                    Message = "You're passing your turn. ",
                                    Player = "",
                                    Password = "",
                                    YourTurn = null,
                                    Board = "",
                                    MovableFields = "",
                                    NeedPromotion = "",

                                    KingInCheck = null,
                                    KingMovableField = "",
                                    Blockable = null,
                                    BlockableField = "",
                                    WhoChecked = ""
                                };


                                foundPassword = true;
                                currentPlayerTurn = true;
                                break;
                            }
                            else
                            {
                                statusMessage = new StatusMessage
                                {
                                    Status = "Fail",
                                    Message = "It not in current turn. The game is watch back in past turn. ",
                                    Player = "",
                                    Password = "",
                                    YourTurn = false,
                                    Board = "",
                                    MovableFields = "",
                                    NeedPromotion = "",

                                    KingInCheck = null,
                                    KingMovableField = "",
                                    Blockable = null,
                                    BlockableField = "",
                                    WhoChecked = ""
                                };
                                foundPassword = true;
                                break;
                            }

                        }
                    }

                }

                if (!foundPassword)
                {
                    statusMessage = new StatusMessage
                    {
                        Status = "Error",
                        Message = "You're not the player in this game. ",
                        Player = "",
                        Password = "",
                        YourTurn = null,
                        Board = "",
                        MovableFields = "",
                        NeedPromotion = "",

                        KingInCheck = null,
                        KingMovableField = "",
                        Blockable = null,
                        BlockableField = "",
                        WhoChecked = ""
                    };
                }
                else if (!currentPlayerTurn)
                {
                    statusMessage = new StatusMessage
                    {
                        Status = "Fail",
                        Message = "Currently turn is not your turn. ",
                        Player = "",
                        Password = "",
                        YourTurn = null,
                        Board = "",
                        MovableFields = "",
                        NeedPromotion = "",

                        KingInCheck = null,
                        KingMovableField = "",
                        Blockable = null,
                        BlockableField = "",
                        WhoChecked = ""
                    };
                }
            }
            else if (commandMessage.Command.ToLower() == "checkking" && GameManager.instance.gameStarted)
            {
                bool foundPassword = false;
                bool currentPlayerTurn = false;

                for (int i = 0; i < Server.instance.connectedClients.Count; i++)
                {
                    if (Server.instance.connectedClients[i].Password == commandMessage.Password.ToString())
                    {
                        if (Server.instance.connectedClients[i].Player == GameManager.instance.GetCurrentTurnPlayer())
                        {
                            if (GameManager.instance.currentPastTurn == 0)
                            {
                                GameManager.instance.gsCurrentClient = i;
                                MainThreadDispatcher.instance.RunOnMainThread(() =>
                                {
                                    GameManager.instance.CheckingForKingIfChecked(GameManager.instance.gsCurrentClient);
                                    GameManager.instance.gsCurrentClient = -1;
                                });

                                foundPassword = true;
                                toggleCustomJson = true;
                                currentPlayerTurn = true;
                                break;
                            }
                            else
                            {
                                statusMessage = new StatusMessage
                                {
                                    Status = "Fail",
                                    Message = "It not in current turn. The game is watch back in past turn. ",
                                    Player = "",
                                    Password = "",
                                    YourTurn = false,
                                    Board = "",
                                    MovableFields = "",
                                    NeedPromotion = "",

                                    KingInCheck = null,
                                    KingMovableField = "",
                                    Blockable = null,
                                    BlockableField = "",
                                    WhoChecked = ""
                                };
                                foundPassword = true;
                                currentPlayerTurn = true;
                                break;
                            }

                        }
                    }

                }

                if (!foundPassword)
                {
                    statusMessage = new StatusMessage
                    {
                        Status = "Error",
                        Message = "You're not the player in this game. ",
                        Player = "",
                        Password = "",
                        YourTurn = null,
                        Board = "",
                        MovableFields = "",
                        NeedPromotion = "",

                        KingInCheck = null,
                        KingMovableField = "",
                        Blockable = null,
                        BlockableField = "",
                        WhoChecked = ""
                    };
                }
                else if (!currentPlayerTurn)
                {
                    statusMessage = new StatusMessage
                    {
                        Status = "Fail",
                        Message = "Currently turn is not your turn. ",
                        Player = "",
                        Password = "",
                        YourTurn = null,
                        Board = "",
                        MovableFields = "",
                        NeedPromotion = "",

                        KingInCheck = null,
                        KingMovableField = "",
                        Blockable = null,
                        BlockableField = "",
                        WhoChecked = ""
                    };
                }
            }
            else if (commandMessage.Command.ToLower() == "mypiece" && GameManager.instance.gameStarted)
            {
                bool foundPassword = false;
                bool currentPlayerTurn = false;

                for (int i = 0; i < Server.instance.connectedClients.Count; i++)
                {
                    if (Server.instance.connectedClients[i].Password == commandMessage.Password.ToString())
                    {
                        if (Server.instance.connectedClients[i].Player == GameManager.instance.GetCurrentTurnPlayer())
                        {
                            if (GameManager.instance.currentPastTurn == 0)
                            {
                                Server.instance.connectedClientsForGameServiceMyPiece = i;
                                Server.instance.playerForGameServiceMyPiece = (int)Server.instance.connectedClients[i].Player;
                                MainThreadDispatcher.instance.RunOnMainThread(() =>
                                {
                                    Server.instance.connectedClients[Server.instance.connectedClientsForGameServiceMyPiece].gameService.SendBoardTurnDatasBoardGS(Server.instance.playerForGameServiceMyPiece);

                                    Server.instance.connectedClientsForGameServiceMyPiece = -1;
                                    Server.instance.playerForGameServiceMyPiece = -1;
                                });

                                // Server.instance.connectedClientsForGameService = i;
                                // MainThreadDispatcher.instance.RunOnMainThread(() =>
                                // {
                                //     Server.instance.connectedClients[Server.instance.connectedClientsForGameService].gameService.SendBoardTurnDatasTurn();
                                //     Server.instance.connectedClientsForGameService = -1;
                                // });

                                foundPassword = true;
                                toggleCustomJson = true;
                                currentPlayerTurn = true;
                                break;
                            }
                            else
                            {
                                statusMessage = new StatusMessage
                                {
                                    Status = "Fail",
                                    Message = "It not in current turn. The game is watch back in past turn. ",
                                    Player = "",
                                    Password = "",
                                    YourTurn = false,
                                    Board = "",
                                    MovableFields = "",
                                    NeedPromotion = "",

                                    KingInCheck = null,
                                    KingMovableField = "",
                                    Blockable = null,
                                    BlockableField = "",
                                    WhoChecked = ""
                                };

                                foundPassword = true;
                                currentPlayerTurn = true;
                                break;
                            }

                        }
                    }

                }

                if (!foundPassword)
                {
                    statusMessage = new StatusMessage
                    {
                        Status = "Error",
                        Message = "You're not the player in this game. ",
                        Player = "",
                        Password = "",
                        YourTurn = null,
                        Board = "",
                        MovableFields = "",
                        NeedPromotion = "",

                        KingInCheck = null,
                        KingMovableField = "",
                        Blockable = null,
                        BlockableField = "",
                        WhoChecked = ""
                    };
                }
                else if (!currentPlayerTurn)
                {
                    statusMessage = new StatusMessage
                    {
                        Status = "Fail",
                        Message = "Currently turn is not your turn. ",
                        Player = "",
                        Password = "",
                        YourTurn = null,
                        Board = "",
                        MovableFields = "",
                        NeedPromotion = "",

                        KingInCheck = null,
                        KingMovableField = "",
                        Blockable = null,
                        BlockableField = "",
                        WhoChecked = ""
                    };
                }
            }
            else if (commandMessage.Command.ToLower() == "promote" && GameManager.instance.gameStarted)
            {
                bool foundPassword = false;
                bool currentPlayerTurn = false;
                bool validPromoteInt = false;

                if (string.IsNullOrEmpty(commandMessage.Promotion))
                {
                    statusMessage = new StatusMessage
                    {
                        Status = "Error",
                        Message = "You're need to send promotion too. ",
                        Player = "",
                        Password = "",
                        YourTurn = null,
                        Board = "",
                        MovableFields = "",
                        NeedPromotion = "",

                        KingInCheck = null,
                        KingMovableField = "",
                        Blockable = null,
                        BlockableField = "",
                        WhoChecked = ""
                    };

                }
                else
                {
                    for (int i = 0; i < Server.instance.connectedClients.Count; i++)
                    {
                        if (Server.instance.connectedClients[i].Password == commandMessage.Password.ToString())
                        {
                            foundPassword = true;
                            if (Server.instance.connectedClients[i].Player == GameManager.instance.GetCurrentTurnPlayer())
                            {
                                currentPlayerTurn = true;
                                if (GameManager.instance.togglePromotePawn)
                                {
                                    switch (commandMessage.Promotion.ToLower())
                                    {
                                        case "queen":
                                            GameManager.instance.gsPromotionInt = 0;
                                            break;
                                        case "knight":
                                            GameManager.instance.gsPromotionInt = 1;
                                            break;
                                        case "rook":
                                            GameManager.instance.gsPromotionInt = 2;
                                            break;
                                        case "bishop":
                                            GameManager.instance.gsPromotionInt = 3;
                                            break;
                                        default:
                                            GameManager.instance.gsPromotionInt = -1;
                                            break;
                                    }

                                    if (GameManager.instance.gsPromotionInt != -1)
                                    {
                                        MainThreadDispatcher.instance.RunOnMainThread(() =>
                                        {
                                            GameManager.instance.PromotePawnAction(GameManager.instance.gsPromotionInt);
                                            GameManager.instance.gsPromotionInt = -1;
                                        });
                                        validPromoteInt = true;
                                        statusMessage = new StatusMessage
                                        {
                                            Status = "Success",
                                            Message = "You're successfully promote pawn into new piece. ",
                                            Player = "",
                                            Password = "",
                                            YourTurn = null,
                                            Board = "",
                                            MovableFields = "",
                                            NeedPromotion = "",

                                            KingInCheck = null,
                                            KingMovableField = "",
                                            Blockable = null,
                                            BlockableField = "",
                                            WhoChecked = ""
                                        };
                                    }
                                    else
                                    {
                                        validPromoteInt = false;
                                    }
                                }
                                else
                                {
                                    statusMessage = new StatusMessage
                                    {
                                        Status = "Fail",
                                        Message = "There're no promotion right now. ",
                                        Player = "",
                                        Password = "",
                                        YourTurn = null,
                                        Board = "",
                                        MovableFields = "",
                                        NeedPromotion = "",

                                        KingInCheck = null,
                                        KingMovableField = "",
                                        Blockable = null,
                                        BlockableField = "",
                                        WhoChecked = ""
                                    };
                                    validPromoteInt = true;
                                }
                            }
                        }
                    }

                    if (!foundPassword)
                    {
                        statusMessage = new StatusMessage
                        {
                            Status = "Error",
                            Message = "You're not the player in this game. ",
                            Player = "",
                            Password = "",
                            YourTurn = null,
                            Board = "",
                            MovableFields = "",
                            NeedPromotion = "",

                            KingInCheck = null,
                            KingMovableField = "",
                            Blockable = null,
                            BlockableField = "",
                            WhoChecked = ""
                        };
                    }
                    else if (!currentPlayerTurn)
                    {
                        statusMessage = new StatusMessage
                        {
                            Status = "Fail",
                            Message = "Currently turn is not your turn. ",
                            Player = "",
                            Password = "",
                            YourTurn = null,
                            Board = "",
                            MovableFields = "",
                            NeedPromotion = "",

                            KingInCheck = null,
                            KingMovableField = "",
                            Blockable = null,
                            BlockableField = "",
                            WhoChecked = ""
                        };
                    }
                    else if (!validPromoteInt)
                    {
                        statusMessage = new StatusMessage
                        {
                            Status = "Fail",
                            Message = "Invalid promotion piece. ",
                            Player = "",
                            Password = "",
                            YourTurn = null,
                            Board = "",
                            MovableFields = "",
                            NeedPromotion = "",

                            KingInCheck = null,
                            KingMovableField = "",
                            Blockable = null,
                            BlockableField = "",
                            WhoChecked = ""
                        };
                    }
                }
            }
            else if (commandMessage.Command.ToLower() == "virtualboard" && GameManager.instance.gameStarted)
            {
                if (string.IsNullOrEmpty(commandMessage.VirtualBoard.ToLower()))
                {
                    statusMessage = new StatusMessage
                    {
                        Status = "Error",
                        Message = "You're need to send virtual board too. ",
                        Player = "",
                        Password = "",
                        YourTurn = null,
                        Board = "",
                        MovableFields = "",
                        NeedPromotion = "",

                        KingInCheck = null,
                        KingMovableField = "",
                        Blockable = null,
                        BlockableField = "",
                        WhoChecked = ""
                    };
                }
                else
                {
                    bool foundPassword = false;

                    for (int i = 0; i < Server.instance.connectedClients.Count; i++)
                    {
                        if (Server.instance.connectedClients[i].Password == commandMessage.Password.ToString())
                        {
                            if (GameManager.instance.currentPastTurn == 0)
                            {
                                // GameManager.instance.virtualBoard = commandMessage.VirtualBoard;

                                MainThreadDispatcher.instance.RunOnMainThread(() =>
                                {

                                });

                                foundPassword = true;
                                toggleCustomJson = true;
                                break;
                            }
                            else
                            {
                                statusMessage = new StatusMessage
                                {
                                    Status = "Fail",
                                    Message = "It not in current turn. The game is watch back in past turn. ",
                                    Player = "",
                                    Password = "",
                                    YourTurn = false,
                                    Board = "",
                                    MovableFields = "",
                                    NeedPromotion = "",

                                    KingInCheck = null,
                                    KingMovableField = "",
                                    Blockable = null,
                                    BlockableField = "",
                                    WhoChecked = ""
                                };
                                foundPassword = true;
                                break;
                            }
                        }

                    }

                    if (!foundPassword)
                    {
                        statusMessage = new StatusMessage
                        {
                            Status = "Error",
                            Message = "You're not the player in this game. ",
                            Player = "",
                            Password = "",
                            YourTurn = null,
                            Board = "",
                            MovableFields = "",
                            NeedPromotion = "",

                            KingInCheck = null,
                            KingMovableField = "",
                            Blockable = null,
                            BlockableField = "",
                            WhoChecked = ""
                        };
                    }
                }
            }
            else
            {
                statusMessage = new StatusMessage
                {
                    Status = "Error",
                    Message = "Invalid command. ",
                    Player = "",
                    Password = "",
                    YourTurn = null,
                    Board = "",
                    MovableFields = "",
                    NeedPromotion = "",

                    KingInCheck = null,
                    KingMovableField = "",
                    Blockable = null,
                    BlockableField = "",
                    WhoChecked = ""
                };
            }
        }


        if (!toggleCustomJson)
        {
            json = statusMessage.ToJsonString();
            Send(json);
            toggleCustomJson = false;
        }


    }

    protected override void OnError(ErrorEventArgs e)
    {
        // base.OnError(e);

        // UnityEngine.Debug.Log(e);
    }

    protected override void OnClose(CloseEventArgs e)
    {
        if (!GameManager.instance.gameStarted)
        {
            for (int i = 0; i < Server.instance.connectedClients.Count; i++)
            {
                if (Server.instance.connectedClients[i].gameService == this)
                {
                    if (Server.instance.connectedClients[i].Player == Players.Player1)
                    {
                        Server.instance.player1Leave = true;
                        MainThreadDispatcher.instance.RunOnMainThread(() =>
                        {
                            if (GameManager.instance.LanMode)
                            {
                                Server.instance.uiPlayer1.SetActive(false);
                            }

                        });
                    }
                    else if (Server.instance.connectedClients[i].Player == Players.Player2)
                    {
                        Server.instance.player2Leave = true;
                        MainThreadDispatcher.instance.RunOnMainThread(() =>
                        {
                            if (GameManager.instance.LanMode)
                            {
                                Server.instance.uiPlayer2.SetActive(false);
                            }

                        });
                    }
                    else if (Server.instance.connectedClients[i].Player == Players.Player3)
                    {
                        MainThreadDispatcher.instance.RunOnMainThread(() =>
                        {
                            if (GameManager.instance.LanMode)
                            {
                                Server.instance.uiPlayer3.SetActive(false);
                            }

                        });
                    }

                    if (GameManager.instance.LanMode)
                    {
                        Server.instance.playerJoinCount -= 1;
                    }

                    Server.instance.connectedClients.Remove(Server.instance.connectedClients[i]);
                    MainThreadDispatcher.instance.RunOnMainThread(() =>
                    {
                        GameManager.instance.lobbyUI.transform.GetChild(0).gameObject.SetActive(false);
                    });
                    break;
                }
            }
        }

        Sessions.CloseSession(ID);
    }

    public void SendBoardTurnDatasTurn()
    {
        Send(Server.instance.SendBoardTurnDatasTurn());
    }

    public void SendBoardTurnDatasBoardGS(int player)
    {
        Send(Server.instance.SendBoardTurnDatasBoardServer(player));
    }

    public void SendNeedPromotion(string position)
    {
        StatusMessage statusMessage = new StatusMessage
        {
            Status = "RequirePromotion",
            Message = "Your piece have reach the end of board and need promotion! ",
            Player = "",
            Password = "",
            YourTurn = null,
            Board = "",
            MovableFields = "",
            NeedPromotion = $"{position}",

            KingInCheck = null,
            KingMovableField = "",
            Blockable = null,
            BlockableField = "",
            WhoChecked = ""
        };

        string json = statusMessage.ToJsonString();

        Send(json);
    }

    public void SendMoveCommandSuccess()
    {
        StatusMessage statusMessage = new StatusMessage
        {
            Status = "Success",
            Message = "Your piece have moved! ",
            Player = "",
            Password = "",
            YourTurn = null,
            Board = "",
            MovableFields = "",
            NeedPromotion = "",

            KingInCheck = null,
            KingMovableField = "",
            Blockable = null,
            BlockableField = "",
            WhoChecked = ""
        };
        string json = statusMessage.ToJsonString();

        Send(json);
    }

    public void SendMoveCommandFailNonMovable()
    {
        StatusMessage statusMessage = new StatusMessage
        {
            Status = "Fail",
            Message = "That field is not movable for your piece. ",
            Player = "",
            Password = "",
            YourTurn = null,
            Board = "",
            MovableFields = "",
            NeedPromotion = "",

            KingInCheck = null,
            KingMovableField = "",
            Blockable = null,
            BlockableField = "",
            WhoChecked = ""
        };
        string json = statusMessage.ToJsonString();

        Send(json);
    }


    public void SendMoveCommandFailNonOwner()
    {
        StatusMessage statusMessage = new StatusMessage
        {
            Status = "Fail",
            Message = "That piece is not your piece. ",
            Player = "",
            Password = "",
            YourTurn = null,
            Board = "",
            MovableFields = "",
            NeedPromotion = "",

            KingInCheck = null,
            KingMovableField = "",
            Blockable = null,
            BlockableField = "",
            WhoChecked = ""
        };
        string json = statusMessage.ToJsonString();

        Send(json);
    }


    public void SendMoveCommandErrorInvalidFieldName()
    {
        StatusMessage statusMessage = new StatusMessage
        {
            Status = "Error",
            Message = "Invalid field name. ",
            Player = "",
            Password = "",
            YourTurn = null,
            Board = "",
            MovableFields = "",
            NeedPromotion = "",

            KingInCheck = null,
            KingMovableField = "",
            Blockable = null,
            BlockableField = "",
            WhoChecked = ""
        };
        string json = statusMessage.ToJsonString();

        Send(json);
    }

    public void SendMoveCommandErrorNoPieceInField()
    {
        StatusMessage statusMessage = new StatusMessage
        {
            Status = "Error",
            Message = "There is no piece in that field. ",
            Player = "",
            Password = "",
            YourTurn = null,
            Board = "",
            MovableFields = "",
            NeedPromotion = "",

            KingInCheck = null,
            KingMovableField = "",
            Blockable = null,
            BlockableField = "",
            WhoChecked = ""
        };
        string json = statusMessage.ToJsonString();

        Send(json);
    }

    public void SendMovableFieldCommandSuccess()
    {
        StatusMessage statusMessage = new StatusMessage
        {
            Status = "Success",
            Message = "Look at MovableFields that are list of fields of your piece can move. ",
            Player = "",
            Password = "",
            YourTurn = null,
            Board = "",
            MovableFields = GameManager.instance.movableFieldsListJson,
            NeedPromotion = "",

            KingInCheck = null,
            KingMovableField = "",
            Blockable = null,
            BlockableField = "",
            WhoChecked = ""
        };
        string json = statusMessage.ToJsonString();

        Send(json);
    }

    public void SendMoveCommandFailOnTurnWhilePromotion()
    {
        StatusMessage statusMessage = new StatusMessage
        {
            Status = "Fail",
            Message = "You're currently on promoting a pawn! ",
            Player = "",
            Password = "",
            YourTurn = null,
            Board = "",
            MovableFields = "",
            NeedPromotion = "",

            KingInCheck = null,
            KingMovableField = "",
            Blockable = null,
            BlockableField = "",
            WhoChecked = ""
        };
        string json = statusMessage.ToJsonString();

        Send(json);
    }

    public void SendMoveCommandFailOffTurnWhilePromotion()
    {
        StatusMessage statusMessage = new StatusMessage
        {
            Status = "Fail",
            Message = "Other player currently on promoting a pawn! ",
            Player = "",
            Password = "",
            YourTurn = null,
            Board = "",
            MovableFields = "",
            NeedPromotion = "",

            KingInCheck = null,
            KingMovableField = "",
            Blockable = null,
            BlockableField = "",
            WhoChecked = ""
        };
        string json = statusMessage.ToJsonString();

        Send(json);
    }

    public void SendMovableFieldCommandSuccessButNoMovable()
    {
        StatusMessage statusMessage = new StatusMessage
        {
            Status = "Success",
            Message = "There're no movable field for your piece. ",
            Player = "",
            Password = "",
            YourTurn = null,
            Board = "",
            MovableFields = "",
            NeedPromotion = "",

            KingInCheck = null,
            KingMovableField = "",
            Blockable = null,
            BlockableField = "",
            WhoChecked = ""
        };
        string json = statusMessage.ToJsonString();

        Send(json);
    }

    public void SendMovableFieldCommandFieldIsNull()
    {
        StatusMessage statusMessage = new StatusMessage
        {
            Status = "Error",
            Message = "That field is null. ",
            Player = "",
            Password = "",
            YourTurn = null,
            Board = "",
            MovableFields = "",
            NeedPromotion = "",

            KingInCheck = null,
            KingMovableField = "",
            Blockable = null,
            BlockableField = "",
            WhoChecked = ""
        };
        string json = statusMessage.ToJsonString();

        Send(json);
    }

    // public void SendMovableFieldCommandFieldIsNotOwn()
    // {
    //     StatusMessage statusMessage = new StatusMessage
    //     {
    //         Status = "Fail",
    //         Message = "That piece in the field is yours. ",
    //         Player = "",
    //         Password = "",
    //         YourTurn = null,
    //         Board = "",
    //         MovableFields = "",
    //         NeedPromotion = "",

    //         KingInCheck = null,
    //         KingMovableField = "",
    //         Blockable = null,
    //         BlockableField = "",
    //         WhoChecked = ""
    //     };
    //     string json = statusMessage.ToJsonString();

    //     Send(json);
    // }

    public void BroadcastGameStart()
    {
        StatusMessage statusMessage = new StatusMessage
        {
            Status = "Started",
            Message = "The game have been started!",
            Player = "",
            Password = "",
            YourTurn = null,
            Board = "",
            MovableFields = "",
            NeedPromotion = "",

            KingInCheck = null,
            KingMovableField = "",
            Blockable = null,
            BlockableField = "",
            WhoChecked = ""
        };
        string json = statusMessage.ToJsonString();

        Sessions.Broadcast(json);
    }

    public void KingInCheck(StatusMessage statusMessage)
    {
        Send(statusMessage.ToJsonString());
    }

    public void SendWinnerMessage()
    {
        StatusMessage statusMessage = new StatusMessage
        {
            Status = "GameEnd",
            Message = "The game is ended, You're the winner. ",
            Player = "",
            Password = "",
            YourTurn = null,
            Board = "",
            MovableFields = "",
            NeedPromotion = "",

            KingInCheck = null,
            KingMovableField = "",
            Blockable = null,
            BlockableField = "",
            WhoChecked = ""
        };
        string json = statusMessage.ToJsonString();

        Send(json);
    }

    public void SendLoserMessage()
    {
        StatusMessage statusMessage = new StatusMessage
        {
            Status = "GameEnd",
            Message = "The game is ended, You're defeated. ",
            Player = "",
            Password = "",
            YourTurn = null,
            Board = "",
            MovableFields = "",
            NeedPromotion = "",

            KingInCheck = null,
            KingMovableField = "",
            Blockable = null,
            BlockableField = "",
            WhoChecked = ""
        };
        string json = statusMessage.ToJsonString();

        Send(json);
    }

    public void SendNotWinNotLoseMessage()
    {
        StatusMessage statusMessage = new StatusMessage
        {
            Status = "GameEnd",
            Message = "The game is ended, You aren't win either lose. ",
            Player = "",
            Password = "",
            YourTurn = null,
            Board = "",
            MovableFields = "",
            NeedPromotion = "",

            KingInCheck = null,
            KingMovableField = "",
            Blockable = null,
            BlockableField = "",
            WhoChecked = ""
        };
        string json = statusMessage.ToJsonString();

        Send(json);
    }
}
