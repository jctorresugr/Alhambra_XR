package com.sereno;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.util.ArrayList;
import java.util.List;

/** Class to read basic CSV files
 * The original source code comes from https://stackoverflow.com/questions/5360628/get-and-parse-csv-file-in-android*/
public class CSVReader
{
    /** Read the a CSV input stream using the separator ',' and the delimiter '""
     * @param inputStream the stream to read
     *
     * @return 2D array (row vs. column). Rows may have different sizes*/
    public static List<String[]> read(InputStream inputStream) throws IOException {return read(inputStream, ',', '"');}


    /** Read the a CSV input stream using a custom separator and delimiter
     * @param inputStream the stream to read
     * @param separator  the separator to use, e.g., ','
     * @param delimiter the delimiter to use, e.g., '"'
     *
     * @return 2D array (row vs. column). Rows may have different sizes*/
    public static List<String[]> read(InputStream inputStream, final char separator, final char delimiter) throws IOException
    {
        List<String[]> resultList = new ArrayList<>();
        BufferedReader reader     = new BufferedReader(new InputStreamReader(inputStream));

        String csvLine;
        final char LF = '\n';
        final char CR = '\r';
        boolean quoteOpened = false;
        boolean isEscaped   = false;
        while ((csvLine = reader.readLine()) != null)
        {
            ArrayList<String> a = new ArrayList<>();
            StringBuilder token = new StringBuilder();
            csvLine += separator;

            for(char c : csvLine.toCharArray())
            {
                if(c == LF || c == CR) // not required as we are already read line
                {
                    quoteOpened = false;
                    isEscaped   = false;
                    a.add(token.toString());
                    token.setLength(0);
                }

                else if(c == '\\')
                {
                    if(isEscaped)
                        token.append('\\');
                    isEscaped = !isEscaped;
                }

                else if(c == '#' && !(quoteOpened || isEscaped))
                    break; //Break comments

                else
                {
                    if(isEscaped)
                        token.append(c);
                    else if(c == delimiter)
                        quoteOpened=!quoteOpened;
                    else if(c == separator)
                    {
                        if(!quoteOpened)
                        {
                            a.add(token.toString());
                            token.setLength(0);
                        }
                        else
                        {
                            token.append(c);
                        }
                    }
                    else
                    {
                        token.append(c);
                    }
                    isEscaped = false;
                }
            }

            if(a.size()>0)
            {
                if(resultList.size()>0)
                {
                    String[] row = new String[0];
                    row = a.toArray(row);
                    resultList.add(row);
                }
                else
                {
                    String[] row = new String[0];
                    row = a.toArray(row);
                    resultList.add(row);
                }
            }
        }
        inputStream.close();
        return resultList;
    }
}