using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MyQuoridorApp;
/// <summary>
/// Singleton for importing resources, holding references to gameobjects 
/// </summary>
public class SC_GameData : MonoBehaviour
{

    #region Singleton
    private static SC_GameData instance;
    public static SC_GameData Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject _gameData = GameObject.FindGameObjectWithTag("GameData");
                if (_gameData == null)
                {
                    _gameData = new GameObject("SC_GameData", typeof(SC_GameData));
                }
                instance = _gameData.InitComponent<SC_GameData>();
            }
            return instance;
        }
    }
    #endregion

    #region Variables

    [Header("Game Settings")]
    [SerializeField]
    private int numOfPlayers = 4;
    public int NumOfPlayers
    {
        get => numOfPlayers;
        set => numOfPlayers = value; 
    }
    [SerializeField]
    private int numOfBarriers = 5;
    public int NumOfBarriers
    {
        get => numOfBarriers;
        set => numOfBarriers = value; 
    }

    [Header("Multiplayer Settings")]
    [SerializeField]
    private string userID;
    public string UserID
    {
        get => userID;
        set => userID = value; 
    }

    [SerializeField]
    private int password = 4;
    public int Password
    {
        get => password;
        set => password = value; 
    }
    [SerializeField]
    private List<PlayerData> playersData = new();
    public List<PlayerData> PlayersData
    {
        get => playersData;
        set => playersData = value; 
    }
    public PlayerData myPlayerData 
    {
        get
        {
            foreach (var data in SC_GameData.Instance.PlayersData)
            {
                if (data.UserID == UserID)
                    return data;
            }
            Debug.LogError("Failed to send color update to server");
            return null;
        }
    }
    public string RoomOwner;

    [Header("Timer")]
    public Timer timer; 
    public Image timerFill;
    [SerializeField]
    public int TurnTime 
    {
        get => timer.seconds; 
        set => timer.seconds = value; 
    }

    [SerializeField]
    public RoomProperties RoomProps => new(password, numOfPlayers, numOfBarriers, timer.seconds, PlayersData);

    [Header("Components")]
    [SerializeField]
    public Transform gameBoard;

    [Header("Prefabs")]
    public GameObject PawnPrefab;
    public GameObject BarrierPrefab;


    [Header("Colors")]
    public ColorOptions MyColor = ColorOptions.Error;
    /// <summary>
    /// Set of the colors picked. 
    /// Used to in force different color to each player.
    /// </summary>
    [SerializeField]
    private List<ColorOptions> pickedColors = new();

    /// <summary>
    /// Mapping colors to pawn meshes.
    /// </summary>
    private Dictionary<ColorOptions, Mesh> pawnMeshs;

    /// <summary>
    /// Mapping colors to barrier meshes.
    /// </summary>
    private Dictionary<ColorOptions, Mesh> barrierMeshs;


    // UI Elements Dict
    private Dictionary<string, GameObject> unityObjects;

    #endregion

    #region MonoBehaviour

    void Awake()
    {
        InitUnityObjects();
        InitPawnMeshes();
        InitBarrierMeshes();
    }

    #endregion

    // Taken from GPT use with care! 
    public Vector3 MousePositionOnBoard()
    {
        // Create a plane representing the game board
        Plane boardPlane = new Plane(gameBoard.transform.forward, gameBoard.transform.position);

        // Cast a ray from the camera to the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Calculate the distance from the camera to the intersection point
        if (boardPlane.Raycast(ray, out float enter))
        {
            // Get the intersection point
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 localPosition = gameBoard.InverseTransformPoint(hitPoint);
            localPosition.x = GlobalConstants.BOARD_SIZE+1 - localPosition.x;
            // Calculate the position relative to the game board's origin
            return localPosition;
        }
        return Square.Error;
    }

    #region Colors Logic
    public List<ColorOptions> PickedColors => pickedColors;

    public ColorOptions RandomColor 
    { 
        get 
        {
            int colorsCount =  Enum.GetValues(typeof(ColorOptions)).Length;
            ColorOptions color = (ColorOptions)UnityEngine.Random.Range(1, colorsCount);
            while (pickedColors.Contains(color)) 
            { 
                color = (ColorOptions)((int)(color+1)%colorsCount); 
            }
            return color;
        }
    }

    public void UpdatePickedColors(string newColorName, string oldColorName)
    {
        if (Enum.TryParse(oldColorName, out ColorOptions oldColor) &&
            pickedColors.Contains(oldColor))
        {
            pickedColors.Remove(oldColor);
        }
        if (Enum.TryParse(newColorName, out ColorOptions newColor) &&
            !pickedColors.Contains(newColor))
        {
            pickedColors.Add(newColor);
        } 
    }
    public void UpdatePickedColors(ColorOptions newColor, ColorOptions oldColor)
    {
        if (pickedColors == null) { return; }
        if (pickedColors.Contains(oldColor)) 
        {
            pickedColors.Remove(oldColor);
        } 
        if (!pickedColors.Contains(newColor)) 
        {
            pickedColors.Add(newColor);
        } 
    }

    #endregion
    
    #region UnityObjects
    public bool TryGetScreen(Screens screenName, out GameObject Screen)
    {
        return unityObjects.TryGetValue(screenName.ToString(), out Screen);
    }

    public bool TryGetSlider(Sliders Sliders, out GameObject slider)
    {
        return unityObjects.TryGetValue(Sliders.ToString(), out slider);
    }
    
    public bool TryGetUnityObject(string unityObjectName, out GameObject Object)
    {
        return unityObjects.TryGetValue(unityObjectName, out Object);
    }

    public void ToggleUnityObject(string unityObjectName)
    {
        unityObjects.TryGetValue(unityObjectName, out GameObject Object);
        if (Object == null) { Debug.LogError(unityObjectName); }
        Object.SetActive(!Object.activeSelf);
    }

    public void ToggleUnityObject(string unityObjectName, bool force)
    {
        unityObjects.TryGetValue(unityObjectName, out GameObject Object);
        Object.SetActive(force);
    }

    public void InitUnityObjects()
    {
        unityObjects = new();
        GameObject[] _unityObjects = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (var _unityObject in _unityObjects)
        {
            if (_unityObject == null || !unityObjects.TryAdd(_unityObject.name.TextAfter("_"), _unityObject))
            {
                Debug.LogError($"Failed to add unity Object ({_unityObject.name}) to unityObjects dict");
            }
        }
    }

    #endregion
    #region Resources Logic 

    public bool GetBarrierMesh(ColorOptions color, out Mesh mesh)
    {
        return barrierMeshs.TryGetValue(color, out mesh);
    }    
    public bool GetBarrierMesh(string color, out Mesh mesh)
    {
        mesh = null;
        return Enum.TryParse(color, out ColorOptions _color) && barrierMeshs.TryGetValue(_color, out mesh);
    }
    private void InitBarrierMeshes()
    {
        barrierMeshs = new();
        Mesh[] _barrierMeshs = Resources.LoadAll<Mesh>("Meshes/Barriers");
        foreach (var _mesh in _barrierMeshs)
        {
            if (Enum.TryParse(_mesh.name.TextAfter("_"), out ColorOptions color) && !barrierMeshs.ContainsKey(color))
            {
                barrierMeshs.Add(color, _mesh);
            }
        }
    }

    public bool GetPawnMesh(ColorOptions color, out Mesh mesh)
    {
        return pawnMeshs.TryGetValue(color, out mesh);
    }
    public bool GetPawnMesh(string color, out Mesh mesh)
    {
        mesh = null;
        return Enum.TryParse(color, out ColorOptions _color) && pawnMeshs.TryGetValue(_color, out mesh);
    }
    private void InitPawnMeshes()
    {
        pawnMeshs = new();
        Mesh[] _pawnMeshs = Resources.LoadAll<Mesh>("Meshes/Pawns");
        foreach (var _mesh in _pawnMeshs)
        {
            if (Enum.TryParse(_mesh.name.TextAfter("_"), out ColorOptions color) && !pawnMeshs.ContainsKey(color))
            {
                pawnMeshs.Add(color, _mesh);
            }
        }
    }

    #endregion
}
