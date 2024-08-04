using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_BarrierLogic : MonoBehaviour
{

    private SC_ColorsManager myColorsManager = null;

    /// <summary>
    /// Used to set the color of the barrier.
    /// </summary>
    /// <param name="_color">String name of color to change to</param>
    private void SetMyColor(string color)
    {
        if (myColorsManager == null || !Enum.TryParse(color, out ColorOptions _color))
        {
            Debug.LogError("Failed to Set pawn Color!");
            return;
        }
        myColorsManager.SetBarrierColor(gameObject, _color);
    }

    private void Awake()
    {
        myColorsManager = SC_ColorsManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ColorOptions colorOption = (ColorOptions)UnityEngine.Random.Range(0, 9);
            Debug.Log("Changing Pawn Color to " + colorOption.ToString());
            SetMyColor(colorOption.ToString());
        }
    }
}
