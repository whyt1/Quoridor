using System;
using UnityEngine;
using MyQuoridorApp;
using Unity.Mathematics;

public class SC_BarrierLogic : GamePiece
{
    #region Variables

    // Events
    public static Action<SC_BarrierLogic> OnBarrierPlaced;
    public static Action<SC_BarrierLogic> OnBarrierClick;
    public static Action<string> OnBarrierMoved;
    [SerializeField]
    private bool isPlacing;
    [SerializeField]
    public Barrier Home = null;
    [SerializeField]
    Barrier prevPlacement;
    [Header("Materials")]
    [SerializeField]
    private Material TransparentMat; // set in editor
    [SerializeField]
    private Material DefaultMat; // set in editor

    #endregion


    #region Logic

    protected override void UpdatePosition(Square position)
    {
        base.UpdatePosition(position);
        myTransform.position += new Vector3(0,0,-0.25f);
    }

    /// <summary>
    /// Used to set the color of the barrier.
    /// </summary>
    /// <param name="_color">String name of color to change to</param>
    protected override void UpdateColor(ColorOptions newColor)
    {
        if (SC_GameData.Instance == null || !SC_GameData.Instance.GetBarrierMesh(newColor, out Mesh mesh) || myMeshFilter == null) 
        {
            Debug.LogError("Failed to Set Barrier Color! No Game Data or Mesh filter");
            return;
        }
        string oldColor = myMeshFilter.mesh.name.TextAfter("_").Replace(" Instance", "");;
        myMeshFilter.mesh = mesh;
    }

    protected override void UpdateRotation(Directions value)
    {
        if (myTransform != null)
        {
            myTransform.rotation = value.GetRotation();
        } 
    }

    Barrier GetBarrierFromMousePosition()
    {
        Vector3 mousePosition = SC_GameData.Instance.MousePositionOnBoard();
        Vector2 relativePosition = new(mousePosition.x%1, mousePosition.y%1);
        float xDist = math.min(Vector2.Distance(relativePosition, new(0.5f, 0)), Vector2.Distance(relativePosition, new(0.5f, 1)));
        float yDist = math.min(Vector2.Distance(relativePosition, new(0, 0.5f)), Vector2.Distance(relativePosition, new(1, 0.5f)));

        Directions direction = xDist < yDist ? Directions.Horizontal : Directions.Vertical;
        Square position = new Vector2(math.round(mousePosition.x), math.round(mousePosition.y));
        return (position, direction);
    }

    public void CancelPlacement()
    {
        Debug.Log("Cancelling placement...");
        isPlacing = false;
        myMeshRenderer.material = DefaultMat; // TODO null checks
        // take me home, country roads!
        (Position, Direction) = Home;
        OnBarrierMoved?.Invoke($"{name},{Home}");
    }

    void PlaceBarrier()
    {
        // Cancel placement on any other than left mouse
        if (!Input.GetMouseButton(0) && Input.anyKey)
        {
            CancelPlacement();
            return;
        }
        Barrier currPlacement = GetBarrierFromMousePosition();
        // can only move to valid spots
        if (prevPlacement != currPlacement && SC_GameLogic.Instance.IsValidPlacement(currPlacement))
        {
            (Position, Direction) = prevPlacement = currPlacement;
            OnBarrierMoved?.Invoke($"{name},{currPlacement}");
        }
        bool canPlace = prevPlacement == currPlacement || SC_GameLogic.Instance.IsValidPlacement(currPlacement);
        // Place Barrier on mouse/touch release
        if (Input.GetMouseButtonUp(0) && canPlace) 
        {
            OnBarrierPlaced?.Invoke(this);
            isPlacing = false;
            myMeshRenderer.material = DefaultMat; // TODO null checks
        }
    }

    public void ToggleCollider()
    {
        myMeshCollider.enabled = !myMeshCollider.enabled; // placed barriers no longer clickable
    }

    public void SetMaterial(int i)
    {
        if (i == 0)
            myMeshRenderer.material = DefaultMat;
        else
            myMeshRenderer.material = TransparentMat;
    }

    private void OnChatReceived(string msg)
    {
        string[] nameBarrier = msg.Split(",");
        if (nameBarrier.Length == 2 && nameBarrier[0] == name)
            if (Barrier.TryParse(nameBarrier[1], out Barrier barrier))
            {
                OnBarrierMoved(null);
                (Position, Direction) = barrier;
                if (barrier == Home)
                {
                    SC_GameLogic.Instance.PlacingBarrier = null;
                    myMeshRenderer.material = DefaultMat;
                }
                else
                {
                    SC_GameLogic.Instance.PlacingBarrier = this;
                    myMeshRenderer.material = TransparentMat;
                }
            }
    }

    #endregion

    #region MonoBehaviour
    void OnEnable()
    {
        WarpListener.OnChatReceived += OnChatReceived;
    }

    void OnDisable()
    {
        WarpListener.OnChatReceived -= OnChatReceived;
    }
    void OnMouseDown()
    {
        // ignore clicks on barriers that don't belong to me or not during my turn
        if (!isMine || !SC_GameLogic.Instance.IsMyTurn || isPlacing) { return; } 
        Debug.Log($"{name} was clicked!");
        isPlacing = true;
        myMeshRenderer.material = TransparentMat; // TODO null checks
        OnBarrierClick?.Invoke(this);
    }

    void Update()
    {
        if (!isPlacing) { return; } 
        PlaceBarrier();
    }


    #endregion

}
