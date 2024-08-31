using System;
using UnityEngine;
using MyQuoridorApp;

public class GamePiece : MonoBehaviour
{
    #region Variables

    [Header("Managers")]
    [SerializeField]
    protected SC_GameData GameData = null;

    [Header("Components")]
    [SerializeField]
    protected Transform myTransform = null;
    [SerializeField]
    protected MeshFilter myMeshFilter = null;
    [SerializeField]
    protected MeshRenderer myMeshRenderer = null;
    [SerializeField]
    protected MeshCollider myMeshCollider = null;

    
    [Header("Properties")]
    [SerializeField]
    // set by players manager during Instantiate 
    public bool isMine = true;
    
    [SerializeField]
    private Directions direction;
    public Directions Direction
    {
        get => direction;    
        set
        {
            direction = value;
            UpdateRotation(value);
        }
    }
    protected virtual void UpdateRotation(Directions value)
    {
        if (myTransform != null)
        {
            myTransform.rotation = value.GetRotation();
        } 
    }

    [SerializeField]
    private Square position;
    public Square Position 
    {
        get => position;
        set 
        {
            position = value;
            UpdatePosition(value);
        }
    }
    protected virtual void UpdatePosition(Square position)
    {
        if (myTransform != null)
        {
            myTransform.position = position;
        }   
    }

    [SerializeField]
    private ColorOptions color;
    public ColorOptions Color
    {
        get => color;
        set
        {
            color = value;
            UpdateColor(value);
        }
    }
    protected virtual void UpdateColor(ColorOptions newColor) {}

    public void Initialize(ColorOptions color, Square position, Directions direction)
    {
        this.Color = color;
        this.Position = position;
        this.Direction = direction;
    }

    #endregion

    #region MonoBehaviour
    protected void Awake() 
    {
        myTransform = GetComponent<Transform>();
        myMeshFilter = GetComponent<MeshFilter>();
        myMeshRenderer = GetComponent<MeshRenderer>();
    }
    #endregion

}
