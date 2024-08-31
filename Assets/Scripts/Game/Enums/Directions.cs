using System;
using UnityEngine;

namespace MyQuoridorApp
{
/// <summary>
/// Possible Move or Placements Directions
/// </summary>
public enum Directions {
    East = 'E',
    South = 'S',
    West = 'W',
    North = 'N',
    Horizontal = 'H',
    Vertical = 'V',
    Error = 0
};

public static class DirectionsExtensions  {
    public static bool TryParse(string str, out Directions result) 
    {
        if (string.IsNullOrEmpty(str)) 
        { 
            result = Directions.Error; 
            return false;
        }
        if (str.Length == 1) 
        { 
            switch (str[0]) {
            case 'E':
            case 'N':
            case 'W':
            case 'S':
            case 'H':
            case 'V':
                result = (Directions)str[0];
                return true;
            default:
                result = Directions.Error;
                return false;
            } 
        }
        return Enum.TryParse(str, out result);
    }


            // Directions.East => new(0.707106769f,0,0,0.707106769f),
            // Directions.West => new(0,0.707106769f,0.707106769f,0),
            // Directions.Horizontal => new(0,0,1,0),
            // Directions.North => new(0.5f,0.5f,0.5f,0.5f),
            // Directions.South => new(0.5f,-0.5f,-0.5f,0.5f),
            // Directions.Vertical => new(0,0,0.707106829f,0.707106829f),
    public static Quaternion GetRotation(this Directions direction)
    {
        return direction switch {
            Directions.East => new(-0.707106769f,0,0,0.707106769f),
            Directions.West => new(0,0.707106769f,-0.707106769f,0),
            Directions.Horizontal => new(-1,0,0,0),
            Directions.North => new(-0.5f,-0.5f,0.5f,0.5f),
            Directions.South => new(0.5f,-0.5f,0.5f,-0.5f),
            Directions.Vertical => new(-0.707106829f,-0.707106829f,0,0),
            _ => new()
        };
    }

    public static Directions GetBarrierDirections(this Directions direction) 
    {
        return direction switch {
            Directions.North => Directions.Vertical,
            Directions.South => Directions.Vertical,
            Directions.East => Directions.Horizontal,
            Directions.West => Directions.Horizontal,
            _ => Directions.Error
        };
    }
    public static Directions GetOpposite(this Directions direction) 
    {
        return direction switch {
            Directions.East => Directions.West,
            Directions.West => Directions.East,
            Directions.North => Directions.South,
            Directions.South => Directions.North,
            Directions.Horizontal => Directions.Vertical,
            Directions.Vertical => Directions.Horizontal,
            _ => Directions.Error
        };
    }

    public static (Directions, Directions) GetNormals(this Directions direction) 
    {
        return direction switch {
            Directions.East => (Directions.North, Directions.South),
            Directions.West => (Directions.South, Directions.North),
            Directions.North => (Directions.East, Directions.West),
            Directions.South => (Directions.West, Directions.East),
            Directions.Horizontal => (Directions.Vertical, Directions.Vertical),
            Directions.Vertical => (Directions.Horizontal, Directions.Horizontal),
            _ => (Directions.Error, Directions.Error),
        };
    }
}
}