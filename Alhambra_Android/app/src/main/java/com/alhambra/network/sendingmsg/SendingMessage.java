package com.alhambra.network.sendingmsg;

/** Class that has all the common functionalities between the other class that generates JSON messages to send to the server*/
public class SendingMessage
{
    /** Generate a string have N whitespace
     * @param incr the incrementation to apply. Should be positive
     * @return the string that has 'incr' whitespace*/
    public static String generateIncr(int incr)
    {
        StringBuilder res = new StringBuilder();
        for(int i = 0; i < incr; i++)
            res.append(' ');
        return res.toString();
    }
}
