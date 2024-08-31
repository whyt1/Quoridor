using System;
using UnityEngine;

namespace MyQuoridorApp
{
    using static GlobalConstants;

    /// <summary>
    /// using chess algebraic notation to represent board positions.
    /// </summary>
    [Serializable]
    public class Square 
    {
        #region Variables & Contractor 

        [SerializeField]
        private int file;
        /// <summary>
        /// File represents the column (x coordinate) 
        /// </summary>
        public int File => file;

        [SerializeField]
        private int rank;
        /// <summary>
        /// Rank representing the row (y coordinate) 
        /// </summary>
        public int Rank => rank;

        public static readonly Square Error = (-1,-1);
    
        public Square(int _file, int _rank) 
        {
            file = _file;
            rank = _rank;
        }

        public Square() 
        {
            file = Error.File;
            rank = Error.Rank;
        }
        #endregion

        #region Moves

        /// <summary>
        /// Get the direction a given Square is in.
        /// </summary>
        /// <param name="other">The Square to get direction to</param>
        /// <returns><c>Directions</c> Example: "E5".GetDirection("E6") => North (E6 is North to E5)</returns>
        public Directions GetDirection(Square other) 
        {
            (int, int) diff = (other.File-File, other.Rank-Rank);
            if (diff.Item2 == 0) {
                if (diff.Item1 > 0) { return Directions.East; }
                if (diff.Item1 < 0) { return Directions.West; }
            }
            if (diff.Item1 == 0) {
                if (diff.Item2 > 0) { return Directions.North; }
                if (diff.Item2 < 0) { return Directions.South; }
            }
            return Directions.Error;
        }

        /// <summary>
        /// Get the Square in the given direction.
        /// </summary>
        /// <param name="d">the direction</param>
        /// <returns><c>Square</c> Example: "E5".GetMove(North) => "E6" (E6 is North to E5)</returns>
        public Square GetTarget(Directions d)
        {
            return d switch {
                Directions.East => (File+1, Rank),
                Directions.North => (File, Rank+1),
                Directions.West => (File-1, Rank),
                Directions.South => (File, Rank-1),
                _ => Error,
            };
        }

        public static Square operator+(Square pos, Directions d) 
        {
            return pos.GetTarget(d);
        }

        #endregion

        #region Vector Conversion

        public static implicit operator Vector3(Square square)
        {
            return new(square.File, square.Rank);
        }

        public static implicit operator Square(Vector3 vector)
        {
            return new Square((int)vector.x, (int)vector.y);
        }

        public static implicit operator Vector2(Square square)
        {
            return new(square.File, square.Rank);
        }

        public static implicit operator Square(Vector2 vector)
        {
            return new Square((int)vector.x, (int)vector.y);
        }

        public static implicit operator (int, int)(Square square)
        {
            return (square.File, square.Rank);
        }

        public static implicit operator Square((int, int) tuple)
        {
            return new Square(tuple.Item1, tuple.Item2);
        }

        #endregion

        #region String Conversion 

        public static bool TryParse(string str, out Square result) 
        {
            return (result = Parse(str)) != null;
        }

        public static Square Parse(string str) 
        {
            if (string.IsNullOrEmpty(str) || str.Length != 2) 
            {
                return null;
            }
            return new Square(str[0]-ZERO_FILE, str[1]-ZERO_RANK);
        }


        public static explicit operator Square(string str) 
        {
            return Parse(str);
        }

        // override object.ToString
        public override string ToString() 
        {
            if (this == Error) 
            {
                return "Error";
            }
            return $"{(char)(File+ZERO_FILE)}{(char)(Rank+ZERO_RANK)}";
        }

        public static explicit operator string(Square square)
        {
            return square.ToString();
        }

        #endregion

        #region Equality

        // override object.Equals
        public override bool Equals(object obj) {      
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            Square other = (Square)obj;
            return File == other.File && Rank == other.Rank;
        }

        public static bool operator ==(Square left, Square right) {
            if (left is null && right is null)
                return true;
            if (left is null || right is null)
                return false;
            return left.Equals(right);
        }

        public static bool operator !=(Square left, Square right) {
            return !(left == right);
        }

        // override object.GetHashCode
        public override int GetHashCode() {
            return HashCode.Combine(File, Rank);
        }

        #endregion
    }
}