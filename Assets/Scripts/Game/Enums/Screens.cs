using UnityEngine;

public enum Screens
{
    Error,
    MainMenu, 
    SinglePlayer, 
    Game,
    MultiPlayer, 
    Host,
    Searching, 
    Waiting,
    StudentInfo, 
    Options, 
    Previous, 
}
public static class ScreensExtensions  {
    public static void TurnOffScreen(this Screens screenName)
    {
        if (SC_GameData.Instance.TryGetScreen(screenName, out GameObject screen))
        {
            screen.SetActive(false);
        }
    }
    public static void TurnOnScreen(this Screens screenName)
    {
        if (SC_GameData.Instance.TryGetScreen(screenName, out GameObject screen))
        {
            screen.SetActive(true);
        }
    }
    public static void ToggleScreen(this Screens screenName)
    {
        if (SC_GameData.Instance.TryGetScreen(screenName, out GameObject screen))
        {
            screen.SetActive(!screen.activeSelf);
        }
    }
}