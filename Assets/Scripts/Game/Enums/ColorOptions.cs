using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public enum ColorOptions
{
    Error,
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


public static class ColorOptionsExtensions  {
    static List<Vector4> AllOptionsRGB = new(){
        Color.black, 
        Color.blue, 
        Color.cyan, 
        Color.green,
        Color.grey,
        new(0.45f,0.25f,0.75f,1),
        Color.red,
        Color.white,
        Color.yellow,
    };

    public static Color GetRGB(this ColorOptions color)
    {
        return color switch {
            ColorOptions.Black => Color.black,
            ColorOptions.Blue => Color.blue,
            ColorOptions.Cyan => Color.cyan,
            ColorOptions.Green => Color.green,
            ColorOptions.Grey => Color.grey,
            ColorOptions.Purple => new(0.45f,0.25f,0.75f,1),
            ColorOptions.Red => Color.red,
            ColorOptions.White => Color.white,
            ColorOptions.Yellow => Color.yellow,
            ColorOptions.Error => new(),
            _ => new()
        };
    }

    public static ColorOptions FindClosestOption(this Color target)
    {
        Vector4 closest = Vector4.zero;
        float minDistance = float.MaxValue;

        foreach (Vector4 vector in AllOptionsRGB)
        {
            float distance = Vector4.Distance(target, vector);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = vector;
                // Debug.Log($"<color=#{((Color)closest).ToHexString()}>{(Color)closest}</color>  d={distance}");
            }
        }
        foreach (ColorOptions color in Enum.GetValues(typeof(ColorOptions)))
        {
            if ((Vector4)color.GetRGB() == closest)
            {
                Debug.Log($"closest color to <color=#{target.ToHexString()}>{target}</color> is <color=#{color.GetRGB().ToHexString()}>{color}</color>");
                return color;
            }
        }
        return ColorOptions.Error;
    }

}