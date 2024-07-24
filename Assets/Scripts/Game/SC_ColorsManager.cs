using System;
using System.Collections;
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
/// Singleton used throghout the game to access 
/// game objects and resources.
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

    // List of color-mesh pairs to be set up in the Inspector
    public List<ColorMeshPair> colorPawnMeshs = new();
    /// <summary>
    /// Mapping pawn colors to meshes.
    /// </summary>
    private Dictionary<ColorOptions, Mesh> pawnMeshs;

    // List of color-mesh pairs to be set up in the Inspector
    public List<ColorMeshPair> colorBarrierMeshs = new();
    /// <summary>
    /// Mapping barrier colors to meshes.
    /// </summary>
    private Dictionary<ColorOptions, Mesh> barrierMeshs;

    #endregion

    #region API

    public Mesh GetBarrierMesh(ColorOptions color)
    {
        if (barrierMeshs == null || !barrierMeshs.ContainsKey(color))
        {
            Debug.LogError("Failed to Get barrier Mesh!");
            return null;
        }
        return barrierMeshs[color]; 
    }

    public Mesh GetPawnMesh(ColorOptions color)
    {
        if (pawnMeshs == null || !pawnMeshs.ContainsKey(color))
        {
            Debug.LogError("Failed to Get pawn Mesh!");
            return null;
        }
        return pawnMeshs[color];
    }

    public void SetBarrierColor(GameObject obj, ColorOptions color)
    {
        Mesh mesh = GetBarrierMesh(color);
        if (mesh == null || obj == null)
        {
            Debug.LogError("Failed to set barrier color!");
            return;
        }
        MeshFilter filter = obj.InitComponent<MeshFilter>();
        filter.mesh = mesh;
    }

    public void SetPawnColor(GameObject obj, ColorOptions color)
    {
        Mesh mesh = GetPawnMesh(color);
        if (mesh == null || obj == null)
        {
            Debug.LogError("Failed to set pawn color!");
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
        foreach (var pair in colorBarrierMeshs)
        {
            if (!barrierMeshs.ContainsKey(pair.color))
            {
                barrierMeshs.Add(pair.color, pair.mesh);
            }
        }
    }

    private void InitPawnMeshs()
    {
        pawnMeshs = new();
        foreach (var pair in colorPawnMeshs)
        {
            if (!pawnMeshs.ContainsKey(pair.color))
            {
                pawnMeshs.Add(pair.color, pair.mesh);
            }
        }
    }
    #endregion
}
