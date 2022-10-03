package com.alhambra.network;

import android.util.Log;

import com.alhambra.MainActivity;

import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.InetAddress;
import java.net.InetSocketAddress;
import java.net.Socket;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.nio.charset.StandardCharsets;
import java.util.ArrayDeque;
import java.util.ArrayList;

public class SocketManager
{
    public interface ISocketManagerListener
    {
        /** Function called when the socket has been disconnected
         * @param socket The SocketManager calling this Method*/
        void onDisconnection(SocketManager socket);

        /** Function called when a new incoming JSON message as been received
         * @param socket The SocketManager calling this Method
         * @param jsonMsg  The received JSON message*/
        void onRead(SocketManager socket, String jsonMsg);

        /** Function called when the socket has been reconnected
         * @param socket The SocketManager calling this Method*/
        void onReconnection(SocketManager socket);
    }

    /** A valid read*/
    private static class ReadValue
    {
        /** Is the value valid?*/
        boolean valid = false;
        /** The new buffer offset to apply*/
        int     bufOff = 0;
    }

    /** The Int32 value read*/
    private static class ReadInt32 extends ReadValue
    {
        /** The value*/
        int value;
    }

    /** The String value read*/
    private static class ReadString extends ReadValue
    {
        /** The value*/
        String  value;
    }


    /** The connection timeout in milliseconds*/
    public static final int CONNECT_TIMEOUT    = 200;
    /** How many milliseconds the thread has to sleep before reattempting to connect ?*/
    public static final int FAIL_CONNECT_SLEEP = 200;
    /** How many milliseconds the thread has to sleep before resending data ?*/
    public static final int THREAD_SLEEP       = 1000/90;
    /** How many milliseconds the thread has to sleep before reading again potential incoming data ?*/
    public static final int READ_TIMEOUT       = 0;


    /** The socket object for communicating with the server*/
    private Socket           m_socket;
    /** The output stream of the socket*/
    private DataOutputStream m_output;
    /** The input stream of the socket*/
    private InputStream      m_input;

    /** The ip of the server (here: the hololens)*/
    private String m_serverIP;
    /** The port of the server*/
    private int    m_serverPort;

    /** The socket's write thread*/
    private final Thread  m_writeThread;
    /** The socket's read thread. This thread also handles disconnections.*/
    private final Thread  m_readThread;
    /** Is the socket closed ?*/
    private boolean m_isClosed = false;

    /***************************************************************/
    /***** HELPER FUNCTIONS FOR READING INCOMING JSON AS STRING*****/
    /***************************************************************/

    /** The current size of the JSON message to read*/
    private int m_curJsonSize = -1;
    /** The buffer internal position*/
    private int m_dataPos = 0;
    /** The current buffer in construction usually for an Int*/
    private byte[] m_data = new byte[4];
    /** Special string buffer for strings*/
    private StringBuilder m_stringBuf  = null;


    /** The queue buffer storing data to SEND*/
    private ArrayDeque<byte[]> m_queueSendBuf = new ArrayDeque<>();

    /** List of listener to call when the socket status changes*/
    private ArrayList<ISocketManagerListener> m_listeners = new ArrayList<>();

    /** Runnable writing to the socket. It also tries to reconnect to the server every time*/
    private Runnable m_writeThreadRunnable = new Runnable()
    {
        @Override
        public void run()
        {
            while(!m_isClosed)
            {
                boolean isConnected;
                synchronized(this)
                {
                    isConnected = m_socket.isConnected();
                }

                //If not connected, sleep
                if(!isConnected)
                {
                    try {Thread.sleep(SocketManager.FAIL_CONNECT_SLEEP);} catch (Exception e) {}
                    continue;
                }

                if(!checkWriting())
                    continue;

                try{Thread.sleep(SocketManager.THREAD_SLEEP);} catch (Exception e) {}
            }
        }
    };

    /** Runnable reading the socket*/
    private Runnable m_readThreadRunnable = new Runnable()
    {
        @Override
        public void run()
        {
            byte[] buf = new byte[65536];

            while(!m_isClosed)
            {

                //Connect the socket
                boolean isConnected;
                InputStream input;
                synchronized(this)
                {
                    isConnected = m_socket.isConnected();

                    //Check the connection
                    if (!isConnected)
                        isConnected = connect();
                    input = m_input;
                }

                try
                {
                    //Read the data
                    if(isConnected && input != null)
                    {
                        int readSize = input.read(buf);
                        if (readSize > 0) {
                            int curOffset = 0;
                            while(curOffset < readSize)
                            {
                                ReadString readString = readString(buf, curOffset, readSize);
                                curOffset = readString.bufOff;
                                if(readString.valid)
                                {
                                    synchronized(this)
                                    {
                                        for (ISocketManagerListener l : m_listeners)
                                            l.onRead(SocketManager.this, readString.value);
                                    }
                                }
                            }
                        }
                        else //EOF
                            close();
                    }
                    else
                    {
                        Thread.sleep(THREAD_SLEEP);
                        continue;
                    }
                }
                catch(Exception e)
                {
                    synchronized (this)
                    {
                        close();
                    }
                }
            }
        }
    };

    /** Constructor. Initialize the Client Socket and tries to connect to "ip":"port"
     * @param ip the server IP address
     * @param port the server port to connect to*/
    public SocketManager(String ip, int port)
    {
        m_socket      = new Socket();
        m_input       = null;
        m_output      = null;
        m_serverIP    = ip;
        m_serverPort  = port;
        m_writeThread = new Thread(m_writeThreadRunnable);
        m_writeThread.start();

        m_readThread = new Thread(m_readThreadRunnable);
        m_readThread.start();
    }

    /** Add a listener object to call when the internal states of this socket changes
     * @param l the new listener to take account of*/
    public void addListener(ISocketManagerListener l)
    {
        if(!m_listeners.contains(l))
            m_listeners.add(l);
    }

    /** Remove a listener object to call when the internal states of this socket changes
     * @param l the new listener to take account of*/
    public void removeListener(ISocketManagerListener l)
    {
        m_listeners.remove(l);
    }

    /** Set the server address
     * @param ip the server ip
     * @param port the server port*/
    public synchronized void setServerAddr(String ip, int port)
    {
        m_serverIP   = ip;
        m_serverPort = port;
        if(m_input != null) { //Prefer to close the input stream to let the thread "read" handle the connection
            try {m_input.close();}
            catch(IOException e) {}
        }
    }

    /** Close the socket*/
    public synchronized void close()
    {
        if(m_socket.isConnected())
            try{m_socket.close();} catch(Exception e){}

        m_dataPos     = 0;
        m_curJsonSize = -1;
        m_queueSendBuf.clear();
        m_output = null;
        m_input  = null;
        m_socket = new Socket();

        try{m_socket.setReuseAddress(true);}catch(Exception e){}

        for(ISocketManagerListener l : m_listeners)
            l.onDisconnection(this);
    }

    /** Connect to the server. We put this into a separate function for letting the thread connecting to the server
     * (non-blocking connection)
     *
     * Do not connect more than once if the boolean has returned true
     * The function "run" called this method.
     *
     * @return true on success, false on failure*/
    private boolean connect()
    {
        try
        {
            //We may want to close everything just in case
            //Indeed, the stream output or input may have been the issue into the last attempt
            //In can also be an error from the main application
            if(m_input != null || m_output != null || m_socket.isConnected())
                close();

            m_socket.connect(new InetSocketAddress(InetAddress.getByName(m_serverIP), m_serverPort), CONNECT_TIMEOUT);
            m_socket.setSoTimeout(READ_TIMEOUT);
            m_output = new DataOutputStream(m_socket.getOutputStream());
            m_input  = m_socket.getInputStream();

            for(ISocketManagerListener l : m_listeners)
                l.onReconnection(this);
        }
        catch(Exception e)
        {
            m_socket = new Socket();
            m_output = null;
            m_input  = null;
            return false;
        }

        return true;
    }

    /** Push a new value to write to the server
     * @param data array of bytes to write to the server*/
    public synchronized void push(byte[] data)
    {
        m_queueSendBuf.add(data);
    }

    /** Push a new string to send via the network. This method will generate (1) a 4-bytes integer to state the size of the string, and (2) the string in itself using a default UTF-8 conversion
     * @param str the string to send*/
    public synchronized void push(String str)
    {
        ByteBuffer buf = ByteBuffer.allocate(4 + str.length());
        buf.order(ByteOrder.BIG_ENDIAN);
        buf.putInt(str.length());
        buf.put(str.getBytes(StandardCharsets.UTF_8));

        push(buf.array());
    }

    /** Check the writing part of the client
     * @return true if no error occurred, false otherwise*/
    private boolean checkWriting()
    {
        OutputStream output = null;
        synchronized (this) {
            output = m_output;
        }
        if(output == null)
            return false;

        //Send buffers. These buffer are generate from the static method this class provides
        while(true)
        {
            byte[] d;
            synchronized(this) {
                if(m_queueSendBuf.isEmpty())
                    break;
                d = m_queueSendBuf.poll();
            }

            try
            {
                output.write(d);
                output.flush();
            }
            catch(final Exception e)
            {
                //close();
                return false;
            }
        }
        return true;
    }

    /** Read 32 bits integer in the incoming byte
     * @param data the incoming data
     * @param offset the offset in the data array
     * @param readSize the size of the data (initial size)
     * @return the value read*/
    private ReadInt32 readInt32(byte[] data, int offset, int readSize)
    {
        ReadInt32 val = new ReadInt32();
        val.bufOff = offset;
        val.valid  = false;

        if(readSize + m_dataPos - offset < 4)
        {
            for(int i = offset; i < readSize; i++, val.bufOff++)
                m_data[m_dataPos++] = data[i];
            return val;
        }

        for(int i = offset; m_dataPos < 4; m_dataPos++, i++, val.bufOff++)
            m_data[m_dataPos] = data[i];

        val.value = (int)(((m_data[0] & 0xff) << 24) +
                ((m_data[1] & 0xff) << 16) +
                ((m_data[2] & 0xff) << 8) +
                (m_data[3] & 0xff));
        val.valid = true;
        m_dataPos = 0;
        return val;
    }


    /** Read String (32 bits + n bits) in the incoming byte
     * @param data the incoming data
     * @param offset the offset in the data array
     * @param readSize the size of the data (initial size)
     * @return the value read*/
    private ReadString readString(byte[] data, int offset, int readSize)
    {
        ReadString val = new ReadString();
        val.valid  = false;
        val.bufOff = offset;

        if(m_curJsonSize < 0)
        {
            ReadInt32 stringSize = readInt32(data, offset, readSize);
            val.bufOff = stringSize.bufOff;
            if(!stringSize.valid)
                return val;

            m_curJsonSize = stringSize.value;
            if(m_curJsonSize < 0)
            {
                Log.e(MainActivity.TAG, "Received a string buffer size inferior than 0... Treat it as 0");
                m_curJsonSize = 0;
            }
            m_stringBuf  = new StringBuilder(m_curJsonSize);
        }
        while(m_stringBuf.length() != m_curJsonSize && val.bufOff < readSize)
        {
            m_stringBuf.append((char)data[val.bufOff++]);
        }

        if(m_stringBuf.length() == m_curJsonSize)
        {
            val.valid = true;
            val.value = m_stringBuf.toString();
            m_curJsonSize = -1;
        }
        return val;
    }
}