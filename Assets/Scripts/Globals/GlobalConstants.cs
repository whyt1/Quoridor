using UnityEngine;

namespace MyQuoridorApp
{
public static class GlobalConstants {
    public const int ZERO_FILE = 'A';
    public const int ZERO_RANK = '1';
    /// <summary>
    /// Board Size, used  to calculate board bounds and players goals.
    /// </summary>
    public const int BOARD_SIZE = 8;
    /// <summary>
    /// Maximum number of players allowed to play, 
    /// used when adding players and for avoiding recursion and infinite while loops..
    /// </summary>
    public const int MAX_PLAYERS = 4;
    /// <summary>
    /// Starting positions for each player based on index, 
    /// first player added to the game gets position index 0 and so on.
    /// </summary>
    public static readonly Square[] STARTING_POSITIONS = { 
        (BOARD_SIZE/2, 0), (BOARD_SIZE/2, BOARD_SIZE), (0, BOARD_SIZE/2), (BOARD_SIZE, BOARD_SIZE/2) 
    };
    public static readonly Directions[] STARTING_DIRECTIONS = {
        Directions.North, Directions.South, Directions.East, Directions.West
    };
    /// <summary>
    /// Possible move direction in the game.
    /// </summary>
    public static readonly Directions[] COMPASS_ROSE = new[] {
        Directions.East, Directions.North, Directions.West, Directions.South
    };

    public static readonly (Vector3, Quaternion)[] Camera3DPositions = new (Vector3, Quaternion)[] {
        (new(4.5f, -7.5f, -13), new Quaternion(-0.382683426f,0,0,0.923879564f)), 
        (new(4.5f, 17.5f, -13), new Quaternion(0,-0.382683426f,0.923879564f,0)), 
        (new(-7.5f, 4.5f, -13), new Quaternion(-0.270598054f,0.270598054f,-0.65328151f,0.65328151f)), 
        (new(17.5f, 4.5f, -13), new Quaternion(-0.270598054f,-0.270598054f,0.65328151f,0.65328151f)), 
    };
    public static readonly (Vector3, Quaternion)[] Camera2DPositions = new (Vector3, Quaternion)[] {
        (new(4.5f, 5.5f, -20), new Quaternion(0,0,0,1)), 
        (new(4.5f, 3.5f, -20), new Quaternion(0,0,1,0)), 
        (new(5.5f, 4.5f, -20), new Quaternion(0,0,-0.707106829f,0.707106829f)), 
        (new(3.5f, 4.5f, -20), new Quaternion(0,0,0.707106829f,0.707106829f)), 
    };
}
}