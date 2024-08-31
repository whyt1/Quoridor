using System;
using UnityEngine;

public class SC_MultiplayerController : MonoBehaviour
{
    public static Action OnConnect;
    public static Action OnDisconnect;
    public static Action OnJoin;
    public static Action OnSearch;
    public static Action OnHost;
    public static Action OnNext;
    public static Action OnStartGame;
    public static Action OnRematch;

    public void Btn_Rematch()
    {
        OnRematch?.Invoke();
    }

    public void Btn_StartGame()
    {
        OnStartGame?.Invoke();
    }

    public void Btn_Next()
    {
        OnNext?.Invoke();
    }

    public void Btn_Host()
    {
        OnHost?.Invoke();
    }
    public void Btn_Search()
    {
        OnSearch?.Invoke();
    }
    public void Btn_Connect()
    {
        OnConnect?.Invoke();
    }
    public void Btn_Disconnect()
    {
        OnDisconnect?.Invoke();
    }
    public void Btn_Join()
    {
        OnJoin?.Invoke();
    }
}