using System;
using System.Collections;
using System.Collections.Generic;
using MyQuoridorApp;
using UnityEngine;

public class SC_AudioLogic : MonoBehaviour
{
    #region Variables
    [Header("Audio Sources")]
    public AudioSource Music;
    public AudioSource SFX;

    [Header("Sound Clips")]
    public AudioClip musicClip;
    public List<AudioClip> sfxMenuClip;
    public List<AudioClip> sfxPawnClip;
    public List<AudioClip> sfxBarrierMovingClip;
    public List<AudioClip> sfxBarrierPlacedClip;

    #region Events
    void OnGameOver() { PlaySFX(sfxMenuClip); }
    void OnToggle2D3D(CameraMode mode) { PlaySFX(sfxMenuClip); }
    void OnChooseColor(ColorOptions options) { PlaySFX(sfxMenuClip); }
    void OnToggleColors() { PlaySFX(sfxMenuClip); }
    void OnChangeSlider(Sliders sliders) { PlaySFX(sfxMenuClip); }
    void OnOpenLink(string obj) { PlaySFX(sfxMenuClip); }
    void OnChangeScreen(Screens screens) { PlaySFX(sfxMenuClip); }
    private void OnUnDo() { PlaySFX(sfxMenuClip); }
    private void OnStartGame() { PlaySFX(sfxMenuClip); }

    private void OnBarrierMoved(string _) { PlaySFX(sfxBarrierMovingClip); }
    private void OnBarrierPlaced(SC_BarrierLogic _) { PlaySFX(sfxBarrierPlacedClip); }
    private void OnSquareClick(Square _) { PlaySFX(sfxPawnClip); }
    #endregion
    #endregion

    #region Logic
    void PlaySFX(List<AudioClip> currClips)
    {
        if (SFX == null || currClips == null || currClips.Count < 1)
        {
            Debug.LogError("Failed to Play SFX, make sure all fields are assigned in inspector!");
            return;
        }
        int i = UnityEngine.Random.Range(0, currClips.Count);
        SFX.PlayOneShot(currClips[i]);
    }
    #endregion

    #region MonoBehaviour
    void OnEnable()
    {       
        SC_SquareLogic.OnSquareClick += OnSquareClick;
        
        SC_BarrierLogic.OnBarrierPlaced += OnBarrierPlaced;
        SC_BarrierLogic.OnBarrierMoved += OnBarrierMoved;

        SC_MenuController.OnStartGame += OnStartGame;
        SC_MenuController.OnUnDo += OnUnDo;
        SC_MenuController.OnChangeScreen += OnChangeScreen;
        SC_MenuController.OnOpenLink += OnOpenLink;
        // SC_MenuController.OnChangeSlider += OnChangeSlider;
        SC_MenuController.OnToggleColors += OnToggleColors;
        SC_MenuController.OnChooseColor += OnChooseColor;
        SC_MenuController.OnToggle2D3D += OnToggle2D3D;

        SC_GameLogic.OnGameOver += OnGameOver;
    }

    void OnDisable()
    {
        SC_SquareLogic.OnSquareClick -= OnSquareClick;
        
        SC_BarrierLogic.OnBarrierPlaced -= OnBarrierPlaced;
        SC_BarrierLogic.OnBarrierMoved -= OnBarrierMoved;
        
        SC_MenuController.OnStartGame -= OnStartGame;
        SC_MenuController.OnUnDo -= OnUnDo;
        SC_MenuController.OnChangeScreen -= OnChangeScreen;
        SC_MenuController.OnOpenLink -= OnOpenLink;
        // SC_MenuController.OnChangeSlider -= OnChangeSlider;
        SC_MenuController.OnToggleColors -= OnToggleColors;
        SC_MenuController.OnChooseColor -= OnChooseColor;
        SC_MenuController.OnToggle2D3D -= OnToggle2D3D;

        SC_GameLogic.OnGameOver -= OnGameOver;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Music == null || musicClip == null)
        {
            Debug.LogError("Failed to start audio, make sure all fields are assigned in inspector!");
            return;
        }
        Music.clip = musicClip;
        Music.loop = true;
        Music.Play();
    }
    #endregion
}
