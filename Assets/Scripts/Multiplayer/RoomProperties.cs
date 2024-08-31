using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class RoomProperties
{
    #region Constructor & Dict Conversion 
    readonly Dictionary<string, object> properties = new();
    public static implicit operator Dictionary<string, object>(RoomProperties props)
    {
        return props.properties;
    }
    public static implicit operator RoomProperties(Dictionary<string, object> props)
    {
        if (!props.TryGetValue("password", out object _password) || _password is not string password)
        {
            throw new ArgumentException("invalid password passed in props dict.");
        }
        if (!props.TryGetValue("numOfPlayers", out object _numOfPlayers) || _numOfPlayers is not string numOfPlayers)
        {
            throw new ArgumentException("invalid numOfPlayers passed in props dict");
        }
        if (!props.TryGetValue("numOfBarriers", out object _numOfBarriers) || _numOfBarriers is not string numOfBarriers)
        {
            throw new ArgumentException("invalid numOfBarriers passed in props dict");
        }
        if (!props.TryGetValue("turnTimer", out object _turnTimer) || _turnTimer is not string turnTimer)
        {
            throw new ArgumentException("invalid turnTimer passed in props dict");
        }
        if (!props.TryGetValue("playersData", out object _playersData) || _playersData is not string playersData)
        {
            if (_playersData != null)
                throw new ArgumentException("invalid pickedColors passed in props dict, value: " + _playersData + " type:" + _playersData.GetType());
            else
                playersData = null;
        }
        return new RoomProperties(int.Parse(password), int.Parse(numOfPlayers), int.Parse(numOfBarriers), int.Parse(turnTimer), DeserializePlayerData(playersData));
    }
    public RoomProperties(int _password, int _numOfPlayers, int _numOfBarriers, int _turnTimer, List<PlayerData> _playersData)
    {
        properties = new()
        {
            { "password", _password.ToString() },
            { "numOfPlayers", _numOfPlayers.ToString() },
            { "numOfBarriers", _numOfBarriers.ToString() },
            { "turnTimer", _turnTimer.ToString() },
            { "playersData", SerializePlayerData(_playersData) }
        };
    }    

    public RoomProperties(int _password)
    {
        properties = new()
        {
            { "password", _password }
        };
    }
    
    public RoomProperties(RoomProperties other)
    {
        properties = new()
        {
            { "password", other.Password },
            { "numOfPlayers", other.NumOfPlayers },
            { "numOfBarriers", other.NumOfBarriers },
            { "turnTimer", other.TurnTimer },
            { "playersData", other.PlayersData }
        };
    }
    #endregion

    #region Properties
    public bool HasProperty(string prop) 
    {
        return properties.ContainsKey(prop);
    }

    [SerializeField]
    public int Password
    {
        get => int.Parse(properties["password"] as string);
        set => properties["password"] = value.ToString(); 
    }
    [SerializeField]
    public int NumOfPlayers
    {
        get => int.Parse(properties["numOfPlayers"] as string);
        set => properties["numOfPlayers"] = value.ToString(); 
    }
    [SerializeField]
    public int NumOfBarriers
    {
        get => int.Parse(properties["numOfBarriers"] as string);
        set => properties["numOfBarriers"] = value.ToString(); 
    }
    [SerializeField]
    public int TurnTimer
    {
        get => int.Parse(properties["turnTimer"] as string);
        set => properties["turnTimer"] = value.ToString(); 
    }
    [SerializeField]
    public List<PlayerData> PlayersData
    {
        get => DeserializePlayerData(properties["playersData"] as string);
        set => properties["playersData"] = SerializePlayerData(value); 
    }

    #endregion
    #region Helper Function
    static string SerializePlayerData(List<PlayerData> players)
    {
        if (players == null || players.Count == 0)
            return string.Empty;
        return string.Join("+", players.Select(e => PlayerData.Serialize(e)));
    }
    static List<PlayerData> DeserializePlayerData(string players)
    {
        if (string.IsNullOrEmpty(players))
            return null;
        return players.Split('+').Select(s => PlayerData.Deserialize(s)).ToList();
    }
    #endregion
}