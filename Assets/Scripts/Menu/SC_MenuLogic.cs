using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Mathematics;
using MyQuoridorApp;
using Unity.VisualScripting;
public class SC_MenuLogic : MonoBehaviour
{
    
    #region Singleton
    private static SC_MenuLogic instance;
    public static SC_MenuLogic Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject _menuLogic = GameObject.FindGameObjectWithTag("MenuLogic");
                if (_menuLogic == null)
                {
                    _menuLogic = new GameObject("SC_MenuLogic", typeof(SC_MenuLogic));
                }
                instance = _menuLogic.InitComponent<SC_MenuLogic>();
            }
            return instance;
        }
    }
    #endregion

    #region Variables
    private SC_GameData GameData;
    private Stack<Screens> ScreensStack;

    [Header("Game Over Popup")]
    [SerializeField]
    TextMeshProUGUI gameOverTxt;
    [SerializeField]
    Button btn_rematchSingle;
    [SerializeField]
    Button btn_rematchMulti;

    [Header("Audio Manager")]
    public SC_AudioLogic AudioManager;

    [Space]
    [Header("Camera Mode Components")]
    public Camera camera2D;
    public Camera camera3D;
    public Button button2D;
    public Button button3D;
    public Canvas canvasGame;

    [Space]
    [Header("Colors Components")]
    public GameObject ColorsPopUp;
    public Image currColorImg;
    #endregion

    #region Logic

    private void InitLogic()
    {
        GameSettingsDisplay(false);
        ScreensStack = new Stack<Screens>();
        foreach (Screens screen in Enum.GetValues(typeof(Screens)))
        {
            screen.TurnOffScreen();
        }
        OnChangeScreen(Screens.MainMenu);
        GameData.ToggleUnityObject("GameOver", false);
    }
    
    #endregion

    #region Events

    private void OnGameOver()
    {
        // TODO open pop up and have option to restart
        GameData.ToggleUnityObject("GameOver", true);
        GameData.ToggleUnityObject("MultiStatus", false);

        if (SC_GameLogic.Instance.isMultiplayer)
        {
            btn_rematchMulti.gameObject.SetActive(true);
            btn_rematchSingle.gameObject.SetActive(false);
        }
        else
        {
            btn_rematchMulti.gameObject.SetActive(false);
            btn_rematchSingle.gameObject.SetActive(true);
        }
        if (gameOverTxt != null)
        {
            ColorOptions color = SC_GameLogic.Instance.Winner != null ? SC_GameLogic.Instance.Winner.Color : ColorOptions.Error;
            Debug.Log($"{color} won");
            string hex_color = color.GetRGB().ToHexString();
            gameOverTxt.text = $"The <color=#{hex_color}>{color}</color> Player Won !";
        }
        else 
            Debug.LogError("Failed to get GameOver text, SET IN INSPECTOR");
    }

    private void OnChangeScreen(Screens screenName)
    {
        if (ScreensStack.Count > 0)
        {
            if (screenName == ScreensStack.Peek())
                return;
            ScreensStack.Peek().TurnOffScreen();
        }
        if (screenName == Screens.Previous)
        {
            if (ScreensStack.Count > 1) { ScreensStack.Pop(); }
        }
        else 
        { 
            ScreensStack.Push(screenName); 
        }
        ScreensStack.Peek().TurnOnScreen();
        // show game settings on either singleplayer or host, whenever setting up a new game
        GameSettingsDisplay(ScreensStack.Peek() == Screens.SinglePlayer || ScreensStack.Peek() == Screens.Host);
        // toggle game to avoid collisions with menu
        GameData.gameBoard.gameObject.SetActive(ScreensStack.Peek() == Screens.Game);
    }

    void GameSettingsDisplay(bool active)
    {
        Sliders.NumberOfPlayers.ToggleSlider(active);
        Sliders.TurnTimer.ToggleSlider(active);
        Sliders.BarriersCount.ToggleSlider(active);
    }

    private void OnOpenLink(string url)
    {
        // should validate the url to avoid security risk 
        Application.OpenURL(url);
    }

    private void OnToggleColors()
    {
        ColorsPopUp.SetActive(!ColorsPopUp.activeSelf);
        if (ColorsPopUp.activeSelf)
        {
            currColorImg.color = GameData.MyColor.GetRGB();
            foreach (var btn in ColorsPopUp.GetComponentsInChildren<Button>())
            {
                if (Enum.TryParse(btn.name.TextAfter("_"), out ColorOptions color))
                    btn.interactable = !GameData.PickedColors.Contains(color);
            }
        }
    }

    private void OnChooseColor(ColorOptions newColor)
    {
        GameData.MyColor = newColor;
        currColorImg.color = newColor.GetRGB();
        var myPlayer = SC_GameLogic.Instance.MyPlayer;
        if (myPlayer != null) myPlayer.Color = newColor;
        if (SC_GameLogic.Instance.IsMyTurn)
            GameData.timerFill.color = newColor.GetRGB();
        OnToggleColors();
    }

    private void OnToggle2D3D(CameraMode mode)
    {
        if (button2D == null || button3D == null || camera2D == null || 
            camera3D == null || canvasGame == null)
        {
            Debug.LogError("Failed to change camera mode, null components." 
                        +"check MenuLogic in inspector");
            return;
        }
        switch (mode)
        {
        case CameraMode.TwoD:
            button2D.interactable = false;
            camera2D.gameObject.SetActive(true);

            button3D.interactable = true;
            camera3D.gameObject.SetActive(false);

            canvasGame.worldCamera = camera2D;
            break;
        case CameraMode.ThreeD:
            button3D.interactable = false;
            camera3D.gameObject.SetActive(true);

            button2D.interactable = true;
            camera2D.gameObject.SetActive(false);
            
            canvasGame.worldCamera = camera3D;
            break;
        default:
            Debug.LogError("Failed to change camera mode, mode = "+mode);
            return;
        }
    }

    #region Sliders Logic

    public void OnChangeSlider(Sliders slider)
    {
        float value;
        if ((value = slider.GetSliderValue()) == -1)
        {
            Debug.LogError($"Failed to change Slider {slider}, value={value}");
            return;
        }
        TextMeshProUGUI text;
        if ((text = slider.GetSliderText()) == null)
        {
            Debug.LogError($"Failed to change Slider {slider}, text={text}");
            return;
        }
        
        switch (slider)
        {
        case Sliders.NumberOfPlayers:
            OnAdjustNumberOfPlayers(value);
            break;
        case Sliders.TurnTimer:
            OnSetTurnTimer(value);
            break;
        case Sliders.BarriersCount:
            OnSetBarriersCount(value);
            break;
        case Sliders.Password:
            OnSetPassword(value);
            break;
        case Sliders.Music:
            OnAdjustMusicVolume(value);
            value = math.ceil(value*10);
            break;
        case Sliders.SFX:
            OnAdjustSFXVolume(value);
            value = math.ceil(value*10);
            break;
        default:
            return;
        }
        text.text = value.ToString();

    }

    private void OnSetPassword(float value)
    {
        GameData.Password = (int)value;
    }

    private void OnAdjustMusicVolume(float value)
    {
        if (AudioManager == null || AudioManager.Music == null)
        {
            Debug.LogError("Failed to Adjust Music Volume, null params");
            return;
        }
        AudioManager.Music.volume = value;
    }
    
    private void OnAdjustSFXVolume(float value)
    {
        if (AudioManager == null || AudioManager.SFX == null)
        {
            Debug.LogError("Failed to Adjust Music Volume, null params");
            return;
        }
        AudioManager.SFX.volume = value;
    }

    private void OnSetTurnTimer(float value)
    {
        GameData.timer.seconds = (int)value;
    }

    private void OnSetBarriersCount(float value)
    {
        GameData.NumOfBarriers = (int)value;
    }

    private void OnAdjustNumberOfPlayers(float value)
    {
        GameData.NumOfPlayers = (int)value;
    }


    #endregion
    
    #endregion

    #region MonoBehaviour

    void OnEnable()
    {
        // Menu
        SC_MenuController.OnChangeScreen += OnChangeScreen;
        SC_MenuController.OnOpenLink += OnOpenLink;
        SC_MenuController.OnChangeSlider += OnChangeSlider;
        SC_MenuController.OnToggleColors += OnToggleColors;
        SC_MenuController.OnChooseColor += OnChooseColor;
        SC_MenuController.OnToggle2D3D += OnToggle2D3D;
        SC_MenuController.OnUpdateCameraPosition += OnUpdateCameraPosition;
        // Game
        SC_GameLogic.OnGameOver += OnGameOver;
    }

    void OnDisable()
    {
        SC_MenuController.OnChangeScreen -= OnChangeScreen;
        SC_MenuController.OnOpenLink -= OnOpenLink;
        SC_MenuController.OnChangeSlider -= OnChangeSlider;
        SC_MenuController.OnToggleColors -= OnToggleColors;
        SC_MenuController.OnChooseColor -= OnChooseColor;
        SC_MenuController.OnToggle2D3D -= OnToggle2D3D;
        SC_MenuController.OnUpdateCameraPosition -= OnUpdateCameraPosition;

        SC_GameLogic.OnGameOver -= OnGameOver;
    }

    private void OnUpdateCameraPosition()
    {
        if (SC_GameData.Instance.myPlayerData != null)
        {
            (camera3D.transform.position, camera3D.transform.rotation) = GlobalConstants.Camera3DPositions[SC_GameData.Instance.myPlayerData.Index];
            (camera2D.transform.position, camera2D.transform.rotation) = GlobalConstants.Camera2DPositions[SC_GameData.Instance.myPlayerData.Index];
        }
    }

    void Awake()
    {
        GameData = SC_GameData.Instance;
    }
    void Start()
    {
        InitLogic();
    }

    #endregion
}
