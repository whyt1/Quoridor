using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyQuoridorApp
{
using static GlobalConstants;

/// <summary>
/// Manages the state of the game board.
/// Holds references to each board square.
/// </summary>
[Serializable]
public class BoardManager : ICloneable
{
    #region Variables

    /// <summary>
    /// Holds references to a square script for each square on the board.
    /// </summary>
    Dictionary<Square, SC_SquareLogic> BoardSquares = new();

    /// <summary>
    /// Moves blocked by barriers, used by <seealso cref="IsValidMove"/>.
    /// </summary>
    [SerializeField]
    public HashSet<Move> BlockedMoves { get; } = new();

    /// <summary>
    /// Barrier placements blocked by existing barriers, used by <seealso cref="IsValidPlacement"/>.
    /// </summary>
    [SerializeField]
    HashSet<Barrier> BlockedPlacements { get; } = new();

    /// <summary>
    /// Squares occupied by players, used by <seealso cref="IsValidMove"/> and <see cref="MovePlayer"/>.
    /// </summary>
    [SerializeField]
    HashSet<Square> OccupiedSquares { get; } = new();
    #endregion
    #region Constructor 
    public BoardManager(int numOfPlayers=0) 
    {
        InitBoardSquares();
        for (int i = 0; i < numOfPlayers; i++) 
        {
            OccupiedSquares.Add(STARTING_POSITIONS[i]);
        }
    }

    void InitBoardSquares()
    {
        GameObject[] squares = GameObject.FindGameObjectsWithTag("Square");
        foreach (GameObject square in squares)
        {
            SC_SquareLogic squareScript = square.InitComponent<SC_SquareLogic>();
            if (!BoardSquares.TryAdd(Square.Parse(square.name), squareScript))
            {
                Debug.LogError($"Failed to initialize game board! {square.name}, {squareScript}");
            }
        }
    }

    public object Clone() {
        BoardManager Clone = new();
        Clone.BlockedMoves.UnionWith(BlockedMoves);
        Clone.BlockedPlacements.UnionWith(BlockedPlacements);
        Clone.OccupiedSquares.UnionWith(OccupiedSquares);
        return Clone;
    }
    #endregion
    #region Barrier Placement
    /// <summary>
    /// Checks if a given barrier is on the board.
    /// </summary>
    /// <param name="barrier">The barrier to Check</param>
    /// <returns><c>True</c> if barrier is in board's bounds</returns>
    static bool IsInBounds(Barrier barrier) 
    {
        // barriers take 2 squares so can't be placed on the 0-th file or rank
        return  barrier.Position != Square.Error &&
                0 < barrier.Position.File && barrier.Position.File <= BOARD_SIZE && 
                0 < barrier.Position.Rank && barrier.Position.Rank <= BOARD_SIZE;
    }

    /// <summary>
    /// Checks if a barrier placement is valid.
    /// </summary>
    /// <param name="barrier">barrier to place</param>
    /// <returns><c>True</c> if barrier can be placed</returns>
    public bool IsValidPlacement(Barrier barrier) 
    {
        if (barrier == null) { return false; } // LogErr
        return !BlockedPlacements.Contains(barrier) && IsInBounds(barrier);
    }

    public void AddBlockedPlacement(Barrier barrier) 
    {
        BlockedPlacements.Add(barrier);
    }

    /// <summary>
    /// Places a barrier on the board.
    /// </summary>
    /// <param name="barrier">The barrier to place.</param>
    /// <returns><c>True</c> if barrier was placed successfully</returns>
    public bool PlaceBarrier(Barrier barrier) 
    {
        if (!IsValidPlacement(barrier)) 
        {
            return false;
        }
        // updating board to reflect placement 
        BlockedPlacements.UnionWith(barrier.BlockedPlacements);
        BlockedMoves.UnionWith(barrier.BlockedMoves);
        return true;
    }

    /// <summary>
    /// Removes a barrier on the board.
    /// </summary>
    /// <param name="barrier">The barrier to remove.</param>
    public void RemoveBarrier(Barrier barrier) 
    {
        if (barrier == null) { return; } // LogErr
        // updating board to reflect removal 
        BlockedPlacements.ExceptWith(barrier.BlockedPlacements);
        BlockedMoves.ExceptWith(barrier.BlockedMoves);
    }
    #endregion
    
    #region Player Movement
    /// <summary>
    /// Checks if a given position is on the board.
    /// </summary>
    /// <param name="position">The Position to Check</param>
    /// <returns><c>True</c> if Position is on the board</returns>
    bool IsInBounds(Square position) 
    {
        return  position != Square.Error &&
                0 <= position.File && position.File <= BOARD_SIZE && 
                0 <= position.Rank && position.Rank <= BOARD_SIZE;
    }
    
    /// <summary>
    /// Checks if a player move is valid.
    /// </summary>
    /// <param name="position">current player position</param>
    /// <param name="direction">direction to move in</param>
    /// <param name="target"><c>out</c> The square the player ends on, accounting for possible jumps.</param>
    /// <param name="depth">Used to avoid infinite loops on errors</param>
    /// <returns><c>True</c> if player can be moved</returns>
    public bool IsValidMove(Square position, Directions direction, out Square target, out Square altTarget, int depth = 0) {
        if (position == null || direction == Directions.Error || depth > MAX_PLAYERS) 
        {
            target = altTarget = Square.Error;
            return false; 
        }
        altTarget = Square.Error;
        target = position+direction;
        if (BlockedMoves.Contains(new Move (position, target)) || !IsInBounds(target)) 
        { 
            target = altTarget = Square.Error;
            return false;
        }
        if (OccupiedSquares.Contains(target)) 
        {
            position = target;
            // jump over other players, can jump over all 3 other players
            if (IsValidMove(position, direction, out target, out _,depth+1))
            {
                return true;
            }
            // if jump is blocked try jumping to the sides
            bool sidestep1 = IsValidMove(position, direction.GetNormals().Item1, out target, out _, depth+1);
            bool sidestep2 = IsValidMove(position, direction.GetNormals().Item2, out altTarget, out _, depth+1);
            return sidestep1 || sidestep2;
            // if cant jump and cant jump to the sides move is not valid.
        }
        return true;
    }

    /// <summary>
    /// Moves a player in a given direction. <para>
    /// </para>
    /// Checks valid, Updates occupied and returns target square. 
    /// </summary>
    /// <param name="currPosition">The position of player to move</param>
    /// <param name="direction">direction to move in</param>
    /// <param name="target"><c>out</c> The square the player ends on, accounting for possible jumps.</param>
    /// <returns><c>True</c> if player was moved successfully</returns>
    // public bool MovePlayer(Square currPosition, Directions direction, out Square target) 
    // {
    //     if (!IsValidMove(currPosition, direction, out target)) 
    //     { 
    //         return false; 
    //     }
    //     OccupiedSquares.Remove(currPosition);
    //     OccupiedSquares.Add(target);
    //     return true;
    // }

    /// <summary>
    /// updates the board with a player move. <para></para>
    /// <c>!!!</c> Make Sure to Validate Move before calling this!
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool MovePlayer(Square source, Square target) 
    {
        // // Validation checks already done before this method is called
        // if (!IsValidMove(source, source.GetDirection(target), out _, out _)) 
        // { 
        //     return false; 
        // }
        OccupiedSquares.Remove(source);
        OccupiedSquares.Add(target);
        return true;
    }

    /// <summary>
    /// Gets a list of all possible moves from a given square.
    /// </summary>
    public HashSet<SC_SquareLogic> PossibleMoves(Square square)
    {
        HashSet<SC_SquareLogic> possibleMoves = new();
        foreach (Directions D in COMPASS_ROSE)
        {
            if (IsValidMove(square, D, out Square target, out Square altTarget))
            {
                if (BoardSquares.TryGetValue(target, out SC_SquareLogic possibleSquare)) {
                    possibleMoves.Add(possibleSquare);
                }
                if (BoardSquares.TryGetValue(altTarget, out SC_SquareLogic altPossibleSquare)) {
                    possibleMoves.Add(altPossibleSquare);
                }
            }
        }
        return possibleMoves;
    }

    #endregion

    #region String Conversion 

    public static bool TryParse(string str, out BoardManager result)
    {
        return (result = Parse(str)) != null;
    }

    private enum Types {
        Move,
        Barrier,
        Square, 
        Error
    }

    bool ParsingCurrValue(Types T, string str) {
        switch (T) {
        case Types.Move:
            if (Move.TryParse(str, out Move move)) {
                BlockedMoves.Add(move);
                return true; 
            }
            return false;
        case Types.Barrier:
            if (Barrier.TryParse(str, out Barrier barrier)) {
                BlockedPlacements.Add(barrier);
                return true; 
            }
            return false;
        case Types.Square:
            if (Square.TryParse(str, out Square square)) {
                OccupiedSquares.Add(square);
                return true;
            }
            return false;
        default:
            return false;
        }
    }
    static Types ParsingCurrType(string str) {
        if (str == "BlockedMoves") {
            return Types.Move;
        } else
        if (str == "BlockedPlacements") {
            return Types.Barrier;
        } else
        if (str == "OccupiedSquares") {
            return Types.Square;
        } else {
            return Types.Error;
        }
    }
    public static BoardManager Parse(string str) {
        if (string.IsNullOrEmpty(str) || str[0] != '{') { return null; }
        str = str.Substring(1);
        BoardManager result = new();
        Types currType = Types.Error; 
        string currValue = "";
        foreach (char c in str) {
            switch (c) {
                case '}':
                    // return the result
                    return result;
                case ':':
                    // set current type based on current value
                    currType = ParsingCurrType(currValue);
                    currValue="";
                    break;
                case '\t':                    
                case ',':
                    // add current value to the correct set by current type
                    result.ParsingCurrValue(currType, currValue);
                    currValue = "";
                    break;
                case '\n':
                case ' ':
                case '[':
                case ']':
                    // ignore whitespace and square brackets
                    break;
                default:
                    // build current value until next special char 
                    currValue += c;
                    break;
            }
        }
        return null;
    }

    public static explicit operator BoardManager(string str)
    {
        return Parse(str);
    }

    // override object.ToString
    public override string ToString() 
    {
        return "{\n\tBlockedMoves: [\n\t\t" + string.Join(", ", BlockedMoves) + "\n\t]" + 
                "\n\tBlockedPlacements: [\n\t\t" + string.Join(", ", BlockedPlacements) + "\n\t]" +  
                "\n\tOccupiedSquares: [\n\t\t" + string.Join(", ", OccupiedSquares) + "\n\t]\n}"; 
    }

    public static explicit operator string(BoardManager board)
    {
        return board.ToString();
    }

    #endregion

}
}