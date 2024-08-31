using MyQuoridorApp;
using System;
using UnityEngine;

public class SC_SquareLogic : MonoBehaviour
{
    #region Variables

    public static Action<Square> OnSquareClick;
    
    [Header("Components")]
    private MeshRenderer myMeshRenderer;
    [Space]

    [Header("Properties")]
    [SerializeField]
    public Square Position;
    [SerializeField]
    private bool isPossibleMove = false;

    [Header("Colors")]
    [SerializeField]
    private Color highlightColor = new(0.5f, 1f, 0.5f);
    [SerializeField]
    private Color defaultColor = new(0.25f, 0.25f, 0.25f);

    #endregion

    #region MonoBehaviour

    void Awake()
    {    
        myMeshRenderer = GetComponent<MeshRenderer>();
        Position = (Vector2)transform.position;
        name = Position.ToString();
    }

    void OnMouseDown()
    {
        if (isPossibleMove && OnSquareClick != null)
        {
            OnSquareClick.Invoke(Position);
        }
    }
    #endregion

    #region Logic

    /// <summary>
    /// Resets square after player moves.
    /// </summary>
    public void UnHighlight()
    {
        isPossibleMove = false;
        myMeshRenderer.material.color = defaultColor;
    }

    /// <summary>
    /// highlights square and enable click to move
    /// </summary>
    public void Highlight()
    {
        isPossibleMove = true;
        myMeshRenderer.material.color = highlightColor;
    }

    public void ToggleHighlight()
    {
        if (isPossibleMove) 
        {
            UnHighlight();
        } 
        else
        {
            Highlight();
        } 
    }
    #endregion
}
