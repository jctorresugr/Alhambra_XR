using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class DataLoader
{

    // a quick and dirty csv loader
    public static string[][] CSVLoader(string s, char sep=',')
    {
        string[] lines = s.Split(new char[] { '\n', '\r' });
        int count = 0;
        for(int i=0;i<lines.Length;i++)
        {
            lines[i] = lines[i].Trim();
            if (lines[i].Length > 0)
                count++;
        }
        string[][] results = new string[count][];
        int j = 0;
        for(int i=0;i<lines.Length;i++)
        {
            if (lines[i].Length > 0)
            {
                results[j] = CSVSplit(lines[i],sep);
                j++;
            }
                
        }
        return results;
    }

    private static string[] CSVSplit(string s, char sep=',')
    {
        int i = 0;
        int lastI = 0;
        s += ',';

        bool isInsideQuotes = false;
        List<string> r = new List<string>();
        while(i<s.Length)
        {
            switch(s[i])
            {
                case ',':
                    if(isInsideQuotes)
                    {
                        i++;
                        continue;
                    }
                    //a,bcde,f
                    //01234567
                    //^ last i
                    // ^ cur i
                    string ns = s.Substring(lastI, i - lastI);
                    if(ns.StartsWith("\"") && ns.EndsWith("\""))
                    {
                        ns = ns.Substring(1, ns.Length - 2);
                    }
                    r.Add(ns);
                    lastI = i + 1;
                    break;
                case '\"':
                    isInsideQuotes = !isInsideQuotes;
                    break;
            }
            i++;
        }
        if (isInsideQuotes)
        {
            Debug.LogWarning("Pase csv encounter unclosed quotes for string: " + s.Substring(0, s.Length - 1));
        }
        return r.ToArray();
    }
    
}
