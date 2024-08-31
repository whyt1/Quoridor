using System;
using System.Collections.Generic;
using UnityEngine;

using MyQuoridorApp;
using static MyQuoridorApp.GlobalConstants;
public class SC_PlayerLogic : GamePiece
{
    #region Variables
    // Events
    public static Action OnPawnClick;
    
    // Properties

    private List<SC_BarrierLogic> constBarriers = new();
    public List<SC_BarrierLogic> barriers = new();
    public bool HasBarriers => barriers.Count > 0;
    public Predicate<Square> reachedGoal;
    public bool ReachedGoal => reachedGoal != null && reachedGoal(Position);

    #endregion

    #region Logic

    public void AddBarrier(SC_BarrierLogic barrier)
    {
        barriers.Add(barrier);
        constBarriers.Add(barrier);
    }

    protected override void UpdatePosition(Square position)
    {
        myTransform.position = position + new Vector2(0.5f, 0.5f);
    }

    /// <summary>
    /// Used to set the color of the pawn.
    /// </summary>
    /// <param name="newColor">name of color to change to</param>s
    protected override void UpdateColor(ColorOptions newColor)
    {
        if (SC_GameData.Instance == null || !SC_GameData.Instance.GetPawnMesh(newColor, out Mesh mesh) || myMeshFilter == null) 
        {
            Debug.LogError("Failed to Set pawn Color! No Game Data or Mesh");
            return;
        }
        string oldColor = myMeshFilter.mesh.name.TextAfter("_").Replace(" Instance", "");
        SC_GameData.Instance.UpdatePickedColors(newColor.ToString(), oldColor);
        Debug.Log($"the <color={oldColor}>{oldColor}</color> player changed to <color={newColor}>{newColor}</color>");
        myMeshFilter.mesh = mesh;
        foreach (var barrier in constBarriers)
        {
            barrier.Color = newColor;
        }
    }

    protected override void UpdateRotation(Directions startingDirection)
    {
        base.UpdateRotation(startingDirection);
        reachedGoal = startingDirection switch {
             Directions.North => curPos => curPos.Rank == BOARD_SIZE,
             Directions.South => curPos => curPos.Rank == 0,
             Directions.East => curPos => curPos.File == BOARD_SIZE,
             Directions.West => curPos => curPos.File == 0,
             _ => curPos => false
        };
    }

    #endregion

    #region MonoBehaviour 

    void OnMouseDown()
    {
        if (isMine) 
        {
            Debug.Log($"{name} was clicked!");
            OnPawnClick?.Invoke();
        }
    }
    
    #endregion
}
