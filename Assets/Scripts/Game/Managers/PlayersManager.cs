using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyQuoridorApp
{
using static GlobalConstants;

/// <summary>
/// Manages the players, the current player index, and their interactions.
/// </summary>
[Serializable]
public class PlayersManager : IEnumerable<SC_PlayerLogic>
{
    #region Variables
    private SC_GameData GameData => SC_GameData.Instance;

    /// <summary>
    /// Holds the reference to the players in the game.
    /// Used with <see cref="currPlayerIndex"/> to retrieve the reference to the current player.
    /// </summary>
    [SerializeField]
    private readonly List<SC_PlayerLogic> players = new();

    [SerializeField]
    public SC_PlayerLogic myPlayer { get; private set; }

    /// <summary>
    /// Index of the current player, indicating who's turn it is 
    /// </summary>
    [SerializeField]
    private int currPlayerIndex;

    [SerializeField]
    string myUserId;
    #endregion

    public PlayersManager(int numOfPlayers, string _myUserId=null)
    {
        myUserId = _myUserId;
        for (int i = 0; i < numOfPlayers; i++)
        {
            AddPlayer(null, ColorOptions.Error, i);
        }
    }

    #region Logic
    public SC_PlayerLogic GetPlayer(int index)
    {
        return players[index];
    }
    public SC_PlayerLogic GetPlayer(string name)
    {
        foreach (var player in players)
        {
            if (player == null) { break; }
            if (player.name == name)
                return player;
        }
        Debug.LogError("Failed to get player, name: "+name);
        return null;
    }

    public void AddPlayer(string name, ColorOptions color, int i)
    {
        var player = GameObject.Instantiate(GameData.PawnPrefab, GameData.gameBoard).InitComponent<SC_PlayerLogic>();
        player.name = name ?? $"Player {i+1}";
        player.isMine = name == null ? i == 0 : name == myUserId;
        player.Position = STARTING_POSITIONS[i];
        player.Direction = STARTING_DIRECTIONS[i];
        if (player.isMine) 
        {
            myPlayer=player;
            SC_MenuController.OnUpdateCameraPosition?.Invoke();
            if (GameData.MyColor == ColorOptions.Error)
            {
                player.Color = color != ColorOptions.Error ? color : GameData.RandomColor;
                GameData.MyColor = player.Color;
            }
            else 
            {
                player.Color = GameData.MyColor;
            }
        }
        else
        {
            player.Color = color != ColorOptions.Error ? color : GameData.RandomColor;
        }
        InstantiateBarriers(GameData.NumOfBarriers, player);

        players.Add(player);
    }
    #endregion

    #region Logic 

    static private (Vector2, Vector2) FindBarriersStartingPosition(Square Position)
    {
        Vector2 zeroPosition, incrementPosition;
        zeroPosition = incrementPosition = Vector2.zero; 
        if (Position.Rank == BOARD_SIZE)
        {
            zeroPosition = new(0, BOARD_SIZE+2);
            incrementPosition = new(1, 0);
        }
        else if (Position.Rank == 0)
        {
            zeroPosition = new(0, -1);
            incrementPosition = new(1, 0);
        }
        else if (Position.File == BOARD_SIZE)
        {
            zeroPosition = new(BOARD_SIZE+2, 0);
            incrementPosition = new(0, 1);
        }
        else if (Position.File == 0)
        {
            zeroPosition = new(-1, 0);
            incrementPosition = new(0, 1);
        }
        return (zeroPosition, incrementPosition);
    }

    protected static void InstantiateBarriers(int numOfBarriers, SC_PlayerLogic player)
    {    
        Vector2 zeroPosition, incrementPosition;
        (zeroPosition, incrementPosition) = FindBarriersStartingPosition(player.Position);
        // GameObject BarriersParent = new($"{player.name}'s Barriers"); // save to destroy on game reset
        for (int i = 0; i < numOfBarriers; i++) 
        {
            // var barrier = GameObject.Instantiate(GameData.BarrierPrefab, BarriersParent.transform).InitComponent<SC_BarrierLogic>();
            var barrier = GameObject.Instantiate(SC_GameData.Instance.BarrierPrefab, SC_GameData.Instance.gameBoard).InitComponent<SC_BarrierLogic>();
            barrier.name = player.name+$" Barrier {i+1}";
            barrier.isMine = player.isMine;
            // barrier.Position = zeroPosition + incrementPosition * i;
            // barrier.Direction = player.Direction.GetBarrierDirections();
            // barrier.Color = player.Color;
            barrier.Initialize(player.Color, zeroPosition + incrementPosition * i, player.Direction.GetBarrierDirections());
            barrier.Home = ((zeroPosition + incrementPosition * i), player.Direction.GetBarrierDirections());
            player.AddBarrier(barrier);
        }
    }

    #endregion
    
    #region API
    public SC_PlayerLogic Current => players?[currPlayerIndex];

    /// <summary>
    /// Advances current player to next and returns it
    /// </summary>
    public SC_PlayerLogic NextTurn()
    {
        GameData.timer.StopTimer();
        currPlayerIndex = (currPlayerIndex + 1) % players.Count;
        GameData.timerFill.color = Current.Color.GetRGB();
        GameData.timer.StartTimer();
        return Current;
    }

    /// <summary>
    /// Reverses current player to prev and returns it
    /// </summary>
    public SC_PlayerLogic PrevTurn()
    {
        GameData.timer.StopTimer();
        currPlayerIndex = (currPlayerIndex - 1 + players.Count) % players.Count;
        GameData.timerFill.color = Current.Color.GetRGB();
        GameData.timer.StartTimer();
        return Current;
    }
    #endregion

    #region Enumerator Interface
    public int Count => players != null ? players.Count : 0;

    public IEnumerator<SC_PlayerLogic> GetEnumerator()
    {
        return ((IEnumerable<SC_PlayerLogic>)players).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)players).GetEnumerator();
    }
    #endregion

    }
}