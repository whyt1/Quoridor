using System;
using System.Collections.Generic;
using UnityEngine;


public enum ColorOptions
{
    Black,
    Blue,
    Cyan,
    Green,
    Grey,
    Purple,
    Red,
    White,
    Yellow
}

[System.Serializable]
public class ColorMeshPair
{
    public ColorOptions color;
    public Mesh mesh;
}


/// <summary>
/// Singleton used throughout the game to set game object colors.
/// </summary>
public class SC_ColorsManager : MonoBehaviour
{

    #region Singleton
    private static SC_ColorsManager instance;
    public static SC_ColorsManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject _colorsManager = GameObject.FindGameObjectWithTag("ColorsManager");
                if (_colorsManager == null)
                {
                    _colorsManager = new GameObject("SC_ColorsManager", typeof(SC_ColorsManager));
                }
                instance = _colorsManager.InitComponent<SC_ColorsManager>();
            }
            return instance;
        }
    }
    #endregion


    #region Variables

    /// <summary>
    /// Mapping colors to pawn meshes.
    /// </summary>
    private Dictionary<ColorOptions, Mesh> pawnMeshs;

    /// <summary>
    /// Mapping colors to barrier meshes.
    /// </summary>
    private Dictionary<ColorOptions, Mesh> barrierMeshs;

    #endregion


    #region Methods
    private Mesh GetBarrierMesh(ColorOptions color)
    {
        if (barrierMeshs == null || !barrierMeshs.ContainsKey(color))
        {
            Debug.LogError("Failed to Get barrier Mesh!");
            return null;
        }
        return barrierMeshs[color];
    }

    private Mesh GetPawnMesh(ColorOptions color)
    {
        if (pawnMeshs == null || !pawnMeshs.ContainsKey(color))
        {
            Debug.LogError("Failed to Get pawn Mesh!");
            return null;
        }
        return pawnMeshs[color];
    }

    #endregion


    #region API

    public void SetBarrierColor(GameObject obj, ColorOptions color)
    {
        Mesh mesh = GetBarrierMesh(color);
        if (mesh == null || obj == null)
        {
            Debug.LogError("Failed to set barrier color!");
            return;
        }
        foreach (var _filter in obj.GetComponentsInChildren<MeshFilter>())
        {
            if (_filter == null) continue;
            _filter.mesh = mesh;
        }
    }

    public void SetPawnColor(GameObject obj, ColorOptions color)
    {
        Mesh mesh = GetPawnMesh(color);
        if (mesh == null || obj == null)
        {
            Debug.LogError("Failed to set pawn color!\nmesh="+mesh+"\nobj="+obj);
            return;
        }
        MeshFilter filter = obj.InitComponent<MeshFilter>();
        filter.mesh = mesh;
    }

    #endregion

    #region MonoBehaviour

    void Awake()
    {
        InitPawnMeshs();
        InitBarrierMeshs();
    }

    #endregion

    #region Logic
    private void InitBarrierMeshs()
    {
        barrierMeshs = new();
        Mesh[] _barrierMeshs = Resources.LoadAll<Mesh>("Meshes/Barriers");
        foreach (var _mesh in _barrierMeshs)
        {
            if (Enum.TryParse(_mesh.name.Replace("Mesh_Barrier_", ""), out ColorOptions color) &&
                !barrierMeshs.ContainsKey(color))
            {
                barrierMeshs.Add(color, _mesh);
            }
        }
    }

    private void InitPawnMeshs()
    {
        pawnMeshs = new();
        Mesh[] _pawnMeshs = Resources.LoadAll<Mesh>("Meshes/Pawns");
        foreach (var _mesh in _pawnMeshs)
        {
            if (Enum.TryParse(_mesh.name.Replace("Mesh_Pawn_", ""), out ColorOptions color) && 
                !pawnMeshs.ContainsKey(color))
            {
                pawnMeshs.Add(color, _mesh);
            }
        }
    }
    #endregion
}
