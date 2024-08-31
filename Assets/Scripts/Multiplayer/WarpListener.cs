using System;
using System.Collections.Generic;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.listener;
using com.shephertz.app42.gaming.multiplayer.client.events;
using static com.shephertz.app42.gaming.multiplayer.client.WarpConnectionState;
using static com.shephertz.app42.gaming.multiplayer.client.command.WarpResponseResultCode;
using UnityEngine;
using MyQuoridorApp;

public class WarpListener : ConnectionRequestListener, ZoneRequestListener, RoomRequestListener, NotifyListener
{
    #region Callback API
    
    public static Action<bool> OnConnect;
    public static Action<bool> OnDisconnect;
    public static Action<bool, Stack<string>> OnSearch;
    public static Action<bool, List<string>, RoomProperties> OnGetRoomInfo;
    public static Action<bool, string, string> OnJoinRoom;
    public static Action<string, string, string> OnLeaveRoom;
    public static Action<RoomProperties> OnUpdateProps; 
    public static Action<string, string> OnMoveCompleted;   
    public static Action<string> OnGameStarted;   
    public static Action<string> OnChatReceived;   
    #endregion

    #region Requests API

    public void Connect(string _userID)
    {
        if (ConnectionState != DISCONNECTED)
        {
            Debug.LogError("Connect Request not valid. Already Connected.");
            OnConnect?.Invoke(false);
            return;
        }
        userID = _userID;
        client.Connect(userID);
    }

    public void Disconnect()
    {
        if (ConnectionState == DISCONNECTED)
        {
            Debug.LogError("Disconnect Request not valid. Already Disconnected.");
            OnDisconnect?.Invoke(false);
            return;
        }
        client.Disconnect();
    }

    public void Search(int _password)
    {
        if (ConnectionState != CONNECTED)
        {
            Debug.LogError("Search Request not valid. Client Disconnected.");
            OnSearch?.Invoke(false, null);
            return;
        }
        password = _password;
        client.GetRoomWithProperties(new RoomProperties(password));
    }

    public void GetRoomInfo(string _roomInfoID)
    {
        if (ConnectionState != CONNECTED)
        {
            Debug.LogError("GetRoomInfo Request not valid. Client Disconnected.");
            OnGetRoomInfo(false, null, null);
            return;
        }
        if (string.IsNullOrEmpty(_roomInfoID))
        {
            Debug.LogError("GetRoomInfo Request not valid. roomID IsNullOrEmpty.");
            OnGetRoomInfo(false, null, null);
            return;
        }
        
        roomInfoID = _roomInfoID;
        client.GetLiveRoomInfo(roomInfoID);
    }

    public void CreateRoom(RoomProperties _roomProps)
    {
        if (ConnectionState != CONNECTED)
        {
            Debug.LogError("CreateRoom Request not valid. Client Disconnected.");
            OnJoinRoom(false, null, null);
            return;
        }
        if (!_roomProps.HasProperty("turnTimer"))
        {
            Debug.LogError("CreateRoom Request not valid. roomProps does not contain turn timer.");
            OnJoinRoom(false, null, null);
            return;
        }
        
        roomProps = _roomProps;
        string rand = "".RandomString();
        client.CreateTurnRoom(rand, userID, GlobalConstants.MAX_PLAYERS, roomProps, roomProps.TurnTimer+2);
    }

    public void JoinRoom(string _roomID)
    {
        if (ConnectionState != CONNECTED)
        {
            Debug.LogError("JoinRoom Request not valid. Client Disconnected.");
            OnJoinRoom(false, null, null);
            return;
        }
        if (string.IsNullOrEmpty(_roomID))
        {
            Debug.LogError("JoinRoom Request not valid. roomID IsNullOrEmpty.");
            OnJoinRoom(false, null, null);
            return;
        }
        
        roomID = _roomID;
        isJoined = isSubscribed = false;
        client.JoinRoom(roomID);
        client.SubscribeRoom(roomID);
    }

    public void UpdateProperty()
    {
        if (ConnectionState != CONNECTED)
        {
            Debug.LogError("UpdateProperty Request not valid. Client Disconnected.");
            OnUpdateProps(null);
            return;
        }
        client.UpdateRoomProperties(roomID, SC_GameData.Instance.RoomProps, new());
    }

    public void StartGame()
    {
        if (ConnectionState != CONNECTED)
        {
            Debug.LogError("StartGame Request not valid. Client Disconnected.");
            OnGameStarted(null);
            return;
        }
        client.startGame(true, userID);
    }

    public void EndGame()
    {
        if (ConnectionState != CONNECTED)
        {
            Debug.LogError("stopGame Request not valid. Client Disconnected.");
            return;
        }
        client.stopGame();
    }

    public void SendChat(string msg)
    {
        if (ConnectionState != CONNECTED)
        {
            Debug.LogError("SendChat Request not valid. Client Disconnected.");
            return;
        }
        if (string.IsNullOrEmpty(msg))
        {
            Debug.LogError("SendChat Request not valid. msg IsNullOrEmpty.");
            return;
        }
        client.SendChat(msg);
    }
    
    #endregion

    #region Variables

    readonly WarpClient client = null;
    byte ConnectionState => client.GetConnectionState();
    string userID;
    int password;
    RoomProperties roomProps;
    string roomInfoID;
    string roomID;
    bool isSubscribed;
    bool isJoined;

    #endregion


    #region Constructor

    public WarpListener(WarpClient _client)
    {
        if (_client == null)
        {
            throw new ArgumentNullException($"Wrap Client not properly initialized!\nClient={_client}");
        }
        client = _client;
        client.AddConnectionRequestListener(this);
        client.AddZoneRequestListener(this);
        client.AddRoomRequestListener(this);
        client.AddNotificationListener(this);
    }

    #endregion

    #region Response Processing
    
    #region ConnectionRequestListener
    public void onConnectDone(ConnectEvent eventObj)
    {
        byte result = eventObj != null ? eventObj.getResult() : UNKNOWN_ERROR;
        Debug.Log("on Connect  "+result.ResultCodeToString());
        switch (result)
        {
        case SUCCESS_RECOVERED:
        case SUCCESS:
            // Notify other scripts that connection is ready
            OnConnect?.Invoke(true);
            return;
        case CONNECTION_ERR:
        case CONNECTION_ERROR_RECOVERABLE:
            // This occurs if the underlying TCP connection with AppWarp cloud service got broken.
            // The client will need to reconnect to the service and retry the operation.
            client.RecoverConnection();
            return;
        case UNKNOWN_ERROR:
            // This is an unexpected error. 
            // Retrying the request is recommended if this happens.
            client.Connect(userID);
            return;
        default:
            // Notify other scripts that connection failed to connect
            OnConnect?.Invoke(false);
            return;
        }
    }

    public void onDisconnectDone(ConnectEvent eventObj)
    {
        byte result = eventObj != null ? eventObj.getResult() : UNKNOWN_ERROR;
        Debug.Log("on Disconnect  "+result.ResultCodeToString());
        switch (result)
        {
        case SUCCESS:
            // Notify other scripts that connection ended
            OnDisconnect?.Invoke(true);
            return;
        case UNKNOWN_ERROR:
            client.Disconnect();
            return;
        default:
            // what does it even mean to fail to disconnect.. 
            OnDisconnect?.Invoke(false);
            return;
        }
    }    
    #endregion

    #region ZoneRequestListener
    private Stack<string> ProcessSearchResult(RoomData[] roomData)
    {
        Stack<string> roomIds = new();
        if (roomData != null)
            foreach (RoomData data in roomData)
                roomIds.Push(data.getId());
        return roomIds;
    }
    public void onGetMatchedRoomsDone(MatchedRoomsEvent eventObj)
    {
        byte result = eventObj != null ? eventObj.getResult() : UNKNOWN_ERROR;
        Debug.Log("on Get Matched Rooms  "+result.ResultCodeToString());
        switch (result)
        {
        case SUCCESS:
            // Notify other scripts that rooms were found
            OnSearch?.Invoke(true, ProcessSearchResult(eventObj.getRoomsData()));
            break;
        case UNKNOWN_ERROR:
            // try again
            Search(password);
            return;
        default:
            // failed to search for rooms.. 
            OnSearch?.Invoke(false, null);
            break;
        }
    }
    
    public void onCreateRoomDone(RoomEvent eventObj)
    {
        byte result = eventObj != null ? eventObj.getResult() : UNKNOWN_ERROR;
        Debug.Log("on Create Room  "+result.ResultCodeToString());
        switch (result)
        {
        case SUCCESS:
            JoinRoom(eventObj.getData()?.getId());
            return;
        case UNKNOWN_ERROR:
            // try again
            CreateRoom(roomProps);
            return;
        default:
            // failed to create room.. 
            OnJoinRoom(false, null, null);
            return;
        }
    }
    #endregion

    #region RoomRequestListener
    private List<string> ProcessInfoResult(string[] _users)
    {
        List<string> users = new();
        if (_users != null)
            foreach (string user in _users)
                users.Add(user);
        return users;
    }
    public void onGetLiveRoomInfoDone(LiveRoomInfoEvent eventObj)
    {
        byte result = eventObj != null ? eventObj.getResult() : UNKNOWN_ERROR;
        Debug.Log("on Get Live Room Info  "+result.ResultCodeToString());
        switch (result)
        {
        case SUCCESS:
            OnGetRoomInfo(true, ProcessInfoResult(eventObj.getJoinedUsers()), eventObj.getProperties());
            return;
        case UNKNOWN_ERROR:
            // try again
            client.GetLiveRoomInfo(roomInfoID);
            return;
        default:
            // failed to subscribe room.. 
            OnGetRoomInfo(false, null, null);
            return;
        }
    }

    public void onSubscribeRoomDone(RoomEvent eventObj)
    {
        byte result = eventObj != null ? eventObj.getResult() : UNKNOWN_ERROR;
        RoomData data = eventObj.getData();
        if (data == null) result = UNKNOWN_ERROR;
        Debug.Log("on Subscribe Room  "+result.ResultCodeToString());
        switch (result)
        {
        case SUCCESS:
            isSubscribed = true;
            if (isJoined)
                OnJoinRoom(true, data.getRoomOwner(), userID);
            return;
        case UNKNOWN_ERROR:
            // try again
            client.SubscribeRoom(roomID);
            return;
        default:
            // failed to subscribe room.. 
            OnJoinRoom(false, null, null);
            return;
        }
    }

    public void onUnSubscribeRoomDone(RoomEvent eventObj)
    {
        throw new NotImplementedException();
    }

    public void onJoinRoomDone(RoomEvent eventObj)
    {
        byte result = eventObj != null ? eventObj.getResult() : UNKNOWN_ERROR;
        RoomData data = eventObj.getData();
        if (data == null) result = UNKNOWN_ERROR;
        Debug.Log("on Join Room  "+result.ResultCodeToString());
        switch (result)
        {
        case SUCCESS:
            isJoined = true;
            if (isSubscribed)
                OnJoinRoom(true, data.getRoomOwner(), userID);
            return;
        case UNKNOWN_ERROR:
            // try again
            client.JoinRoom(roomID);
            return;
        default:
            // failed to join room.. 
            OnJoinRoom(false, null, null);
            return;
        }
    }
    
    public void onLeaveRoomDone(RoomEvent eventObj)
    {
        throw new NotImplementedException();
    }

    public void onUpdatePropertyDone(LiveRoomInfoEvent eventObj)
    {
        byte result = eventObj != null ? eventObj.getResult() : UNKNOWN_ERROR;
        Debug.Log("on Update Property Done  "+result.ResultCodeToString());
        switch (result)
        {
        case UNKNOWN_ERROR:
            // try again
            client.UpdateRoomProperties(roomID, SC_GameData.Instance.RoomProps, new());
            return;
        default:
            // result will be returned in onUserChangeRoomProperty if successful
            return;
        }
    }

    #endregion
    
    #region  NotifyListener

    public void onUserJoinedRoom(RoomData roomData, string username)
    {
        // if (roomData == null) { return; } // failed request
        // if (username == userID) { return; } // my join is handled in onJoinRoomDone
        Debug.Log($"on User {username} Joined Room {roomData.getId()}");
        OnJoinRoom?.Invoke(true, roomData.getRoomOwner(), username);
    }

    public void onUserLeftRoom(RoomData roomData, string username)
    {
        OnLeaveRoom?.Invoke(roomData.getRoomOwner(), username, roomData.getId());
    }

    public void onUserChangeRoomProperty(RoomData roomData, string sender, Dictionary<string, object> properties, Dictionary<string, string> lockedPropertiesTable)
    {
        if (properties == null) { return; } // failed request
        OnUpdateProps?.Invoke(properties);
    }

    public void onMoveCompleted(MoveEvent moveEvent)
    {
        if (moveEvent == null) { return; } // failed request
        if (moveEvent.getSender() == userID) { return; } // ignore updates from myself
        OnMoveCompleted?.Invoke(moveEvent.getMoveData(), moveEvent.getNextTurn());
    }

    public void onGameStarted(string sender, string roomId, string nextTurn)
    {
        if (nextTurn == null) { return; } // failed request
        OnGameStarted?.Invoke(nextTurn);

    }

    public void onGameStopped(string sender, string roomId)
    {
        // what should go here?
    }

    public void onChatReceived(ChatEvent eventObj)
    {
        if (eventObj == null) { return; } // failed request
        if (eventObj.getSender() == userID) { return; } // ignore updates from myself
        Debug.Log($"on User {eventObj.getSender()} Sent Chat {eventObj.getMessage()}");
        OnChatReceived(eventObj.getMessage());
    }

    #endregion
    
    #region  Not Used

    public void onInitUDPDone(byte resultCode)
    {
        // I'm not sure what to do with this one.. 
        switch (resultCode)
        {
        case SUCCESS:
            Debug.Log("Init UDP successful");
            return;
        default:
            Debug.Log("Init UDP failed, Error: "+resultCode.ResultCodeToString());
            return;
        }
    }

    public void onDeleteRoomDone(RoomEvent eventObj)
    {      
        throw new NotImplementedException();
    }

    public void onGetAllRoomsDone(AllRoomsEvent eventObj)
    {
        throw new NotImplementedException();
    }

    public void onGetLiveUserInfoDone(LiveUserInfoEvent eventObj)
    {
        throw new NotImplementedException();
    }

    public void onGetOnlineUsersDone(AllUsersEvent eventObj)
    {
        throw new NotImplementedException();
    }

    public void onSetCustomUserDataDone(LiveUserInfoEvent eventObj)
    {
        throw new NotImplementedException();
    }

    public void onSetCustomRoomDataDone(LiveRoomInfoEvent eventObj)
    {
        throw new NotImplementedException();
    }

    public void onLockPropertiesDone(byte result)
    {
        throw new NotImplementedException();
    }

    public void onUnlockPropertiesDone(byte result)
    {
        throw new NotImplementedException();
    }

    public void onRoomCreated(RoomData eventObj)
    {
        throw new NotImplementedException();
    }

    public void onRoomDestroyed(RoomData eventObj)
    {
        throw new NotImplementedException();
    }

    public void onUserLeftLobby(LobbyData eventObj, string username)
    {
        throw new NotImplementedException();
    }

    public void onUserJoinedLobby(LobbyData eventObj, string username)
    {
        throw new NotImplementedException();
    }

    public void onUpdatePeersReceived(UpdateEvent eventObj)
    {
        throw new NotImplementedException();
    }

    public void onPrivateChatReceived(string sender, string message)
    {
        throw new NotImplementedException();
    }

    public void onUserPaused(string locid, bool isLobby, string username)
    {
        throw new NotImplementedException();
    }

    public void onUserResumed(string locid, bool isLobby, string username)
    {
        throw new NotImplementedException();
    }

    public void onPrivateUpdateReceived(string sender, byte[] update, bool fromUdp)
    {
        throw new NotImplementedException();
    }

    public void onNextTurnRequest(string lastTurn)
    {
        throw new NotImplementedException();
    }

    #endregion
    #endregion
}