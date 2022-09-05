using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

#if WINDOWS_UWP
using Windows.System.Threading;
using System.Threading;
#else
using System.Threading;
using UnityEngine;
#endif


/// <summary>
/// The Connection Status enumeration
/// </summary>
public enum ConnectionStatus
{
    CONNECTED,
    RECONNECTED,
    DISCONNECTED
}

public interface IAlhambraServerListener
{
    /// <summary>
    /// Method called when the connection status of the remote tablet has changed
    /// </summary>
    /// <param name="server">The server calling this method</param>
    /// <param name="status">The change in the connection status</param>
    public void OnConnectionStatus(AlhambraServer server, ConnectionStatus status);
}


/// <summary>
/// The Alhambra server.
/// This server handles only one tablet at a time. 
/// If multiple attempts of connection happens, this server only keeps the last one
/// </summary>
public class AlhambraServer : ServerSocket, IServerSocketListener
{
    /// <summary>
    /// The tablet client. Compared to the default settings of ServerSocket, here, we handle only one tablet
    /// </summary>
    private Client m_tabletClient = null;

    /// <summary>
    /// The port to open
    /// </summary>
    public static readonly uint SERVER_PORT = 8080;

    private List<IAlhambraServerListener> m_listeners = new List<IAlhambraServerListener>();

    public AlhambraServer() : base(AlhambraServer.SERVER_PORT)
    {
        AddListener(this);
    }

    /// <summary>
    /// Add a new listener to notify changes
    /// </summary>
    /// <param name="listener">A new listener to send events</param>
    public void AddListener(IAlhambraServerListener listener)
    {
        m_listeners.Add(listener);
    }

    /// <summary>
    /// Remove an already registered listener
    /// </summary>
    /// <param name="listener">The old listener called on events</param>
    public void RemoveListener(IAlhambraServerListener listener)
    {
        m_listeners.Remove(listener);
    }

    //--------------------------------//
    //-----------Listeners------------//
    //--------------------------------//

    public void OnAcceptClient(ServerSocket s, Client c)
    {
        if(m_tabletClient == null)
        {
            m_tabletClient = c;
            foreach (IAlhambraServerListener l in m_listeners)
                l.OnConnectionStatus(this, ConnectionStatus.CONNECTED);
        }

        else
        {
            Client oldTablet = m_tabletClient;
            m_tabletClient = c; //Change m_tabletClient before calling the Close method, because otherwise, the condition in the OnClose method will be true (which we want to avoid)
            oldTablet.Close();

            foreach (IAlhambraServerListener l in m_listeners)
                l.OnConnectionStatus(this, ConnectionStatus.RECONNECTED);
        }
    }

    public override void OnClose(Client c)
    {
        if (m_tabletClient == c)
        {
            m_tabletClient = null;
            foreach (IAlhambraServerListener l in m_listeners)
                l.OnConnectionStatus(this, ConnectionStatus.DISCONNECTED);
        }
        base.OnClose(c);
    }

    public override void OnRead(Client c, String msg)
    {
        if(c == m_tabletClient)
        {
            Debug.Log($"Received: {msg}");
        }
    }

    /// <summary>
    /// Is the tablet connected?
    /// </summary>
    public bool Connected { get => m_tabletClient != null; }
    
    /// <summary>
    /// What is the current tablet client?
    /// </summary>
    public Client TabletClient { get => m_tabletClient; }
}
