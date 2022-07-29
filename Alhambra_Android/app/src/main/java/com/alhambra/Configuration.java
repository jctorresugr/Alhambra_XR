package com.alhambra;

import android.util.Log;

import java.io.File;
import java.io.FileInputStream;
import java.nio.charset.StandardCharsets;

import org.json.JSONException;
import org.json.JSONObject;


/** This class permits to read the configuration file to define some parameters*/
public class Configuration
{
    /** The server IP address*/
    private String m_serverIP;

    /** The server port address*/
    private int    m_serverPort;

    /** Constructor, initialize the IP and the port to 127.0.0.1:8080*/
    public Configuration()
    {
        m_serverIP   = "127.0.0.1";
        m_serverPort = 8080;
    }

    /** Constructor, read the JSON configuration file.
     *
     * The JSON should follow the following template
     *
     * {
     *     "network": {
     *         "ip": "127.0.0.1",
     *         "port": 8080
     *     }
     * }*/
    public Configuration(File f)
    {
        this();

        try
        {
            //Read the file
            FileInputStream fis = new FileInputStream(f);
            byte[] jsonData = new byte[(int) f.length()];
            fis.read(jsonData);
            fis.close();

            //Parse it
            JSONObject reader = new JSONObject(new String(jsonData, StandardCharsets.UTF_8));

            //Parse Network
            try
            {
                JSONObject ntwk = reader.getJSONObject("network");
                m_serverIP = ntwk.getString("ip");
                m_serverPort = ntwk.getInt("port");
            }
            catch (final JSONException e)
            {
                Log.e(MainActivity.TAG, "Error at parsing network configuration");
            }
        }
        catch(Exception e)
        {
            return;
        }
    }

    /** Get the server IP address to connect to
     * @return the string representing the IP address in the format of 255.255.255.255*/
    public String getServerIP()
    {
        return m_serverIP;
    }

    /** Get the server port as described in the configuration file
     * @return the server port to connect to*/
    public int getServerPort()
    {
        return m_serverPort;
    }
}
