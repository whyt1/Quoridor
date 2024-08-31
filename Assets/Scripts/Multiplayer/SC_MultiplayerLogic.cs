using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.shephertz.app42.gaming.multiplayer.client;
using MyQuoridorApp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SC_MultiplayerLogic : MonoBehaviour
{
    #region Variables
    [Header("Menu Components")]
    public Button Btn_Search;
    public Button Btn_Host;
    public Button Btn_OpenGame;
    public Button Btn_StartGame; // only host has this
    public TextMeshProUGUI TxtMultiStatus;
    public TextMeshProUGUI TxtRoomInfo;
    public RoomProperties currProps;
    string currRoomID;
    Stack<string> roomIDS;
    Coroutine currLoading;
    
    // private readonly string apiKey = "2fa36e2dc2f2e34b62e271307ec9f7dd95184098c3c7955994da1775e2fbc9c8";
    // private readonly string pvtKey = "c3a5469362eba68c784838656b369800329511c5f8b6544c51e1465dd596d1bb";

    private readonly string apiKey = "7a4d7ab6dd01a049d4153354dfe6b262e328f6c0f4b5b63cfda7a4898b993adf";
    private readonly string pvtKey = "72870153452ab88beae898d3d3331814f579f8695eb9d56d8a4cec746d0b6a69";

    WarpClient client = null;
    public WarpClient Client 
    {
        get
        {
            if (client == null)
            {
                WarpClient.initialize(apiKey, pvtKey);
                client = WarpClient.GetInstance();
            }
            return client;
        }
    }
    private string _userID;
    public string UserID 
    {
        get
        {
            // string userID = PlayerPrefs.GetString("userID", string.Empty);
            // if (string.IsNullOrEmpty(userID))
            // {
            //     userID = userID.RandomString();
            //     PlayerPrefs.SetString("userID", userID);
            // }
            // return userID +
            if (string.IsNullOrEmpty(_userID))
            {
                _userID = UnityEngine.Random.Range(0, 100).ToString();
                SC_GameData.Instance.UserID = _userID;
            }
            return _userID;
        }       
    }

    WarpListener listener = null;
    private int RematchCount = 1;

    public WarpListener Listener
    {
        get
        {
            listener ??= new(Client);
            return listener;
        }
    }
    
    #endregion

    #region Requests

    void OnConnect() 
    { 
        StartLoading("Connecting");
        Listener.Connect(UserID); 
    }
    void OnDisconnect() 
    { 
        Listener.Disconnect(); 
        // disconnect callback returns not on main thread, can't be used to update UI
        SC_GameData.Instance.PlayersData.Clear();
        SC_GameLogic.Instance.ClearGameBoard();
        StopLoading("");
        SC_MenuController.OnChangeScreen(Screens.MainMenu);
    }  

    private void OnSearch() 
    { 
        StartLoading("Searching");
        Listener.Search(SC_GameData.Instance.Password);
    }

    private void OnHost() 
    { 
        StartLoading("Open New Game");      
        Listener.CreateRoom(SC_GameData.Instance.RoomProps);
    }

    private void OnNext()
    {
        if (roomIDS.Count > 0)
        {
            currRoomID = roomIDS.Pop();
            listener.GetRoomInfo(currRoomID);
        }
        else 
        {
            SC_MenuController.OnChangeScreen(Screens.Previous);
            OnSearchCallback(false, null);
        }
    }

    private void OnJoin()
    {
        if (currRoomID == null || currProps == null)
        {
            Debug.LogError("No Room To Join");
            OnNext();
            return;
        }
        if (currProps.PlayersData.Count >= currProps.NumOfPlayers)
        {
            TxtMultiStatus.text = "Can't Join, Room is full";
            return;
        }
        listener.JoinRoom(currRoomID);
        StartLoading("Joining Game");
    }

    private void OnStartGame()
    {
        listener.StartGame();
        StartLoading("Starting Game");
    }

    private void OnGameOver()
    {
        listener?.EndGame();
    }

    private void OnBarrierMoved(string nameBarrier)
    {
        if (nameBarrier != null)
            Listener.SendChat(nameBarrier);
    }

    #endregion

    #region MonoBehaviour
    void OnEnable()
    {
        SC_MultiplayerController.OnConnect += OnConnect;
        SC_MultiplayerController.OnDisconnect += OnDisconnect;
        SC_MultiplayerController.OnSearch += OnSearch;
        SC_MultiplayerController.OnHost += OnHost;
        SC_MultiplayerController.OnNext += OnNext;
        SC_MultiplayerController.OnJoin += OnJoin;
        SC_MultiplayerController.OnStartGame += OnStartGame;
        SC_MultiplayerController.OnRematch += OnRematch;

        WarpListener.OnConnect += OnConnectCallback;
        WarpListener.OnSearch += OnSearchCallback;
        WarpListener.OnGetRoomInfo += OnGetRoomInfoCallback;
        WarpListener.OnJoinRoom += OnJoinRoomCallback;
        WarpListener.OnLeaveRoom += OnLeaveRoomCallback;
        WarpListener.OnUpdateProps += OnUpdatePropsCallback;
        WarpListener.OnGameStarted += OnGameStartedCallback;
        WarpListener.OnChatReceived += OnChatReceivedCallback;

        SC_MenuController.OnChooseColor += OnChooseColor;

        SC_GameLogic.OnGameOver += OnGameOver;

        SC_BarrierLogic.OnBarrierMoved += OnBarrierMoved;
    }

    void OnDisable()
    {
        SC_MultiplayerController.OnConnect -= OnConnect;
        SC_MultiplayerController.OnDisconnect -= OnDisconnect;
        SC_MultiplayerController.OnSearch -= OnSearch;
        SC_MultiplayerController.OnHost -= OnHost;
        SC_MultiplayerController.OnNext -= OnNext;
        SC_MultiplayerController.OnJoin -= OnJoin;
        SC_MultiplayerController.OnStartGame -= OnStartGame;
        SC_MultiplayerController.OnRematch -= OnRematch;

        WarpListener.OnConnect -= OnConnectCallback;
        WarpListener.OnSearch -= OnSearchCallback;
        WarpListener.OnGetRoomInfo -= OnGetRoomInfoCallback;
        WarpListener.OnJoinRoom -= OnJoinRoomCallback;
        WarpListener.OnLeaveRoom += OnLeaveRoomCallback;
        WarpListener.OnUpdateProps -= OnUpdatePropsCallback;
        WarpListener.OnGameStarted -= OnGameStartedCallback;
        WarpListener.OnChatReceived -= OnChatReceivedCallback;

        SC_MenuController.OnChooseColor -= OnChooseColor;

        SC_GameLogic.OnGameOver -= OnGameOver;

        SC_BarrierLogic.OnBarrierMoved -= OnBarrierMoved;
    }

    #endregion

    #region Loading
    void StartLoading(string msg)
    {
        StopLoading("");
        Btn_Search.interactable = false;
        Btn_Host.interactable = false;
        Btn_OpenGame.interactable = false;
        currLoading = StartCoroutine(Loading(msg));
    }
    void StopLoading(string msg)
    {
        Btn_Search.interactable = true;
        Btn_Host.interactable = true;
        Btn_OpenGame.interactable = true;
        if (currLoading != null)
            StopCoroutine(currLoading);
        TxtMultiStatus.text = msg;
    }
    IEnumerator Loading(string msg)
    {
        List<string> dots = new()
        {
            "", ".", "..", "..."
        };
        for (int i = 0; ; i++)
        {
            TxtMultiStatus.text = msg + dots[i%4];
            yield return new WaitForSeconds(0.5f);
        }
    }
    #endregion

    #region Sever Callbacks
    private void OnConnectCallback(bool success)
    {
        if (success)
        {
            StopLoading("Connected");
        }
        else
        {
            StopLoading("Failed To Connect!");
            SC_MenuController.OnChangeScreen(Screens.MultiPlayer);
        }
        SC_GameData.Instance.ToggleUnityObject("MultiStatus", true);
    }

    private void OnSearchCallback(bool success, Stack<string> rooms)
    {
        if (success && rooms.Count > 0)
        {
            StopLoading("Found "+rooms.Count+" Rooms!");
            SC_MenuController.OnChangeScreen(Screens.Searching);
            roomIDS = rooms;
            OnNext();
            // show rooms info, give next prev buttons, maybe new screen?
        }
        else
        {
            StopLoading("No Games Found!");
        }
    }

    private void OnGetRoomInfoCallback(bool success, List<string> users, RoomProperties props)
    {
        if (!success)
        {
            SC_MenuController.OnChangeScreen(Screens.Previous);
            StopLoading("Failed Loading Rooms");
            return;
        }
        if (users.Count == 0 || users.Count == props.NumOfPlayers)
        {
            // ignore empty or full rooms
            TxtRoomInfo.text = users.Count == 0 ? "Room is unavailable" : "Room is Full";
        }
        else
        {
            currProps = props;
            TxtRoomInfo.text = 
                        "Password:\t\t" + props.Password + "\n" +
                        "Players:\t\t" + currProps.PlayersData.Count + " of " + props.NumOfPlayers + "\n" +
                        "Barriers:\t\t" + props.NumOfBarriers + "\n" +
                        "Timer:\t\t" + props.TurnTimer + " s";
        }
        SC_MenuController.OnChangeScreen(Screens.Searching);
    }

    void AddPlayerData(string joinedUserID)
    {
        ColorOptions color = SC_GameData.Instance.RandomColor;
        SC_GameData.Instance.PlayersData.Add(
            new PlayerData(joinedUserID, color, SC_GameData.Instance.PlayersData.Count)
        );
        SC_GameData.Instance.UpdatePickedColors(color, ColorOptions.Error);
        Listener.UpdateProperty();
    }

    void InitPlayers()
    {
        SC_GameLogic.Instance.ClearGameBoard();
        SC_GameLogic.Instance.players = new PlayersManager(0, UserID);
        foreach (var data in SC_GameData.Instance.PlayersData)
            SC_GameLogic.Instance.players.AddPlayer(data.UserID, data.Color, data.Index);
        return;
    }

    private void OnChatReceivedCallback(string msg)
    {
        if (msg == "Rematch") RematchCount++; 
        Debug.Log($"{SC_GameData.Instance.NumOfPlayers != RematchCount} {SC_GameData.Instance.NumOfPlayers} {RematchCount}");
        if (SC_GameData.Instance.RoomOwner == UserID && SC_GameData.Instance.NumOfPlayers == RematchCount)
        {
            StopLoading("All Players Ready!");
            Btn_StartGame.interactable = true;
        }
    }

    private void OnRematch()
    {
        SC_GameData.Instance.ToggleUnityObject("GameOver", false);
        SC_GameData.Instance.ToggleUnityObject("MultiStatus", true);
        InitPlayers();
        if (SC_GameData.Instance.RoomOwner == UserID)
        {
            Btn_StartGame.gameObject.SetActive(true);
            Btn_StartGame.interactable = false;
            if (SC_GameData.Instance.NumOfPlayers != RematchCount)
                StartLoading($"Waiting For Players");
            else
            {
                StopLoading("All Players Ready!");
                Btn_StartGame.interactable = true;
            }
        }
        else
        {
            StartLoading($"Waiting For Players");
            Listener.SendChat("Rematch");
        }
    }

    private void OnJoinRoomCallback(bool success, string owner, string joinedUserID)
    {
        Debug.Log($"{success} Player {joinedUserID} joined {owner}'s game");
        Btn_StartGame.interactable = false;
        if (!success)
        {
            StopLoading("Join Game Failed");
            SC_MenuController.OnChangeScreen(Screens.Previous);
            return;
        }
        SC_GameData.Instance.RoomOwner = owner;
        if (joinedUserID == UserID)
        {
            InitPlayers();
        }
        if (owner == UserID)
        {
            AddPlayerData(joinedUserID);
            if (SC_GameData.Instance.NumOfPlayers != SC_GameData.Instance.PlayersData.Count)
                StartLoading($"Waiting For Players");
            else
            {
                StopLoading("All Players Ready!");
                Btn_StartGame.gameObject.SetActive(true);
                Btn_StartGame.interactable = true;
            }
        }
        else
        {
            Btn_StartGame.gameObject.SetActive(false);
            // sync game data to room props
            SC_GameData.Instance.PlayersData = currProps.PlayersData;
            SC_GameData.Instance.NumOfPlayers = currProps.NumOfPlayers;
            SC_GameData.Instance.NumOfBarriers = currProps.NumOfBarriers;
            SC_GameData.Instance.TurnTime = currProps.TurnTimer;
            
            if (SC_GameData.Instance.NumOfPlayers > SC_GameData.Instance.PlayersData.Count+1)
                StartLoading("Waiting For Players");
            else
                StartLoading("Waiting For Host");
        }
        SC_MenuController.OnChangeScreen(Screens.Game);
    }

    private void OnLeaveRoomCallback(string owner, string userId, string roomId)
    {
        if (owner != UserID) { return; } // only owner handles players leaving
        if (owner == userId) 
        {
            Client.DeleteRoom(roomId);
        }
        PlayerData userData = null;
        foreach (var data in SC_GameData.Instance.PlayersData)
        {
            if (data.UserID == userId)
                userData = data;
        }
        if (userData == null) 
        { 
            Debug.LogError("Failed to delete left user data from server");
            return; 
        }
        SC_GameData.Instance.PlayersData.Remove(userData);
        Listener.UpdateProperty();
    }
    
    private void OnUpdatePropsCallback(RoomProperties props)
    {
        // We only care about players for now
        if (props == null || !props.HasProperty("playersData")) { return; }
        SC_GameData.Instance.PlayersData = props.PlayersData;
        
        if (SC_GameLogic.Instance.players == null) 
        { 
            InitPlayers();
        }

        foreach (PlayerData playerData in props.PlayersData)
        {
            SC_PlayerLogic player = SC_GameLogic.Instance.players.GetPlayer(playerData.UserID);
            if (player != null)
            {
                if (player.Color != playerData.Color)
                {
                    player.Color = playerData.Color;
                    SC_GameData.Instance.UpdatePickedColors(playerData.Color, player.Color);
                }
            }
            else
            {
                SC_GameLogic.Instance.players.AddPlayer(playerData.UserID, playerData.Color, playerData.Index);
            }
        }
    }

    private void OnGameStartedCallback(string obj)
    {
        Btn_StartGame.gameObject.SetActive(false);
        RematchCount = 1;
        StopLoading("");
    }
    #endregion

    private void OnChooseColor(ColorOptions newColor)
    {
        PlayerData myData = SC_GameData.Instance.myPlayerData;
        Debug.Log("Changing color, my data="+SC_GameData.Instance.myPlayerData.Color);
        SC_GameData.Instance.PlayersData.Remove(myData);
        SC_GameData.Instance.PlayersData.Add(new PlayerData(UserID, newColor, myData.Index));
        Debug.Log("Changing color, my data="+SC_GameData.Instance.myPlayerData.Color);
        Listener.UpdateProperty();
    }

}