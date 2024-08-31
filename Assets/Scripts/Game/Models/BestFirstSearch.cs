

namespace MyQuoridorApp 
{
using static GlobalConstants;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Directions;
    using UnityEngine;

    public class BestFirstSearch : IComparable<BestFirstSearch> {

    public static Dictionary<SC_PlayerLogic, BestFirstSearch> ParallelSearch(PlayersManager players, BoardManager board) {
        SC_PlayerLogic currentPlayer = players.Current;
        Dictionary<SC_PlayerLogic, BestFirstSearch> searches = players.ToDictionary(
            player=>player,
            player=>new BestFirstSearch(player, board)
        );
        // setting up searches 
        foreach (var s in searches.Values) { s.GetNext(); s.ExpandNext(); }
        while (searches.Values.Any(s=>s.Alive)) {
            // makes a simulated move with current player
            searches[players.Current].GetNext(); 
            // calculating best moves for next player
            searches[players.NextTurn()].ExpandNext(); 
        }
        // setting current back to original
        while (currentPlayer != players.NextTurn());
        return searches;
    }

    #region Fields and Properties 
    delegate int Heuristic(Move move);

    private readonly BoardManager board;
    readonly Predicate<Square> IsGoal;
    readonly Heuristic CostToGoal;
    readonly PriorityQueue<Move, int> frontier = new();
    readonly Dictionary<Square, Move> reached = new();
    readonly Stack<Move> moves = new();
    private List<Move> myMoves;

    Move Current;
    Move End;

    public bool Failed => !Alive && End == null;
    public bool Succeeded => End != null;
    public bool Alive => (frontier.Count > 0 || AnyMovesLeft) && !Succeeded;    
    public int Cost => End != null ? End.PathCost : int.MaxValue;
    public bool HasMoves => moves.Count > 0;
    public string NextMove => moves.Count > 0 ? moves.Pop().Target.ToString() : "";
    private bool AnyMovesLeft 
    { 
        get
        {
            foreach (Move next in PossibleMoves) {
                // Debug.Log($"<color=#E720B0>possible move: {next}</color>");
                if (!reached.TryGetValue(next.Target, out Move move) || 
                    next.PathCost < move.PathCost)
                {
                    return true;
                }
            }
            return false;
        }
    }
    public string NextBlockingBarrier 
    {
        get 
        {
            if (moves.Count <= 0) { return ""; }
            Move nextMove = moves.Pop();
            bool rand = UnityEngine.Random.Range(0, 2) % 2 == 0;
            return nextMove.Direction switch 
            {
                West => (rand ? nextMove.Source : nextMove.Source+North).ToString() + Vertical.ToString(),
                East => (rand ? nextMove.Target : nextMove.Target+North).ToString() + Vertical.ToString(),
                North => (rand ? nextMove.Target : nextMove.Target+East).ToString() + Horizontal.ToString(),
                South => (rand ? nextMove.Source : nextMove.Source+East).ToString() + Horizontal.ToString(),
                _ => ""
            };
        }
    }
    public bool IsBlockingMe(Barrier barrier)
    {
        if (barrier == null || myMoves == null || myMoves.Count < 1) 
        { 
            return false; 
        }
        var blockedMoves = barrier.BlockedMoves;
        foreach (Move move in myMoves)
        {
            if (blockedMoves.Contains(move))
            {
                return true;
            }
        } 
        return false;
    }
    #endregion

    public BestFirstSearch(SC_PlayerLogic p, BoardManager b) {
        board = b;
        IsGoal = SetGoal(p);
        CostToGoal = SetHeuristic(p);
        End = null;
        Current = new(Square.Error, p.Position);
        frontier.Enqueue(Current, CostToGoal(Current));
        reached[Current.Target] = Current;
    }

    private static Predicate<Square> SetGoal(SC_PlayerLogic p) {
        return p.Direction switch
        {
            Directions.East => curPos => curPos.File == BOARD_SIZE,
            Directions.North => curPos => curPos.Rank == BOARD_SIZE,
            Directions.West => curPos => curPos.File == 0,
            Directions.South => curPos => curPos.Rank == 0,
            _ => curPos => true,
        };
        ;    
    }
    private static Heuristic SetHeuristic(SC_PlayerLogic p) {
        return p.Direction switch
        {
            Directions.East => move => BOARD_SIZE - move.Target.File,
            Directions.North => move => BOARD_SIZE - move.Target.Rank,
            Directions.West => move => move.Target.File - 0,
            Directions.South => move => move.Target.Rank - 0,
            _ => move => int.MaxValue,
        };
        ;    
    }
    private void SearchCompleted() {
        frontier.Clear();
        End = Current;
        while (Current.PrevMove != null) {
            moves.Push(Current);
            Current = Current.PrevMove;
        }
        myMoves = moves.ToList();
    } 

    public bool UntilCompleted() {
        while (Alive) {
            GetNext();
            ExpandNext();
        }
        return Succeeded;
    }

    public void GetNext() {
        if (frontier.TryDequeue(out Move current, out int costToGoal) &&
            board != null && board.MovePlayer(Current.Target, current.Target)) {
            // Debug.Log($"<color=#20E7B0>Simulating move {Current}</color>");
            Current = current;
            if (IsGoal(current.Target)) {
                SearchCompleted();
                return;
            }
        }
    }
    public void ExpandNext() {
        if (Succeeded) { return; }
        foreach (Move next in PossibleMoves) {
            // Debug.Log($"<color=#E720B0>possible move: {next}</color>");
            if (!reached.TryGetValue(next.Target, out Move move) || 
                next.PathCost < move.PathCost) {
                    reached[next.Target] = next;
                    frontier.Enqueue(next, CostToGoal(next)+next.PathCost);
            }
        }
    }        
    
    private IEnumerable<Move> PossibleMoves { 
        get {
            Square position = Current.Target;
            foreach (var possibleSquare in board.PossibleMoves(position)) {
                Move next = new(position, possibleSquare.Position) {
                    PathCost = Current.PathCost+1,
                    PrevMove = Current
                };
                yield return next;
            } 
        }
    }

    public int CompareTo(BestFirstSearch other) {
        if (other == null) { return -1; }
        return Cost.CompareTo(other.Cost);
    }
}
}