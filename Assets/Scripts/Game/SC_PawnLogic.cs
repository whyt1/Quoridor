using System;
using UnityEngine;

public class SC_PawnLogic : MonoBehaviour
{
    #region Variables

    private SC_ColorsManager myColorsManager = null;

    #endregion

    #region Methods

    /// <summary>
    /// Used to set the color of the pawn.
    /// </summary>
    /// <param name="_color">String name of color to change to</param>
    private void SetMyColor(string color)
    {
        if (myColorsManager == null || !Enum.TryParse(color, out ColorOptions _color)) {
            Debug.LogError("Failed to Set pawn Color!");
            return;
        }
        myColorsManager.SetPawnColor(gameObject, _color);
    }

    #endregion
    private void Awake()
    {
        myColorsManager = SC_ColorsManager.Instance;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ColorOptions colorOption = (ColorOptions)UnityEngine.Random.Range(0, 9);
            Debug.Log("Changing Pawn Color to "+colorOption.ToString());
            SetMyColor(colorOption.ToString());
        }
    }
}
