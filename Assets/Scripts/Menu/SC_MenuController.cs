using System;
using UnityEngine;

public class SC_MenuController : MonoBehaviour
{
    #region Events

    // Menu Events
    public static Action OnUpdateCameraPosition;
    public static Action<Screens> OnChangeScreen;
    public static Action OnToggleColors;
    public static Action<ColorOptions> OnChooseColor;
    public static Action<string> OnOpenLink;
    public static Action<Sliders> OnChangeSlider;
    public static Action<CameraMode> OnToggle2D3D;

    // Game Events
    public static Action OnEndGame;
    public static Action OnStartGame;
    public static Action OnUnDo;

    #endregion

    #region Logic

    public void Bth_EndGame()
    {
        OnEndGame?.Invoke();
    }

    public void Bth_UnDO()
    {
        OnUnDo?.Invoke();
    }

    public void Bth_StartGame()
    {
        OnChangeScreen?.Invoke(Screens.Game);
        OnStartGame?.Invoke();
    }

    public void Btn_QuitGame()
    {
        if (Application.isPlaying)
        {
            Application.Quit();
        }
    }
    public void Btn_OnToggle2D3D(string _D)
    {
        if (Enum.TryParse(_D, out CameraMode D))
        {
            OnToggle2D3D?.Invoke(D);
        }
    }
    public void Btn_ChangeScreen(string _ScreenName)
    {
        if (Enum.TryParse(_ScreenName, out Screens ScreenName))
        {
            OnChangeScreen?.Invoke(ScreenName);
        }
    }
    public void Btn_OpenLink(string url)
    {
        OnOpenLink?.Invoke(url);
    }
    public void Slider_Generic(string _slider)
    {
        if (Enum.TryParse(_slider, out Sliders slider))
        {
            OnChangeSlider?.Invoke(slider);
        }
    }
    public void Btn_ToggleColors()
    {
        OnToggleColors?.Invoke();
    }
    public void Btn_ChooseColor(string _color)
    {
        if (Enum.TryParse(_color, out ColorOptions color))
        {
            OnChooseColor?.Invoke(color);
        }
    }

    #endregion
}
