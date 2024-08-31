using System.Diagnostics.CodeAnalysis;

namespace MyQuoridorApp
{
public class Move
{
    public Square Source { get; }
    public Square Target { get; }
    public Directions Direction { get => Source.GetDirection(Target); }

    public Move PrevMove = null;
    public int PathCost = 0;

    public Move(Square start, Square end)
    {
        Source = start;
        Target = end;
    }
    public Move()
    {
        Source = Target = Square.Error;
    }

    #region Tuple Conversion

    public static implicit operator (Square, Square)(Move move)
    {
        return (move.Source, move.Target);
    }

    public static implicit operator Move((Square, Square) tuple)
    {
        return new Move(tuple.Item1, tuple.Item2);
    }

    #endregion

    #region String Conversion 

    public static bool TryParse(string str, out Move result)
    {
        return (result = Parse(str)) != null;
    }

    public static Move Parse(string str)
    {
        if (string.IsNullOrEmpty(str) || (str.Length != 4))
        {
            return null;
        }
        if (!Square.TryParse(str.Substring(0, 2), out Square start) ||
            !Square.TryParse(str.Substring(2, 2), out Square end))
        {
            return null;
        }
        return new Move(start, end);
    }

    public static explicit operator Move(string str)
    {
        return Parse(str) as Move;
    }

    // override object.ToString
    public override string ToString()
    {
        return $"{Source}{Target}";
    }

    public static explicit operator string(Move move)
    {
        return move.ToString();
    }

    #endregion

    #region Equality 

    // override object.Equals
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        Move other = (Move)obj;
        // equality between moves between Squares is independent of direction
        // E5 to F5 is equal to F5 to E5
        return Source == other.Source && Target == other.Target ||
                Source == other.Target && Target == other.Source;
    }

    public static bool operator ==(Move left, Move right)
    {
        if (left is null && right is null)
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    public static bool operator !=(Move left, Move right)
    {
        return !(left == right);
    }

    // override object.GetHashCode
    public override int GetHashCode()
    {
        if (Source == null || Target == null) { return 0; }
        return Source.GetHashCode() + Target.GetHashCode();
    }

    #endregion
}
}