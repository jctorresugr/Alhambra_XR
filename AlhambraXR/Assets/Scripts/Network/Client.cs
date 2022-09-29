using System;
using System.Collections.Generic;
using System.Net.Sockets;

#if WINDOWS_UWP
using Windows.System.Threading;
using System.Threading;
#else
using System.Threading;
#endif


public interface IClientListener
{
    /// <summary>
    /// Function called when a client is closed.
    /// This method can only be called on attempts to write data or after the Close() method is called
    /// Note: the Socket is closed just after this event is fired
    /// </summary>
    /// <param name="c"></param>
    void OnClose(Client c);

    /// <summary>
    /// Function called when a new String message has been received
    /// </summary>
    /// <param name="c">The client calling this method</param>
    /// <param name="msg">The received message</param>
    void OnRead(Client c, String msg);
}

/// <summary>
/// Client connected with this Server
/// </summary>
public class Client
{
    /// <summary>
    /// The Socket of the client
    /// </summary>
    private Socket                m_socket;

    /// <summary>
    /// Should we close this socket?
    /// The closing happens in the writing thread
    /// </summary>
    private bool m_shouldClose = false;

    /// <summary>
    /// The separate thread to write data
    /// </summary>
    private Thread                m_writeThread;

    /// <summary>
    /// The data to send to the socket
    /// </summary>
    private Queue<byte[]>         m_writeBuffer = new Queue<byte[]>();

    /// <summary>
    /// The listeners of this client
    /// </summary>
    private List<IClientListener> m_listeners   = new List<IClientListener>();

    /// <summary>
    /// Position along the next byte array to parse
    /// </summary>
    private int m_byteArrayPos = -1;

    /// <summary>
    /// The byte array getting parsed
    /// </summary>
    private byte[] m_byteArray = null;

    /// <summary>
    /// The data array in construction to parse an integer (Uint32)
    /// </summary>
    private byte[] m_data = new byte[4];

    /// <summary>
    /// The current position of the in-construction m_data array
    /// </summary>
    private int m_dataPos = 0;

    /// <summary>
    /// The number of time in micro seconds that the reading and writing threads sleep if no data is to be sent/read
    /// </summary>
    public const int THREAD_SLEEP = 50;

    /// <summary>
    /// Constructor, initialize a connected client
    /// </summary>
    /// <param name="s">The socket associated with the remote client</param>
    public Client(Socket s)
    {
        m_socket                = s;
        m_socket.NoDelay        = true;
        m_socket.Blocking       = false;
        m_writeThread           = new Thread(WriteThread);
    }

    /// <summary>
    /// Add a new listener to notify changes
    /// </summary>
    /// <param name="listener">A new listener to send events</param>
    public void AddListener(IClientListener listener)
    {
        m_listeners.Add(listener);
    }

    /// <summary>
    /// Remove an already registered listener
    /// </summary>
    /// <param name="listener">The old listener called on events</param>
    public void RemoveListener(IClientListener listener)
    {
        m_listeners.Remove(listener);
    }

    /// <summary>
    /// Enqueue a new data to write
    /// </summary>
    /// <param name="data">A data to write</param>
    public void EnqueueData(byte[] data)
    {
        lock(this)
            m_writeBuffer.Enqueue(data);
    }

    /// <summary>
    /// Close the connection with the remote client.
    /// This function does nothing if this.Closed == true
    /// This function is asynchronous: The closing happens in a separate thread. Listen for this.Closed (or the OnClose event) to check when the socket will actually be closed.
    /// </summary>
    public void Close()
    {
        lock (this)
        {
            m_shouldClose = true;
        }
    }

    /// <summary>
    /// Push a new message to read. Warning: this method is not thread safe
    /// </summary>
    /// <param name="data">The incoming data to read</param>
    /// <param name="size">The actual readable size of the buffer</param>
    /// <param name="offset">The offset inside the buffer</param>
    public void PushRead(byte[] data, int size, int offset=0)
    {
        while (ReadString(data, size, offset, out String s, out int newOffset))
        {
            offset = newOffset;
            lock (this)
            {
                foreach (IClientListener l in m_listeners)
                    l.OnRead(this, s);
            }
        }
    }

    /// <summary>
    /// The thread to handle writeable data
    /// </summary>
    private void WriteThread()
    {
        while(true)
        {
            lock(this)
            {
                if(m_shouldClose)
                {
                    foreach (IClientListener l in m_listeners)
                        l.OnClose(this);
                    m_socket.Close();
                    return;
                }
                if(Closed)
                    return;
            }

            byte[] data = null;
            lock(this)
            {
                if (m_writeBuffer.Count > 0)
                    data = m_writeBuffer.Dequeue();
            }
            if (data != null)
            {
                try
                {
                    m_socket.Send(data);
                }
                catch(SocketException e)
                {
                    Console.Error.WriteLine($"Received error socket message: {e.Message}, ErrorCode: {e.ErrorCode}.");
                    m_socket.Close();
                    lock(this)
                    {
                        foreach (IClientListener l in m_listeners)
                            l.OnClose(this);    
                    }
                }
            }

            if(data == null)
                Thread.Sleep(THREAD_SLEEP);
        }
    }

    /// <summary>
    /// Read a string from the current buffer
    /// </summary>
    /// <param name="data">The incoming buffer</param>
    /// <param name="size">The size to read in the incoming buffer</param>
    /// <param name="offset">Offset of the buffer</param>
    /// <param name="val">The value parsed (if any)</param>
    /// <param name="newOff">The new offset to apply</param>
    /// <returns>true if enough data was pushed to parsed a string value, false otherwise</returns>
    private bool ReadString(byte[] data, int size, int offset, out String s, out Int32 newOffset)
    {
        s = "";
        if (!ReadByteArray(data, size, offset, out byte[] stringData, out newOffset))
            return false;

        s = System.Text.Encoding.ASCII.GetString(stringData);
        return true;
    }

    /// <summary>
    /// Read a byte array from the current buffer
    /// </summary>
    /// <param name="data">The incoming buffer</param>
    /// <param name="size">The size to read in the incoming buffer</param>
    /// <param name="offset">Offset of the buffer</param>
    /// <param name="val">The value parsed (if any)</param>
    /// <param name="newOffset">The new offset to apply</param>
    /// <returns>true if enough data was pushed to parsed a byte array value, false otherwise</returns>
    private unsafe bool ReadByteArray(byte[] data, int size, int offset, out byte[] arr, out Int32 newOffset)
    {
        newOffset = offset;
        arr = null;

        if (m_byteArray == null) //Read the size component
        {
            if (!ReadInt32(data, size, newOffset, out m_byteArrayPos, out newOffset))
                return false;
            m_byteArray = new byte[m_byteArrayPos];
            m_byteArrayPos = 0;
        }

        fixed (byte* pData = data, pByteArray = m_byteArray)
        {
            for (; m_byteArrayPos < m_byteArray.Length && newOffset < size; m_byteArrayPos++, newOffset++)
                pByteArray[m_byteArrayPos] = pData[newOffset];
        }

        if (m_byteArrayPos == m_byteArray.Length)
        {
            arr = m_byteArray;
            m_byteArray = null;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Read a Int32 from the current buffer
    /// </summary>
    /// <param name="data">The incoming buffer</param>
    /// <param name="size">The size to read in the incoming buffer</param>
    /// <param name="offset">Offset of the buffer</param>
    /// <param name="val">The value parsed (if any)</param>
    /// <param name="newOff">The new offset to apply</param>
    /// <returns>true if enough data was pushed to parsed a Int32 value, false otherwise. If false: val is not modified.</returns>
    private bool ReadInt32(byte[] data, int size, int offset, out Int32 val, out int newOff)
    {
        val = 0;
        newOff = offset;
        if (size + m_dataPos - offset < 4)
        {
            for (int i = offset; i < size; i++, newOff++)
                m_data[m_dataPos++] = data[i];
            return false;
        }

        for (; m_dataPos < 4; newOff++)
            m_data[m_dataPos++] = data[newOff];

        val = (Int32)(((m_data[0] & 0xff) << 24) +
                      ((m_data[1] & 0xff) << 16) +
                      ((m_data[2] & 0xff) << 8) +
                      (m_data[3] & 0xff));
        m_dataPos = 0;
        return true;
    }

    /// <summary>
    /// Is the connection between the server and the remote client closed?
    /// </summary>
    public bool Closed { get => !m_socket.Connected; }

    /// <summary>
    /// Get the socket associated with this Client. Do not close it!
    /// </summary>
    public Socket Socket { get => m_socket; }
}
