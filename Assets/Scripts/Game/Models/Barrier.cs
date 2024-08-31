using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyQuoridorApp
{
using static Directions;

/// <summary>
/// Represents a Barrier on the board.<para></para>
/// <see cref="File"/>, <see cref="Rank"/> position (file and rank). <para></para>
/// <see cref="Direction"/> direction (vertical or horizontal).<para></para>
/// <see cref="BlockedMoves"/> <c>list of moves blocked by the barrier.</c><para></para>
/// <see cref="BlockedPlacements"/> <c>list of barrier Placements blocked by the barrier.</c><para></para>
/// </summary>
[Serializable]
public class Barrier 
{
    public Directions Direction;
    public Square Position;

    public Barrier(Square position, Directions direction) 
    {
        Position = position;
        Direction = direction;
    }

    public Barrier() 
    {
        Position = Square.Error;
        Direction = Directions.Error;
    }

    public HashSet<Move> BlockedMoves =>
        Direction switch
        {
            Vertical => new() {
                new Move (Position, Position+West),
                new Move (Position+South, Position+South+West)
            },
            Horizontal => new() {
                new Move (Position, Position+South),
                new Move (Position+West, Position+West+South)
            },
            _ => new(),
        };

    public HashSet<Barrier> BlockedPlacements =>
        Direction switch
        {
            Vertical => new() {
                (Position, Direction), (Position, Direction.GetOpposite()),
                (Position+North, Direction), (Position+South, Direction), 
            },
            Horizontal => new() {
                (Position, Direction), (Position, Direction.GetOpposite()),
                (Position+East, Direction), (Position+West, Direction), 
            },
            _ => new(),
        };

    #region Tuple Conversion

    public static implicit operator (Square, Directions)(Barrier barrier)
    {
        return (barrier.Position, barrier.Direction);
    }

    public static implicit operator Barrier((Square, Directions) tuple)
    {
        return new Barrier(tuple.Item1, tuple.Item2);
    }

    public void Deconstruct(out Square _Position, out Directions _Direction)
    {
        _Position = Position;
        _Direction = Direction;
    }

    #endregion

    #region String Conversion 

    public static bool TryParse(string str, out Barrier result) 
    {
        return (result = Parse(str)) != null;
    }

    public static Barrier Parse(string str) 
    {
        if (string.IsNullOrEmpty(str) || str.Length < 3) {
            return null;
        }
        if (!Square.TryParse(str.Substring(0,2), out Square position) ||
            !DirectionsExtensions.TryParse(str.Substring(2), out Directions direction)) {
                return null;
        }
        if (direction == Directions.Error) {
            return null;
        }
        return new Barrier(position, direction);
    }

    public static explicit operator Barrier(string str){
        return Parse(str);
    }


    // override object.ToString
    public override string ToString() {
        return $"{Position} {Direction}";
    }

    public static explicit operator string(Barrier barrier){
        return barrier.ToString();
    }

    #endregion

    #region Equality

    // override object.Equals
    public override bool Equals(object obj) {      
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }
        Barrier other = (Barrier)obj;
        return Position == other.Position && Direction == other.Direction;
    }

    public static bool operator ==(Barrier left, Barrier right) {
        if (left is null && right is null)
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    public static bool operator !=(Barrier left, Barrier right) {
        return !(left == right);
    }

    // override object.GetHashCode
    public override int GetHashCode() {
        return HashCode.Combine(Position, Direction);
    }

    #endregion
}
}