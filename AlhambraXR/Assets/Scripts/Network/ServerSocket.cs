using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

#if WINDOWS_UWP
using Windows.System.Threading;
using System.Threading;
#else
using System.Threading;
#endif

/// <summary>
/// The interface containing methods to call on events for ServerSocket objects
/// </summary>
public interface IServerSocketListener
{
    /// <summary>
    /// Function called when a new client is connected to the server. Listen to this client if you are interested in the data it is sending, or when this client is closing
    /// </summary>
    /// <param name="s">The server socket accepting a new client</param>
    /// <param name="c">The new accepted client</param>
    public void OnAcceptClient(ServerSocket s, Client c);
}

/// <summary>
/// Basic class for TCP/IP Server. All messages are sent as soon as possible (NoDelay)
/// This object opens one thread for accepting incoming connection, one to read their data, and N threads for writing data, N == clients.Length
/// </summary>
public class ServerSocket: IClientListener
{
    /// <summary>
    /// The server socket
    /// </summary>
    private Socket m_socket = null;

    /// <summary>
    /// The port to open
    /// </summary>
    private readonly uint m_port;

    /// <summary>
    /// The thread used to accept new Client
    /// </summary>
    private Thread m_acceptThread = null;

    /// <summary>
    /// The thread used to handle data received by the Client
    /// </summary>
    private Thread m_readThread = null;
    
    /// <summary>
    /// Is this Server closed ?
    /// </summary>
    private bool m_closed = true;

    /// <summary>
    /// The listeners
    /// </summary>
    private List<IServerSocketListener> m_listeners = new List<IServerSocketListener>();

    /// <summary>
    /// List of connected clients
    /// </summary>
    private Dictionary<Socket, Client> m_clients = new Dictionary<Socket, Client>();

    /// <summary>
    /// The number of time in micro seconds that the reading and writing threads sleep if no data is to be sent/read
    /// </summary>
    private const int THREAD_SLEEP = 50;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="port">Which port to listen?</param>
    public ServerSocket(uint port)
    {
        m_port = port;
    }

    /// <summary>
    /// Launch the server. This function can only be called if this.Launched == false
    /// </summary>
    public void Launch()
    {
        if (!m_closed)
            return;
        
        m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        m_socket.Blocking = false;
        m_socket.NoDelay  = true;
        m_socket.ReceiveTimeout = THREAD_SLEEP;
        m_closed = false;

        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, (int)m_port);
        m_socket.Bind(localEndPoint);
        m_socket.Listen(5);

        m_acceptThread = new Thread(AcceptThread);
        m_acceptThread.Start();
        m_readThread = new Thread(ReadThread);
        m_readThread.Start();
    }

    /// <summary>
    /// Close asynchronously the server
    /// </summary>
    /// <returns>An asynchronous task</returns>
    public Task CloseAsync()
    {
        return Task.Run(() => Close());
    }

    /// <summary>
    /// Close the server connection. Can only be called if this.Closed == false
    /// If this function is called while from the OnRead listener, the whole application is in a blocking state.
    /// Use "CloseAsync" to close the server while in the OnRead method.
    /// </summary>
    public void Close()
    {
        if (m_closed)
            return;
        m_closed   = true; //First set m_closed at true to stop all threads
        m_acceptThread.Join();
        m_readThread.Join();
        if(m_socket != null)
            m_socket.Close();

        //The clients should all be closed now, as we closed the server socket
        foreach(Client c in m_clients.Values)
            c.RemoveListener(this);
        m_clients.Clear(); 

        m_socket = null;
    }

    /// <summary>
    /// Wait for the server to close
    /// This function does nothing if the server is not launched
    /// </summary>
    public void Wait()
    {
        while(!m_closed) //This while is unecessary as the Join will already verify this condition. But we are never too secure...
        {
            m_acceptThread.Join();
            m_readThread.Join();
        }
    }

    /// <summary>
    /// Add a new listener to notify changes
    /// </summary>
    /// <param name="listener">A new listener to send events</param>
    public void AddListener(IServerSocketListener listener)
    {
        m_listeners.Add(listener);
    }

    /// <summary>
    /// Remove an already registered listener
    /// </summary>
    /// <param name="listener">The old listener called on events</param>
    public void RemoveListener(IServerSocketListener listener)
    {
        m_listeners.Remove(listener);
    }

    /// <summary>
    /// Send a data to all clients
    /// </summary>
    /// <param name="d">The data to send</param>
    public void SendDataToClients(byte[] d)
    {
        lock(this)
            foreach (Client c in m_clients.Values)
                c.EnqueueData(d);
    }

    /// <summary>
    /// Send a string data to all clients. The byte array data sent is constructed as follow:
    /// 4 bytes: Size of the string (big endian)
    /// n bytes: the ASCII byte caracters of the string
    /// </summary>
    /// <param name="s">The string to send.</param>
    public void SendASCIIStringToClients(String s)
    {
        byte[] data = new byte[4 + s.Length];
        Encoding.ASCII.GetBytes(s, 0, s.Length, data, 4);
        data[0] = (byte)(s.Length >> 24);
        data[1] = (byte)(s.Length >> 16);
        data[2] = (byte)(s.Length >> 8);
        data[3] = (byte)s.Length;
        SendDataToClients(data);
    }

    /// <summary>
    /// The thread that accepts new connection
    /// </summary>
    private void AcceptThread()
    {
        while(!m_closed)
        {
            try
            {
                if(!m_socket.Poll(THREAD_SLEEP, SelectMode.SelectRead))
                    continue;
                Socket s = m_socket.Accept();
                Client c = new Client(s);
                c.AddListener(this);

                lock (this)
                {
                    m_clients.Add(s, c);
                    foreach (IServerSocketListener l in m_listeners)
                        l.OnAcceptClient(this, c);
                }
            }
            catch(SocketException e)
            {
                Console.WriteLine($"Error in the server socket: {e.Message}. Error code: {e.ErrorCode}");
            }
        }
    }

    /// <summary>
    /// The thread that reads incoming data
    /// </summary>
    private void ReadThread()
    {
        while(!m_closed)
        {
            List<Socket> readableClients = null;
            
            lock (this)
            {
                readableClients = new List<Socket>(from client in m_clients select client.Key);
            }
            if(readableClients.Count == 0)
            {
                Thread.Sleep(THREAD_SLEEP);
                continue;
            }
            try
            {
                byte[] data = new byte[1024];
                Socket.Select(readableClients, null, null, THREAD_SLEEP);
                foreach (Socket s in readableClients)
                {
                    int size = 0;
                    lock (s)
                    {
                        if(!s.Connected) //The socket can be disposed at this point...
                            continue;
                        size = s.Receive(data);
                    }
                    if (size == 0) //Socket is closed
                    {
                        OnClose(m_clients[s]);
                        continue;
                    }
                    m_clients[s].PushRead(data, size);
                }
            }
            catch(SocketException e)
            {
                Console.WriteLine($"Error while reading a socket client: {e.Message}. Error code: {e.ErrorCode}");
            }
        }
    }

    //-------------------------------//
    //--------IClientListener--------//
    //-------------------------------//

    public virtual void OnClose(Client c)
    {
        lock(this)
            m_clients.Remove(c.Socket);
    }

    public virtual void OnRead(Client c, String msg)
    {}

    /// <summary>
    /// Is the server closed?
    /// </summary>
    public bool Closed { get => m_closed; }

    /// <summary>
    /// Get the first available IP address to join this server.
    /// </summary>
    public static String DeviceServerAddress
    {
        get
        {
            IPHostEntry HostEntry = Dns.GetHostEntry((Dns.GetHostName()));
            if (HostEntry.AddressList.Length > 0)
            {
                foreach (IPAddress ip in HostEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            return "";
        }
    }

    /// <summary>
    /// Get the list of all available IP address to join this serer
    /// </summary>
    public static List<String> DeviceServerAddresses
    {
        get
        {
            List<String> ret = new List<String>();
            IPHostEntry HostEntry = Dns.GetHostEntry((Dns.GetHostName()));
            if (HostEntry.AddressList.Length > 0)
            {
                foreach (IPAddress ip in HostEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ret.Add(ip.ToString());
                    }
                }
            }
            return ret;
        }
    }
}
