using System;
using MyQuoridorApp;
using UnityEngine;

/// <summary>
/// UserID, used as identifier <para></para>
/// Color, the pawn and barriers color. can be set at anytime <para></para>
/// Index, the player index for setting up the board <para></para>
/// </summary>
[Serializable]
public class PlayerData
{
    [SerializeField]
    private string userID;
    public string UserID {get => userID;}
    [SerializeField]
    private ColorOptions color;
    public ColorOptions Color {get => color;}
    [SerializeField]
    private int index;
    public int Index {get => index;}

    public PlayerData(string _userID, ColorOptions _color, int _index)
    {   
        userID = _userID;
        color = _color;
        index = _index;
    }

    public static string Serialize(PlayerData playerData)
    {
        if (playerData == null || string.IsNullOrEmpty(playerData.userID))
            return string.Empty;
        return string.Join(",", playerData.userID, playerData.color, playerData.index);
    }

    public static PlayerData Deserialize(string playerData)
    {
        if (string.IsNullOrEmpty(playerData) || playerData.Split(',').Length != 3)
            return null;
        string[] data = playerData.Split(',');
        string userID = data[0];
        if (!Enum.TryParse(data[1], out ColorOptions color))
        {
            Debug.LogError("Failed to Parse Player Data, invalid color: "+data[1]);
            return null;
        }
        if (!int.TryParse(data[2], out int index))
        {
            Debug.LogError("Failed to Parse Player Data, invalid index: "+data[2]);
            return null;
        }
        return new PlayerData(userID, color, index);
    }
}