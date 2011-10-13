﻿using System;
using System.Collections.Generic;
using CJRGaming.MMO.Common;
using ExitGames.Client.Photon;
using UnityEngine;

public class PhotonEngine : MonoBehaviour, IPhotonPeerListener
{
    public PhotonPeer Peer { get; protected set; }

    public GameState State { get; protected set; }

    public ViewController Controller { get; set; }

    private static PhotonEngine _instance;

    public PhotonEngine()
    {
        _instance = this;
        State = new Disconnected(this);
        Initialize("localhost:5055", "AegisBorn");
    }

    public static PhotonEngine Instance
    {
       get
       {
           if(_instance == null)
           {
               _instance = new PhotonEngine();
           }
           return _instance;
       }
    }

    #region Implementation of IPhotonPeerListener

    public void DebugReturn(DebugLevel level, string message)
    {
        Controller.DebugReturn(level, message);
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        Controller.OnOperationResponse(operationResponse);
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        switch (statusCode)
        {
            case StatusCode.Connect:
                {
                    Peer.EstablishEncryption();
                    break;
                }
            case StatusCode.Disconnect:
            case StatusCode.DisconnectByServer:
            case StatusCode.DisconnectByServerLogic:
            case StatusCode.DisconnectByServerUserLimit:
            case StatusCode.TimeoutDisconnect:
            case StatusCode.Exception:
                {
                    Controller.OnDisconnected("" + statusCode);
                    State = new Disconnected(this);
                    break;
                }
            case StatusCode.EncryptionEstablished:
                {
                    State = new Connected(this);
                    break;
                }
            default:
                {
                    Controller.OnUnexpectedStatusCode(statusCode);
                    break;
                }
        }

    }

    public void OnEvent(EventData eventData)
    {
        Controller.OnEvent(eventData);
    }

    #endregion

    public void Initialize(string serverAddress, string applicationName)
    {
        Peer = new PhotonPeer(this, false);
        Peer.Connect(serverAddress, applicationName);
        State = new WaitingForConnection(this);
    }

    public void Disconnect()
    {
        if (Peer != null)
        {
            Peer.Disconnect();
        }
    }

    public void Update()
    {
        State.OnUpdate();
    }

    public void SendOp(OperationCode operationCode, Dictionary<byte, object> parameters, bool sendReliable, byte channelId, bool encrypt)
    {
        State.SendOperation(operationCode, parameters, sendReliable, channelId, encrypt);
    }

}
