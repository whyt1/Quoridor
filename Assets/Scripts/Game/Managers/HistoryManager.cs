using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace MyQuoridorApp 
{
[Serializable]
public class HistoryManager
{
    /// <summary>
    /// Game moves history, managed with stack for undoing moves with pop.
    /// </summary>
    [SerializeField]
    private readonly Stack<string> history = new();
    private readonly int numOfPlayers; 

    public HistoryManager(int _numOfPlayers=0) {
        numOfPlayers = _numOfPlayers;
        LastPlay = "Game Start";
    }
    public int Count => history.Count;
    /// <summary>
    /// Accessor for history. get pops the last move from history if possible. set pushed a new move into history.
    /// </summary>
    public string LastPlay 
    { 
        get => history.Count > 0 ? history.Pop() : string.Empty; 
        set { Debug.Log(value); history.Push(value); }
    }

    /// <summary>
    /// converts the game history to string for displaying.
    /// </summary>
    public string GameLog 
    { 
        get 
        {
            if (history.Count <= 0) 
            {
                return "";
            }
            
            StringBuilder result = new();
            int round = 0;
            foreach (var _play in history.Reverse()) 
            {
                // converting play to a string
                string play = _play;
                if (Move.TryParse(play, out Move move)) 
                {
                    play = move.Target.ToString();
                }
                // new line every round change
                if (++round % numOfPlayers == 0) 
                {
                    if (result.Length >= 3) 
                    {
                        // dropping tailing ", "
                        result.Remove(result.Length-2, 2);
                        result.AppendLine();
                    }
                }
                result.Append(play+", ");
            }
            // dropping tailing ", "
            result.Remove(result.Length-2, 2);
            return result.ToString();
        }
    }
}
}